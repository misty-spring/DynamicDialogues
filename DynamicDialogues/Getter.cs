using StardewValley;
using System;

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
    }
}
