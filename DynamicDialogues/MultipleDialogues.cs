using StardewModdingAPI;
using StardewValley;
using System;

namespace DynamicDialogues
{
    /*
     * impl. ideas:
     * - change facedirection
     * - emote
     * - face towards farmer
     * - wait to stop moving before applying dialogue
     *      (smth like :
     *          ApplyWhenMoving defaulting to true,
     *          or WaitToStopMoving which defaults to false
     *      )
     * - shake (originally a (bool, int). duration hardcoded (in my mod) to 1sec??)
     *      (maybe make it int that defaults to 0,
     *      if user int is greater than that perform shake w/ the int
     *      and if not, just do nothing abt it)
     * 
     * I may (or may not) add these in the future
     */

    /// <summary>
    /// A user-friendly class for the framework, all values are strings.
    /// </summary>
    internal class RawDialogues
    {
        /* maybe edit this so ints are actually ints and same with bools. in the case that CP auto-converts them */
        public string Time { get; set; } = "any";
        public string Location { get; set; } = "any";

        public string Dialogue { get; set; }
        public string ClearOnMove { get; set; } = "false";
        public string Override { get; set; } = "false";

        public string IsBubble { get; set; } = "false";
        public string Jump { get; set; } = "false";
        public string Shake { get; set; } = "-1";
        public string Emote { get; set; } = "-1";

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
        }
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

        public ParsedDialogues()
        {
        }

        public ParsedDialogues(RawDialogues md)
        {
            Time = GetTime(md.Time);
            Location = GetLocationOrNull(md.Location);

            Dialogue = md.Dialogue;
            ClearOnMove = GetBool(md.ClearOnMove);
            Override = GetBool(md.Override);

            IsBubble = GetBool(md.IsBubble);
            Jump = GetBool(md.Jump);
            Shake = int.Parse(md.Shake);
            Emote = int.Parse(md.Emote);
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