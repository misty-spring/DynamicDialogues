using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using static DynamicDialogues.Parser;

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
            /* Only uncomment if having formatting issues
             * This will print a template of RawDialogue[], and one of the supposed dictionary.
             * (GreetingTemplate doesn't need a template, because its just strings)
             
            this.Helper.Data.WriteJsonFile("RawDialogue.json", new RawDialogues());

            Dictionary<string, RawDialogues[]> dictTemplate = new();
            dictTemplate.Add("NAME", new RawDialogues[1] { new RawDialogues() });
            dictTemplate.Add("NAME2", new RawDialogues[2] { new RawDialogues(), new RawDialogues()});
            this.Helper.Data.WriteJsonFile("DialogueTemplate.json", dictTemplate);

            //another test, with written data
            RawDialogues[] test = new[] { new RawDialogues("test") };
            DialogueRaw.Add("TEST", test);
            DialogueRaw.Add("TEST2", test);
            this.Helper.Data.WriteJsonFile<Dictionary<string, RawDialogues[]>>("FromDS.json", DialogueRaw);

            //another type of test. writes list with data
            var data = new Dictionary<string, List<RawDialogues>>();
            var temp = new RawDialogues("template");
            var one = new List<RawDialogues>();
            one.Add(temp);
            var two = one;
            two.Add(temp);
            two.Add(temp);
            data.Add("NAME", one);
            data.Add("NAME2", two);
            this.Helper.Data.WriteJsonFile<Dictionary<string, List<RawDialogues>>>("FromAssetReqL.json", data);
            */

            var allNPCs = this.Helper.GameContent.Load<Dictionary<string, string>>("Data\\NPCDispositions");

            // For each string: Check if it's in NPC friendship list, to not cause errors with locked/unmet NPCs.
            foreach (var name in allNPCs.Keys)
            {
                if (Game1.player.friendshipData.ContainsKey(name))
                {
                    AdmittedNPCs.Add(name);
                }
            }
            this.Monitor.Log($"Found {AdmittedNPCs?.Count ?? 0} characters in friendship data.");
        }

        private void OnDayStart(object sender, DayStartedEventArgs e)
        {
            ClearTemp();

            /* Get raw dialogue
             * Foreach check if NPC exists. 
             * If so, check that the array isnt empty and confirm its values
             * If all is good, add to parsed dict
             * 
             * For now, there's two dialogue files: one for <=1.5 patches, and one for 1.6 patches. So, backwards-compatible
             * (just remember to make a post about porting when 1.6 is out)
             */
            GetCompatDialogues();

            this.Monitor.Log($"Loaded {Dialogues?.Count ?? 0} user patches. (Dialogues)");

            /* Get raw greetings. do a foreach on the dictionary
             * If npc doesnt exist, continue with next value in foreach
             * check the values and if all is good, add to Greeting
             */
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
                        Greetings.Add((mainCh, chara), npcgreet.Value);
                        this.Monitor.Log("Greeting added.");
                    }
                }
            }
            this.Monitor.Log($"Loaded {Greetings?.Count ?? 0} user patches. (Greetings)");
        }

        private void OnTimeChange(object sender, TimeChangedEventArgs e)
        {
            foreach (var patch in Dialogues)
            {
                foreach (var d in patch.Value)
                {
                    //"conditional" variable checks if patch has already been added. if so, returns. if not (and conditions apply), adds it to patch so it won't be re-checked.
                    var conditional = (patch.Key.Name, d.Time, d.Location?.ToString());
                    if((bool)(AlreadyPatched?.Contains(conditional)))
                    {
                        continue;
                    }

                    if (d.Time.Equals(e.NewTime) || InRequiredLocation(patch.Key, d.Location))
                    {
                        /* Extra options: 
                         * if any emote, do it. 
                         * if shake is greater than 0, shake. 
                         * if jump is true, make npc jump
                         * if facedirection isn't -1, set facedirection
                         */
                        if (d.Emote >= 0)
                        {
                            this.Monitor.Log($"Doing emote for {patch.Key.Name}. Index: {d.Emote}");
                            patch.Key.doEmote(d.Emote);
                        }
                        if (d.Shake > 0)
                        {
                            this.Monitor.Log($"Shaking {patch.Key.Name} for {d.Shake} milliseconds.");
                            patch.Key.shake(d.Shake);
                        }
                        if (d.Jump)
                        {
                            this.Monitor.Log($"{patch.Key.Name} will jump..");
                            patch.Key.jump();
                        }
                        if(d.FaceDirection != -1)
                        {
                            this.Monitor.Log($"Changing {patch.Key.Name} facing direction to {d.FaceDirection}.");
                            patch.Key.faceDirection(d.FaceDirection);
                        }

                        /* If its supposed to be a bubble, put the dialogue there. If not, proceed as usual. */
                        if (d.IsBubble)
                        {
                            this.Monitor.Log($"Adding text as bubble.");
                            patch.Key.showTextAboveHead(FormatBubble(d.Dialogue));
                        }
                        else
                        {
                            //if the user wants to override current dialogue, this will do it.
                            if (d.Override)
                            {
                                this.Monitor.Log($"Clearing {patch.Key.Name} dialogue.");
                                patch.Key.CurrentDialogue.Clear();
                            }

                            //set new dialogue, log to trace
                            patch.Key.setNewDialogue(d.Dialogue, true, d.ClearOnMove);
                        }
                        this.Monitor.Log($"Adding dialogue for {patch.Key} at {e.NewTime}, in {patch.Key.currentLocation}");

                        /* List is checked daily, but removing causes errors in the foreach loop.
                         * So, there'll be a list with today's already added values (tuple of NPC name, time, location)
                        */
                        AlreadyPatched.Add(conditional);
                    }
                }
            }
        }

        private void OnTitleReturn(object sender, ReturnedToTitleEventArgs e)
        {
            ClearTemp();
            AdmittedNPCs?.Clear();
        }

        private void OnAssetRequest(object sender, AssetRequestedEventArgs e)
        {
            //for 1.6
            if (e.NameWithoutLocale.IsEquivalentTo("mistyspring.dynamicdialogues/Dialogues", true))
            {
                e.LoadFrom(
                () => new Dictionary<string, RawDialogues[]>(),
                AssetLoadPriority.Medium
            );
            }
            //for 1.5 and...earlier?
            if (e.NameWithoutLocale.IsEquivalentTo("mistyspring.dynamicdialogues/DialoguesCompat", true))
            {
                e.LoadFrom(
                () => new Dictionary<string, RawDialogues>(),
                AssetLoadPriority.Medium
            );
            }

            //greetings work regardless of version
            if (e.NameWithoutLocale.IsEquivalentTo("mistyspring.dynamicdialogues/Greetings", true))
            {
                e.LoadFrom(
                () => new Dictionary<string, Dictionary<string, string>>(),
                AssetLoadPriority.Medium
            );
            }
        }

        /* Methods used to get dialogues 
         * do NOT change unless bug-fixing is required
         */
        private void GetCompatDialogues()
        {
            var CompatRaw = Game1.content.Load<Dictionary<string, RawDialogues>>("mistyspring.dynamicdialogues/DialoguesCompat");

            foreach (var singular in CompatRaw)
            {
                this.Monitor.Log($"Checking patch for NPC {singular.Key}...");
                NPC nameof = Compat.GetName(singular.Key);
                if (Exists(nameof))
                {
                    var dialogueInfo = singular.Value;
                    if (dialogueInfo is null)
                    {
                        this.Monitor.Log($"The dialogue data for {singular.Key} is empty!", LogLevel.Warn);
                    }
                    else if (Compat.IsValidAndAddable(dialogueInfo, nameof))
                    {
                        this.Monitor.Log($"Dialogue for {singular.Key} parsed successfully. Adding to dictionary");
                        var data = new ParsedDialogues(dialogueInfo);

                        if ((bool)(Dialogues?.ContainsKey(nameof)))
                        {
                            Dialogues[nameof].Add(data);
                        }
                        else
                        {
                            var list = new List<ParsedDialogues>();
                            list.Add(data);
                            Dialogues.Add(nameof, list);
                        }
                    }
                    else
                    {
                        this.Monitor.Log($"{singular.Key}'s patch won't be added.", LogLevel.Warn);
                    }
                }
                else
                {
                    this.Monitor.Log($"{singular.Key} data won't be added. Check log for more details.", LogLevel.Warn);
                }
            }
        }
        private void GetDialogues()
        {
            //1.6 doesn't exist yet but im leaving here for when it eventually becomes a thing
            var DialogueRaw = Game1.content.Load<Dictionary<string, RawDialogues[]>>("mistyspring.dynamicdialogues/Dialogues");

            foreach (var singular in DialogueRaw)
            {
                this.Monitor.Log($"Checking patch for NPC {singular.Key}...");
                var nameof = Game1.getCharacterFromName(singular.Key);

                if (Exists(nameof))
                {
                    var dialogueInfo = singular.Value;
                    if (dialogueInfo is null)
                    {
                        this.Monitor.Log($"The dialogue list for {singular.Key} is empty!", LogLevel.Warn);
                    }
                    else if (IsValid(dialogueInfo))
                    {
                        this.Monitor.Log($"Dialogue for {singular.Key} parsed successfully. Adding to dictionary");
                        try
                        {
                            Dialogues.Add(nameof, GetParseds(dialogueInfo));
                        }
                        catch (Exception ex)
                        {
                            this.Monitor.Log($"Something went wrong when getting NPC from Game1. Make sure the NPC exists in your game. (Entry will be skipped). Details: {ex}", LogLevel.Warn);
                        }
                    }
                    else
                    {
                        this.Monitor.Log($"{singular.Key}'s patch won't be added.", LogLevel.Warn);
                    }
                }
                else
                {
                    this.Monitor.Log($"{singular.Key} data won't be added. Check log for more details.", LogLevel.Warn);
                }
            }
        }
        private void ClearTemp()
        {
            AlreadyPatched?.Clear();
            Dialogues?.Clear();
            Greetings?.Clear();
        }

        /* Required by mod to work */
        internal static Dictionary<NPC, List<ParsedDialogues>> Dialogues { get; private set; } = new();
        internal static Dictionary<(NPC, NPC), string> Greetings { get; private set; } = new();

        internal static List<string> AdmittedNPCs { get; private set; } = new();
        internal static List<(string, int, string)> AlreadyPatched = new();
        internal static IMonitor Mon { get; private set; }
    }
}