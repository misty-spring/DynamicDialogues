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
  - Dialogues have several configuration options. 
  - Both of these are time and/or location dependant.
- Custom greetings (when npcs say hi to each other)

This mod makes use of ContentPatcher to be edited.

## How to use
Every NPC has its own dialogue file- this is made by checking NPCDispositions when the save is loaded.
So it's compatible with custom NPCs of any type.

Notifs are all in a single file, and so are Greetings (see adding [notifs](#adding-notifications) or [greetings](#adding-greetings) for more info).

If the NPC hasn't been unlocked yet (e.g kent or leo), their entries will be skipped until the player meets them.
**Note:** ALL files are reloaded when the day starts.

### Adding dialogues
To add dialogues, edit `mistyspring.dynamicdialogues/Dialogues/<namehere>`. 
Each dialogue has a unique key to ensure multiple patches can exist.

name | description
-----|------------
Time | (*) Time to set dialogue at. 
Location | (*) Name of the map the NPC has to be in. 
Dialogue | The text to display.
ClearOnMove | (Optional) If `true` and dialogue isn't read, it'll disappear once the NPC moves. 
Override | (Optional) Removes any previous dialogue.
Force | (Optional) Will show this NPC's dialogue even if you're not in the location.
IsBubble | (Optional) `true`/`false`. Makes the dialogue a bubble over their head.
Jump | (Optional) If `true`, NPC will jump. 
Shake | (Optional) Shake for the milliseconds stated (e.g Shake 1000 for 1 second).
Emote | (Optional) Will display the emote at that index ([see list of emotes](https://docs.google.com/spreadsheets/d/18AtLClQPuC96rJOC-A4Kb1ZkuqtTmCRFAKn9JJiFiYE/edit#gid=693962458))
FaceDirection | (Optional) Changes NPC's facing direction. allowed values: `0` to `3` or `up`,`down`,`left`,`right`.

*= You must either set a time or a location (or both) for the dialogue to load.

Template:
```
"nameForPatch": {
          "Time": ,
          "Location": ,
          "Dialogue": ,
          "Override": ,
          "Force": ,
          "ClearOnMove": ,
          "IsBubble": ,
          "Emote": ,
          "Shake": ,
          "Jump": ,
        },
```
Just remove any fields you won't be using.
**Note:** If you don't want the dialogue to appear every day, use CP's "When" field.
Example:
```
{
      "Action": "EditData",
      "Target": "mistyspring.dynamicdialogues/Dialogues/Haley",
      "Entries": {
        "sunnyday": {
          "Time": "1000",
          "Dialogue": "It's so sunny today!",
          "IsBubble": true
        }
      },
      "When":{
        "Weather":"Sun"
      }
    },
```

### Adding greetings

Greetings use a file called `mistyspring.dynamicdialogues/Greetings`.

Template:
```
      "nameOfNpc": {
          "NpcA": "",
          "NpcB": "",
          "NpcC": ""
          //...etc. you can add for any and all NPCs
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
Time | (*) Time to add a notification at.
Location | (*) Name of map to display the notif in. 
Message | Message to display. 
IsBox | (Optional) If `true`, will make notification a box. 
Sound | (Optional) Sound the notif will make, if any. ([see sound IDs](https://docs.google.com/spreadsheets/d/18AtLClQPuC96rJOC-A4Kb1ZkuqtTmCRFAKn9JJiFiYE))

* = like with dialogues, you must either set a time, a location, or both.

Template:

```
        "example_asBox": {
          "Time": "",
          "Location": "",
          "Message": "",
          "IsBox": false,
          "Sound": "",
          }

```
**Note:** If you don't want the notif to appear every day, use CP's "When" field 
(e.g only send when it rains, when you've got x hearts with a NPC, etc. All conditions are compatible).
Example:
```
{
      "Action": "EditData",
      "Target": "mistyspring.dynamicdialogues/Notifs",
      "Entries": {
        "example_asBox": {
          "Location": "Farm",
          "Message": "The weather seems gloomy today...",
          "IsBox": true
        }
      },
      "When":{
        "Weather":"Rain, Storm"
      }
    },
```


## For more information
You can send me any question via [nexusmods](https://www.nexusmods.com/users/130944333) or in here.
