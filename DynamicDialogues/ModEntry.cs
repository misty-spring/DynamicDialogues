using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
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
            Additions?.Clear();

            /* Get raw dialogue
             * Foreach check if NPC exists. 
             * If so, check that the array isnt empty and confirm its values
             * If all is good, add to parsed dict*/
            var DialogueRaw = Game1.content.Load<Dictionary<string, RawDialogues[]>>("mistyspring.dynamicdialogue/Dialogues");
            foreach (var singular in DialogueRaw)
            {
                if (Exists(singular.Key))
                {
                    var dialogueInfo = singular.Value;
                    if (dialogueInfo is null)
                    {
                        this.Monitor.Log($"The dialogue list for {singular.Key} is empty!", LogLevel.Warn);
                    }
                    else if (IsValid(dialogueInfo))
                    {
                        this.Monitor.Log($"Dialogue for {singular.Key} parsed successfully. Adding to dictionary");

                        var nameof = Game1.getCharacterFromName(singular.Key);
                        if (nameof is not null)
                        {
                            Additions.Add(nameof, GetParseds(dialogueInfo));
                        }
                        else
                        {
                            this.Monitor.Log("Something went wrong when getting NPC from Game1. Make sure the NPC exists in your game. (Entry will be skipped)", LogLevel.Warn);
                        }
                    }
                    else
                    {
                        this.Monitor.Log($"{singular.Key}'s patch won't be added.", LogLevel.Warn);
                    }
                }
            }

            /* Get raw greetings. do a foreach on the dictionary
             * If npc doesnt exist, continue with next value in foreach
             * check the values and if all is good, add to Greeting
             */
            var greetRaw = Game1.content.Load<Dictionary<string, Dictionary<string, string>>>("mistyspring.dynamicdialogue/Greetings");
            foreach (var edit in greetRaw)
            {
                NPC mainCh = Game1.getCharacterFromName(edit.Key);
                if (!Exists(mainCh))
                {
                    continue;
                }

                this.Monitor.Log($"Checking greetings for edit.Key...");
                Dictionary<NPC, string> ValueOf = new();

                foreach (var npcgreet in edit.Value)
                {
                    this.Monitor.Log($"Checking greeting for {npcgreet.Key}...");
                    var chara = Game1.getCharacterFromName(npcgreet.Key);

                    if (IsValidGreeting(chara, npcgreet.Value))
                    {
                        Greetings.Add((mainCh, chara), npcgreet.Value);
                        this.Monitor.Log("Greeting added.");
                    }
                }
            }
        }

        private void OnTimeChange(object sender, TimeChangedEventArgs e)
        {
            foreach (var patch in Additions)
            {
                foreach (var d in patch.Value)
                {
                    if (d.Time.Equals(e.NewTime) || InRequiredLocation(patch.Key, d.Location))
                    {
                        /* Extra options: 
                         * if any emote, do it. 
                         * if shake is greater than 0, shake. 
                         * if jump is true, make npc jump
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

                        // remove value so we don't re-add it by accident
                        // list is checked daily, so this is no problem.
                        patch.Value.Remove(d);
                        this.Monitor.Log("Removed value from today's list.");
                    }
                }
            }
        }

        private void OnTitleReturn(object sender, ReturnedToTitleEventArgs e)
        {
            Greetings?.Clear();
            Additions?.Clear();
            AdmittedNPCs?.Clear();
        }

        private void OnAssetRequest(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("mistyspring.dynamicdialogue/Dialogues", true))
            {
                e.LoadFrom(
                () => new Dictionary<string, RawDialogues[]>(),
                AssetLoadPriority.Medium
            );
            }
            if (e.NameWithoutLocale.IsEquivalentTo("mistyspring.dynamicdialogue/Greetings", true))
            {
                e.LoadFrom(
                () => new Dictionary<string, Dictionary<string, string>>(),
                AssetLoadPriority.Medium
            );
            }
        }

        internal static Dictionary<NPC, List<ParsedDialogues>> Additions { get; private set; } = new();
        internal static Dictionary<(NPC, NPC), string> Greetings { get; private set; } = new();

        internal static List<string> AdmittedNPCs { get; private set; } = new();
        internal static IMonitor Mon { get; private set; }
    }
}