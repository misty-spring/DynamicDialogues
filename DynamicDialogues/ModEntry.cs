using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using static DynamicDialogues.Parser;
using static DynamicDialogues.Getter;
using System.Linq;

namespace DynamicDialogues
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += SaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStart;
            helper.Events.GameLoop.TimeChanged += OnTimeChange;

            helper.Events.GameLoop.ReturnedToTitle += OnTitleReturn;
            helper.Events.Content.AssetRequested += OnAssetRequest;

            this.Config = this.Helper.ReadConfig<ModConfig>();
            Mon = this.Monitor;

            this.Monitor.Log($"Applying Harmony patch \"{nameof(Patches)}\": prefixing SDV method \"NPC.sayHiTo(Character)\".");
            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.NPC), nameof(StardewValley.NPC.sayHiTo)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.SayHiTo_Prefix))
                );
        }

        private void SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var allNPCs = this.Helper.GameContent.Load<Dictionary<string, string>>("Data\\NPCDispositions");
            NPCDispositions = allNPCs.Keys.ToList();

            // For each string: Check if npc has been met, to not cause errors with locked/unmet NPCs.
            GetFriendedNPCs();
            this.Monitor.Log($"Found {NPCDispositions?.Count ?? 0} characters in NPC dispositions.");
            this.Monitor.Log($"Found {PatchableNPCs?.Count ?? 0} characters in friendship data.");
        }

        private void OnDayStart(object sender, DayStartedEventArgs e)
        {
            //clear temp data
            ClearTemp();
            GetFriendedNPCs();

            //get dialogue via NPCs in "framework" data
            foreach (var name in PatchableNPCs)
            {
                if (!Exists(name)) //!AdmittedNPCs.Contains(name) || 
                {
                    this.Monitor.Log($"{name} data won't be added. Check log for more details.", LogLevel.Warn);
                    continue;
                }
                if(Config.Verbose)
                {
                    this.Monitor.Log($"Checking patch data for NPC {name}...");
                }
                var CompatRaw = Game1.content.Load<Dictionary<string, RawDialogues>>($"mistyspring.dynamicdialogues/Dialogues/{name}");
                GetNPCDialogues(CompatRaw, name);
            }
            this.Monitor.Log($"Loaded {Dialogues?.Count ?? 0} user patches. (Dialogues)");

            //get greetings
            var greetRaw = Game1.content.Load<Dictionary<string, Dictionary<string, string>>>("mistyspring.dynamicdialogues/Greetings");
            foreach (var edit in greetRaw)
            {
                NPC mainCh = Game1.getCharacterFromName(edit.Key);
                if (!Exists(mainCh))
                {
                    continue;
                }

                this.Monitor.Log($"Loading greetings for {edit.Key}...");
                Dictionary<NPC, string> ValueOf = new();

                foreach (var npcgreet in edit.Value)
                {
                    this.Monitor.Log($"Checking greet data for {npcgreet.Key}...");
                    var chara = Game1.getCharacterFromName(npcgreet.Key);

                    if (IsValidGreeting(chara, npcgreet.Value))
                    {
                        Greetings.Add((edit.Key, npcgreet.Key), npcgreet.Value);
                        this.Monitor.Log("Greeting added.");
                    }
                }
            }
            this.Monitor.Log($"Loaded {Greetings?.Count ?? 0} user patches. (Greetings)");
            
            //get notifs
            var notifRaw = Game1.content.Load<Dictionary<string, RawNotifs>>("mistyspring.dynamicdialogues/Notifs");
            foreach (var pair in notifRaw)
            {
                var notif = pair.Value;
                if (IsValidNotif(notif))
                {
                    ModEntry.Mon.Log($"Notification \"{pair.Key}\" parsed successfully.");
                    Notifs.Add(notif);
                }
                else
                {
                    ModEntry.Mon.Log($"Found error in \"{pair.Key}\" while parsing, check Log for details.", LogLevel.Error);
                }
            }
            this.Monitor.Log($"Loaded {Notifs?.Count ?? 0} user patches. (Notifs)");

        }

        private void OnTimeChange(object sender, TimeChangedEventArgs e)
        {
            foreach (var patch in Dialogues)
            {
                foreach (var d in patch.Value)
                {
                    //"conditional" variable checks if patch has already been added. if so, returns. if not (and conditions apply), adds it to patch so it won't be re-checked.
                    var conditional = (patch.Key, d.Time, d.Location);
                    if ((bool)(AlreadyPatched?.Contains(conditional)))
                    {
                        this.Monitor.LogOnce($"Dialogue {conditional} has already been used today. Skipping...");
                        continue;
                    }

                    if(Config.Verbose)
                    { 
                        this.Monitor.Log($"Checking dialogue with key {conditional}..."); 
                    }

                    var chara = Game1.getCharacterFromName(patch.Key);
                    var inLocation = InRequiredLocation(chara, d.Location);
                    var timeMatch = d.Time.Equals(e.NewTime);

                    if(Config.Debug)
                    {
                        this.Monitor.Log($" inLocation = {inLocation}; timeMatch = {timeMatch}");
                    }

                    if ((timeMatch && inLocation) || (d.Time == -1 && inLocation) || (timeMatch && d.Location is "any"))
                    {
                        if(Config.Verbose)
                        {
                            this.Monitor.Log("Conditions match. Applying...");
                        }

                        //get facing direction if any
                        var facing = ReturnFacing(d.FaceDirection);

                        /* Extra options: 
                         * if any emote, do it. 
                         * if shake is greater than 0, shake. 
                         * if jump is true, make npc jump
                         * if facedirection isn't -1, set facedirection
                         */
                        if (d.Emote >= 0)
                        {
                            this.Monitor.Log($"Doing emote for {patch.Key}. Index: {d.Emote}");
                            chara.doEmote(d.Emote);
                        }
                        if (d.Shake > 0)
                        {
                            this.Monitor.Log($"Shaking {patch.Key} for {d.Shake} milliseconds.");
                            chara.shake(d.Shake);
                        }
                        if (d.Jump)
                        {
                            this.Monitor.Log($"{patch.Key} will jump..");
                            chara.jump();
                        }
                        if (facing != -1)
                        {
                            this.Monitor.Log($"Changing {patch.Key} facing direction to {d.FaceDirection}.");
                            chara.faceDirection(facing);
                        }

                        /* If its supposed to be a bubble, put the dialogue there. If not, proceed as usual. */
                        if (d.IsBubble)
                        {
                            this.Monitor.Log($"Adding text as bubble.");
                            chara.showTextAboveHead(FormatBubble(d.Dialogue));
                        }
                        else
                        {
                            //if the user wants to override current dialogue, this will do it.
                            if (d.Override)
                            {
                                this.Monitor.Log($"Clearing {patch.Key} dialogue.");
                                chara.CurrentDialogue.Clear();
                            }

                            //if should be immediate. ie not wait for npc to pass by
                            if (d.Immediate)
                            {
                                //if npc in location OR force true
                                if (Game1.player.currentLocation.Name == d.Location || d.Force)
                                {
                                    Game1.drawDialogue(chara, d.Dialogue);
                                }
                            }
                            else
                            {
                                //set new dialogue, log to trace
                                chara.setNewDialogue(d.Dialogue, true, d.ClearOnMove);
                            }
                        }
                        this.Monitor.Log($"Adding dialogue for {patch.Key} at {e.NewTime}, in {chara.currentLocation}");

                        /* List is checked daily, but removing causes errors in the foreach loop.
                         * So, there'll be a list with today's already added values (tuple of NPC name, time, location)
                        */
                        AlreadyPatched.Add(conditional);
                    }
                }
            }
            foreach (var notif in Notifs)
            {
                int pos = Notifs.IndexOf(notif);
                //we use notif+index since those aren't tied to a npc.
                var conditional = ($"notification-{pos}", notif.Time, notif.Location);

                if ((bool)(AlreadyPatched?.Contains(conditional)))
                {
                    this.Monitor.LogOnce($"Key {conditional} has already been used today. Skipping...");
                    continue;
                }

                this.Monitor.Log($"Checking notif with key {conditional}...");
                var cLoc = Game1.player.currentLocation;
                var inLocation = notif.Location == cLoc.Name;
                var timeMatch = notif.Time.Equals(e.NewTime);
                
                if(Config.Debug)
                {
                    this.Monitor.LogOnce($"Player name: {Game1.player.Name}");
                    this.Monitor.Log($"cLoc.Name = {cLoc.Name} ; inLocation = {inLocation}; timeMatch = {timeMatch}");
                }

                if ((timeMatch && inLocation) || (notif.Time == -1 && inLocation) || (timeMatch && notif.Location is "any"))
                {
                    this.Monitor.Log($"Adding notif for player at {e.NewTime}, in {cLoc.Name}");
                    if (notif.IsBox)
                    {
                        Game1.drawObjectDialogue(notif.Message);
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(notif.Sound))
                        {
                            Game1.soundBank.PlayCue(notif.Sound);
                        }

                        Game1.showGlobalMessage(notif.Message);
                    }

                    AlreadyPatched.Add(conditional);
                }
            }
        }

        private void OnTitleReturn(object sender, ReturnedToTitleEventArgs e)
        {
            ClearTemp();
            NPCDispositions?.Clear();
        }

        private void OnAssetRequest(object sender, AssetRequestedEventArgs e)
        {
            //list of admitted NPCs - deprecated but still here jic
            if (e.NameWithoutLocale.IsEquivalentTo("mistyspring.dynamicdialogues/NPCs", true))
            {
                e.LoadFrom(
                () => new List<string>(),
                AssetLoadPriority.Medium
            );
            }
            
            //each NPC file
            foreach (var name in NPCDispositions) //NPCsToPatch
            {
                if (e.NameWithoutLocale.IsEquivalentTo($"mistyspring.dynamicdialogues/Dialogues/{name}", true))
                {
                    e.LoadFrom(
                    () => new Dictionary<string, RawDialogues>(),
                    AssetLoadPriority.Medium
                    );
                }
            }

            //greetings
            if (e.NameWithoutLocale.IsEquivalentTo("mistyspring.dynamicdialogues/Greetings", true))
            {
                e.LoadFrom(
                () => new Dictionary<string, Dictionary<string, string>>(),
                AssetLoadPriority.Medium
            );
            }

            //notifs
            if (e.NameWithoutLocale.IsEquivalentTo("mistyspring.dynamicdialogues/Notifs", true))
            {
                e.LoadFrom(
                () => new Dictionary<string, RawNotifs>(),
                AssetLoadPriority.Medium
                );
            }
        }

        /* Methods used to get dialogues 
         * do NOT change unless bug-fixing is required
         */
        private void GetNPCDialogues(Dictionary<string, RawDialogues> raw, string nameof)
        {
            foreach (var singular in raw)
            {
                var dialogueInfo = singular.Value;
                if (dialogueInfo is null)
                {
                    this.Monitor.Log($"The dialogue data for {nameof} is empty!", LogLevel.Warn);
                }
                else if (IsValid(dialogueInfo, nameof))
                {
                    this.Monitor.Log($"Dialogue key \"{singular.Key}\" ({nameof}) parsed successfully. Adding to dictionary");
                    var data = dialogueInfo;

                    if ((bool)(Dialogues?.ContainsKey(nameof)))
                    {
                        Dialogues[nameof].Add(data);
                    }
                    else
                    {
                        var list = new List<RawDialogues>();
                        list.Add(data);
                        Dialogues.Add(nameof, list);
                    }
                }
                else
                {
                    this.Monitor.Log($"Patch '{singular.Key}' won't be added.", LogLevel.Warn);
                }
            }
        }
        private void GetFriendedNPCs()
        {
            foreach (var name in NPCDispositions)
            {
                if(Config.Debug)
                {
                    this.Monitor.Log($"Checking {name}...");
                }

                NPC npc = Game1.getCharacterFromName(name);
                if (npc is not null)
                {
                    PatchableNPCs.Add(name);
                }
                else if (Config.Verbose)
                {
                    this.Monitor.Log($"NPC {name} doesn't exist in save yet.");
                }
            }
        }
        private void ClearTemp()
        {
            AlreadyPatched?.Clear();
            Dialogues?.Clear();
            Greetings?.Clear();
            Notifs?.Clear();
            PatchableNPCs?.Clear();
        }

        /* Required by mod to work */
        internal static Dictionary<string, List<RawDialogues>> Dialogues { get; private set; } = new();
        internal static Dictionary<(string, string), string> Greetings { get; private set; } = new();
        internal static List<RawNotifs> Notifs { get; private set; } = new();

        internal static List<string> PatchableNPCs { get; private set; } = new();
        internal static List<string> NPCDispositions { get; private set; } = new();

        internal static List<(string, int, string)> AlreadyPatched = new();
        internal static IMonitor Mon { get; private set; }
        private ModConfig Config;
    }
}