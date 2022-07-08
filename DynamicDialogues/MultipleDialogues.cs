using StardewModdingAPI;
using StardewValley;
using System;

namespace DynamicDialogues
{
    /*
     * impl. ideas:
     * - face towards farmer
     * - wait to stop moving before applying dialogue
     *      (smth like :
     *          ApplyWhenMoving defaulting to true,
     *          or WaitToStopMoving which defaults to false
     *      )
     * I may (or may not) add these in the future
     */

    /// <summary>
    /// A user-friendly class for the framework, all values are strings.
    /// </summary>
    internal class RawDialogues
    {
        /* maybe edit this so ints are actually ints and same with bools. in the case that CP auto-converts them */
        public int Time { get; set; } = -1;
        public string Location { get; set; } = "any";

        public string Dialogue { get; set; } = null;
        public bool ClearOnMove { get; set; } = false;
        public bool Override { get; set; } = false;

        public bool IsBubble { get; set; } = false;
        public bool Jump { get; set; } = false;
        public int Shake { get; set; } = -1;


        public bool MakeEmote { get; set; } = false;
        public int Emote { get; set; } = -1;

        public bool ChangeFacing { get; set; } = false;
        public int FaceDirection { get; set; } = -1;

        public RawDialogues()
        {
        }

        public RawDialogues(RawDialogues md)
        {
            Time = md.Time;
            Location = md.Location;

            Dialogue = md.Dialogue;
            ClearOnMove = md.ClearOnMove;
            Override = md.Override;

            IsBubble = md.IsBubble;
            Jump = md.Jump;
            Shake = md.Shake;
            Emote = md.Emote;
            FaceDirection = md.FaceDirection;
        }
#if DEBUG
        public RawDialogues(string word)
        {
            Time = (-1 * word.Length);
            Location = $"{word} location";

            Dialogue = $"{word} dialogue";
            ClearOnMove = false;
            Override = false;

            IsBubble = false;
            Jump = false;
            Shake = (1 * word.Length);
            Emote = (1 * word.Length);
            FaceDirection = -1;
        }
#endif
    }

    /// <summary>
    /// Class which parses raw data for internal/mod use.
    /// </summary>
    internal class ParsedDialogues
    {
        public int Time { get; set; }
        public GameLocation Location { get; set; }

        public string Dialogue { get; set; }
        public bool ClearOnMove { get; set; } = false;
        public bool Override { get; set; } = false;

        public bool IsBubble { get; set; } = false;
        public bool Jump { get; set; } = false;
        public int Shake { get; set; } = -1;
        public int Emote { get; set; } = -1;
        public int FaceDirection { get; set; } = -1;

        public ParsedDialogues()
        {
        }

        public ParsedDialogues(RawDialogues md)
        {
            Time = TimeOrNeg(md.Time);
            Location = GetLocationOrNull(md.Location);

            Dialogue = md.Dialogue;
            ClearOnMove = md.ClearOnMove;
            Override = md.Override;

            IsBubble = md.IsBubble;
            Jump = md.Jump;
            Shake = md.Shake;
            Emote = IntOrNegative(md.Emote, md.MakeEmote);
            FaceDirection = IntOrNegative(md.FaceDirection, md.ChangeFacing);
        }

        private static int IntOrNegative(int value, bool enabled)
        {
            if(enabled)
            {
                return value;
            }

            return -1;
        }
        private static int TimeOrNeg(int time)
        {
            if(time >= 600)
            {
                return time;
            }

            return -1;
        }
        public static int GetTime(string at)
        {
            if (at == "any" || string.IsNullOrWhiteSpace(at))
            {
                return 0;
            }
            else
            {
                return int.Parse(at);
            }
        }
        public static GameLocation GetLocationOrNull(string where)
        {
            if (string.IsNullOrWhiteSpace(where))
            {
                return null;
            }
            else
            {
                try
                {
                    return Game1.getLocationFromName(where);
                }
                catch (Exception ex)
                {
                    ModEntry.Mon.Log($"Error while getting game location: {ex}", LogLevel.Error);
                    ModEntry.Mon.Log($"Will default to any location.");
                    return null;
                }
            }
        }
        public static bool GetBool(string which)
        {
            try
            {
                var result = bool.TryParse(which, out bool raw);
                return result;
            }
            catch (Exception ex)
            {
                ModEntry.Mon.Log($"Error: {ex}");
                return true;
            }
        }
    }
}