# DynamicDialogues
A framework which allows for dynamic dialogues throughout the day.


## Contents
* [Features](#features)
* [How to use](#how-to-use)
  * [Adding dialogues](#adding-dialogues)
  * [Adding greetings](#adding-greetings)
  * [Adding notifications](#adding-notifications)
* [Known issues](#known-issues)

## Features
- Custom notifications
- Custom npc dialogues throughout the day
  - Both of these are time and/or location dependant.
- Custom greetings (when npcs say hi to each other)
- Patchable via CP

## How to use
Every NPC has its own dialogue file- this is made by checking NPCDispositions when the save is loaded.
So it's compatible with custom NPCs of any type.

### Adding dialogues
To add dialogues, edit `mistyspring.dynamicdialogues/Dialogues/<namehere>`. 
Each dialogue has a unique key to ensure multiple patches can exist.

name | description
-----|------------
Time | Time to set dialogue at. (Optional*)
Location | Name of the map the NPC has to be in. (Optional*)
Dialogue | The text to display.
ClearOnMove | If `true` and dialogue isn't read, it'll disappear once the NPC moves. (Optional)
Override | Removes any previous dialogue. (Optional)
IsBubble | (`true`/`false`) Makes the dialogue a bubble over their head. (Optional)
Jump | if `true`, NPC will jump. (Optional)
Shake | Shake for the milliseconds stated (e.g Shake 1000 for 1 second). (Optional)
Emote | Will display the emote at that index ([see list of emotes](https://www.reddit.com/r/StardewValley/comments/5s5m9g/help_annoyed_squiggle/ddd33qg/)) (Optional)
FaceDirection | (`0` to `3` or `up`,`down`,etc) Changes NPC's facing direction.(Optional)

*= You must either set a time or a location (or both) for the dialogue to load.

This is the template:
```
"nameForPatch": {
          "Time": ,
          "Location": ,
          "Dialogue": ,
          "ClearOnMove": ,
          "IsBubble": ,
          "Emote": ,
          "Shake": ,
          "Jump": ,
        },
```
Just remove any fields you won't be using. Example:
```
{
      "Action": "EditData",
      "Target": "mistyspring.dynamicdialogues/Dialogues/Haley",
      "Entries": {
        "examplepatch": {
          "Time": "700",
          "Dialogue": "Some placeholder text",
          "IsBubble": "true",
          "Jump": "true"
        }
      }
    },
```


### Adding greetings

Greetings use a file called `mistyspring.dynamicdialogues/Greetings`.

Template:
```
{
      "Action": "EditData",
      "Target": "mistyspring.dynamicdialogues/Greetings",
      "Entries": {
        "nameOfNpc": {
          "NpcA": "",
          "NpcB": "",
          "NpcC": ""
          //...etc. you can add for any and all NPCs
        }
      }
    }
```
Example:

```

{
      "Action": "EditData",
      "Target": "mistyspring.dynamicdialogues/Greetings",
      "Entries": {
        "Alex": {
          "Evelyn": "Hello",
          "George": "Good morning"
        }
      }
    }
```


### Adding notifications
Notifications are loaded from `mistyspring.dynamicdialogues/Notifs`.

name | description
-----|------------ 
Time | Time to add a notification at. (Optional*)
Location | Name of map to display the notif in. (Optional*)
Message | Message to display. 
IsBox | if `true`, will make notification a box. (Optional)
Sound | sound the notif will make, if any. ([see sound IDs](https://docs.google.com/spreadsheets/d/18AtLClQPuC96rJOC-A4Kb1ZkuqtTmCRFAKn9JJiFiYE)) (Optional)

* = like with dialogues, you must either set a time, a dialogue, or both.

Template:

```
        "example_asBox": {
          "Time": "",
          "Message": "",
          "IsBox": false,
          "Sound": "",
          }

```

Example:
```
{
      "Action": "EditData",
      "Target": "mistyspring.dynamicdialogues/Notifs",
      "Entries": {
        "example_asBox": {
          "Time": "640",
          "Message": "Notif test as a box.",
          "IsBox": true
        }
      }
    },

```

