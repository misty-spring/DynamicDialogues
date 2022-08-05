using StardewValley;
using System;
using System.Collections.Generic;

namespace DynamicDialogues
{
    internal class Getter
    {
        /// <summary>
        /// Formats the bubble set by user. "@" is replaced by player name.
        /// </summary>
        /// <param name="which">The dialogue to check.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the FaceDirection string to int, since it also accepts up/down/etc values.
        /// </summary>
        /// <param name="which">The string to check.</param>
        /// <returns></returns>
        internal static int ReturnFacing(string which)
        {
            if (String.IsNullOrWhiteSpace(which))
            {
                return -1;
            }
            var word = which.ToLower();

            if (word is "up")
            { return 0; }

            if (word is "right")
            { return 1; }

            if (word is "down")
            { return 2; }

            if (word is "left")
            { return 3; }

            int toInt = int.Parse(which);
            if (toInt >= 0 && toInt <= 3)
            {
                return toInt;
            }

            return -1;
        }

        /// <summary>
        /// Returns a string with all questions/answers a character can give.
        /// </summary>
        /// <param name="QAs">List of q data.</param>
        /// <returns></returns>
        internal static string QuestionDialogue(List<RawQuestions> QAs, NPC who)
        {
            //$y seems to be infinite version of question. so it's used for this
            string result = "$y '";
            foreach(var extra in QAs)
            {
                if(extra.From > Game1.timeOfDay || extra.To < Game1.timeOfDay)
                {
                    continue;
                }
                if(extra.Location is not "any" && extra.Location.Equals(who.currentLocation.Name) == false)
                {
                    continue;
                }

                if(extra.MaxTimesAsked > 0)
                {
                    int count = 0;
                    if (ModEntry.QuestionCounter.ContainsKey(extra.Question))
                    {
                        count = ModEntry.QuestionCounter[extra.Question];
                    }
                    else
                    {
                        ModEntry.QuestionCounter.Add(extra.Question, 0);
                    }

                    if (count < extra.MaxTimesAsked)
                    {
                        result += $"_{extra.Question}_{extra.Answer}";
                        ModEntry.QuestionCounter[extra.Question]++;
                    }
                }
                else
                {
                    result += $"_{extra.Question}_{extra.Answer}";
                }
            }
            result += "'";
            return result;
        }

        internal static int GetIndex(Dictionary<string, RawQuestions> dict, string name)
        {
            int position = 0;
            foreach(string key in dict.Keys)
            {
                if(key.Equals(name))
                {
                    return position;
                }

                position++;
            }

            throw new ArgumentOutOfRangeException(); //not in dict
        }

        internal static int[] FramesForAnimation(string data)
        {
            var AsList = data.Split(' ');
            var result = new List<int>();

            foreach(var item in AsList)
            {
                var parsedItem = int.Parse(item);
                result.Add(parsedItem);
            }

            return result.ToArray();
        }
    }
}
