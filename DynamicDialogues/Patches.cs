using StardewValley;
using System;

namespace DynamicDialogues
{
    internal class Patches
    {
        public static bool SayHiTo_Prefix(ref NPC __instance, Character c)
        {
            var instancename =__instance.Name; 
            var cname = (c as NPC).Name;
            var mainAndRef = (instancename, cname);
            var refAndMain = (cname, instancename);

            try
            {
                //if a (thisnpc, othernpc) key exists
                if (ModEntry.Greetings.ContainsKey((mainAndRef)))
                {
                    //log, then use previous key to find value
                    ModEntry.Mon.Log($"Found greeting patch for {__instance.Name}");
                    __instance.showTextAboveHead(ModEntry.Greetings[(mainAndRef)]);

                    //if that other npc has a key for thisnpc
                    if (ModEntry.Greetings.ContainsKey(refAndMain))
                    {
                        //same as before
                        ModEntry.Mon.Log($"Found greeting patch for {(c as NPC).Name}");
                        (c as NPC).showTextAboveHead(ModEntry.Greetings[(refAndMain)], -1, 2, 3000, 1000 + Game1.random.Next(500));
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Mon.Log($"Error while applying patch: {ex}", StardewModdingAPI.LogLevel.Error);
            }

            return true;
        }
    }
}