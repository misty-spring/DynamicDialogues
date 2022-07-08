using StardewModdingAPI;
using StardewValley;
using System;
using System.Text.RegularExpressions;

namespace DynamicDialogues
{
    internal class Compat
    {
        /// <summary>
        /// Checks if entry is valid. Compat (<=1.5) version, due to CP limitations.
        /// </summary>
        /// <param name="data">The RawDialogues to check.</param>
        /// <param name="who">The NPC the data is for. (Used to check if they already have an entry in the dictionary, and make the respective checks if so)</param>
        /// <returns></returns>
        internal static bool IsValidAndAddable(RawDialogues data, NPC who)
        {
            try
            {
                var time = data.Time;

                //check if text is bubble and if emotes are allowed. if so return false
                if (data.IsBubble == true && data.MakeEmote == true && data.Emote is not -1)
                {
                    ModEntry.Mon.Log("Configs \"IsBubble\" and \"Emote\" are mutually exclusive (the two can't be applied at the same time). Patch will not be loaded.", LogLevel.Error);
                    return false;
                }

                //if array time is greater than 0 and location is any, return false
                if (time <= 0 && data.Location == "any")
                {
                    ModEntry.Mon.Log($"You must either set an hour or a location.");
                    return false;
                }

                //if time is greater than 0 but not allowed value, return false
                else if (time > 0 && (time <= 600 || time >= 2600))
                {
                    ModEntry.Mon.Log($"Addition has a faulty hour!", LogLevel.Warn);
                    return false;
                }

                //if set to change facing, check value. if less than 0 and bigger than 3 return false
                if (data.ChangeFacing == true)
                {
                    if (data.FaceDirection < 0 || data.FaceDirection > 3)
                    {
                        ModEntry.Mon.Log($"Addition has a faulty facedirection! Value must be between 0 and 3.", LogLevel.Warn);
                        return false;
                    }
                }

                if (ModEntry.Dialogues.ContainsKey(who))
                {
                    foreach(var addition in ModEntry.Dialogues[who])
                    {
                        if(addition.Time == data.Time && addition.Location.ToString() == data.Location)
                        {
                            ModEntry.Mon.Log($"An entry with the values Time={data.Time} and Location={data.Location} already exists. Skipping.", LogLevel.Warn);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ModEntry.Mon.Log($"Error found in contentpack: {ex}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Removes any numbers from a key, and returns the NPC name.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static NPC GetName(string key)
        {
            var name = Regex.Replace(key,"[0 - 9]", "");
            var nameof = Game1.getCharacterFromName(name);
            return nameof;
        }
    }
}