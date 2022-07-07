using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicDialogues
{
    internal class Parser
    {
        /// <summary>
        /// If the NPC is in the required location, return true. Defaults to true if location is any/null.
        /// </summary>
        /// <param name="who"> The NPC to check.</param>
        /// <param name="place">The place to use for comparison.</param>
        /// <returns></returns>
        internal static bool InRequiredLocation(NPC who, GameLocation place)
        {

            if (who.currentLocation == place)
            {
                return true;
            }
            else if (place is null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// For validating user additions. Passes the values to another bool, then returns that result.
        /// </summary>
        /// <param name="which">The raw dialogue data to check.</param>
        /// <returns></returns>
        internal static bool IsValid(RawDialogues[] which)
        {
            try
            {
                return ReturnValidity(which);
            }
            catch (Exception ex)
            {
                ModEntry.Mon.Log($"Error found in contentpack: {ex}", StardewModdingAPI.LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Check raw data. If no errors are found, returns true.
        /// </summary>
        /// <param name="which">The raw data to check.</param>
        /// <returns></returns>
        internal static bool ReturnValidity(RawDialogues[] which)
        {
            Dictionary<string, string> timeAndPlace = new();
            var fix = which.ToList();

            foreach (var array in which)
            {
                //the list is used just to get index, honestly.
                int arrayPos = fix.IndexOf(array);

                if (array.IsBubble == "true" && array.Emote is not "-1")
                {
                    ModEntry.Mon.Log("Configs \"IsBubble\" and \"Emote\" are mutually exclusive (the two can't be applied at the same time). Patch will not be loaded.", LogLevel.Error);
                    return false;
                }
                if (array.Time == "any" && array.Location == "any")
                {
                    ModEntry.Mon.Log($"You must either set an hour or a location. (Addition number {arrayPos})");
                    return false;
                }
                else
                {
                    int time = int.Parse(array.Time);

                    if (time <= 600 || time >= 2600)
                    {
                        ModEntry.Mon.Log($"Addition number {arrayPos} has a faulty hour!", LogLevel.Warn);
                        return false;
                    }
                }
                timeAndPlace.Add(array.Time, array.Location);
            }

            var result = timeAndPlace.Count == timeAndPlace.Distinct().Count();
            if (result == false)
            {
                ModEntry.Mon.Log($"There are duplicates in this list!", LogLevel.Error);
                ModEntry.Mon.Log($"timeAndPlace.Count = {timeAndPlace.Count}; timeAndPlace.Distinct().Count() = {timeAndPlace.Distinct().Count()}");
            }

            return result;
        }
        /// <summary>
        /// Check if NPC exists. If null or not in friendship data, returns false.
        /// </summary>
        /// <param name="who"> The NPC to check.</param>
        /// <returns></returns>
        internal static bool Exists(string who) //rename to CharacterExists
        {
            var monitor = ModEntry.Mon;
            var admitted = ModEntry.AdmittedNPCs;
            var character = Game1.getCharacterFromName(who);

            if (character is null)
            {
                monitor.Log($"NPC {who} could not be found! See log for more details.", LogLevel.Error);
                monitor.Log($"NPC {who} returned null when calling  Game1.getCharacterFromName({who}).");
                return false;
            }

            if (!admitted.Contains(character.Name))
            {
                monitor.Log($"NPC {who} is not in characters! Did you type their name correctly?", LogLevel.Warn);
                monitor.Log($"NPC {who} seems to exist, but wasn't found in the list of admitted NPCs. This may occur if you haven't met them yet, or if they haven't been unlocked.");
                return false;
            }

            return true;
        }
        internal static bool Exists(NPC who)
        {
            var monitor = ModEntry.Mon;
            var admitted = ModEntry.AdmittedNPCs;

            if (who is null)
            {
                monitor.Log($"NPC {who} could not be found! See log for more details.", LogLevel.Error);
                monitor.Log($"NPC {who} returned null when calling  Game1.getCharacterFromName({who}).");
                return false;
            }

            if (!admitted.Contains(who.Name))
            {
                monitor.Log($"NPC {who} is not in characters! Did you type their name correctly?", LogLevel.Warn);
                monitor.Log($"NPC {who} seems to exist, but wasn't found in the list of admitted NPCs. This may occur if you haven't met them yet, or if they haven't been unlocked.");
                return false;
            }

            return true;
        }
        /// <summary>
        /// Checks validity of greeting patch (ie. existing NPC and dialogue)
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="dialogue"></param>
        /// <returns></returns>
        internal static bool IsValidGreeting(NPC chara, string dialogue)
        {
            if (chara is null)
            {
                ModEntry.Mon.Log("Character couldn't be found.");
                return false;
            }
            if (String.IsNullOrWhiteSpace(dialogue))
            {
                ModEntry.Mon.Log("There's no dialogue!");
                return false;
            }

            return true;
        }
        /// <summary>
        /// Converts a raw list to its parsed equivalent.
        /// </summary>
        /// <param name="raws">The raw list to convert.</param>
        /// <returns></returns>
        internal static List<ParsedDialogues> GetParseds(List<RawDialogues> raws)
        {
            List<ParsedDialogues> result = new();

            foreach (var array in raws)
            {
                result.Add(new ParsedDialogues(array));
            }

            return result;
        }
        internal static List<ParsedDialogues> GetParseds(RawDialogues[] raws)
        {
            List<ParsedDialogues> result = new();

            foreach (var array in raws)
            {
                result.Add(new ParsedDialogues(array));
            }

            return result;
        }

        internal static string FormatBubble(string which)
        {
            string result = which;

            var rawspan = which.AsSpan();
            if (rawspan.Contains<char>('@'))
            {
                result = which.Replace("@", Game1.player?.Name);
            }

            return result;
        }
    }
}