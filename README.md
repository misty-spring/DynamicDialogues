# DynamicDialogues
A framework which allows for dynamic dialogues throughout the day.


## Contents
* [Features](#features)
* [How to use](#how-to-use)
  * [Adding dialogues](https://github.com/misty-spring/DynamicDialogues/blob/main/docs/creating-dialogues.md)
  * [Adding greetings](#adding-greetings)
  * [Adding questions](#adding-questions)
  * [Adding notifications](#adding-notifications)
* [Known issues](#known-issues)


## Features
- Custom notifications
- Custom npc dialogues throughout the day
  - Dialogues have several configuration options. 
  - Both of these are time and/or location dependant.
- Custom greetings (when npcs say hi to each other)

This mod makes use of ContentPatcher to be edited.

------------

## How to use
Every NPC has its own dialogue file- this is made by checking NPCDispositions when the save is loaded.
So it's compatible with custom NPCs of any type.

Notifs are all in a single file, and so are Greetings (see adding [notifs](#adding-notifications) or [greetings](#adding-greetings) for more info).

If the NPC hasn't been unlocked yet (e.g kent or leo), their entries will be skipped until the player meets them.
**Note:** ALL files are reloaded when the day starts.

------------

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

------------

### Adding questions

Questions are loaded from `mistyspring.dynamicdialogues/Questions/<NPC name>`. Once a NPC has nothing else to talk about, you can ask them questions (if any exist).

Like with dialogues, these need a key (it's only used in the case errors are found, so the name doesn't matter).

You can add multiple questions.

name | description
-----|------------ 
Question | Text the question will have.
Answer | NPC's answer.
MaxTimesAsked | Max times you can ask this question. If 0, it'll count as infinite (Optional)
Location | The question will only appear when in this location. (Optional)
From | The hour the question *can* begin being added at. (Optional)
To | Limit time for adding the question. (Optional)

Template:
```
"nameForPatch": {
          "Question": ,
          "Answer": ,
          "MaxTimesAsked": ,
          "Location": ,
          "From": ,
          "To": 
        },
```

Just remove any fields you won't be using.
**Note:** If you don't want the question to appear every day, use CP's "When" field.
Example:
```
{
      "Action": "EditData",
      "Target": "mistyspring.dynamicdialogues/Questions/Elliott",
      "Entries": {
        "rainyday": {
          "Question": "What do you think of rainy days?",
          "Answer": "My, they're quite gloomy....$2",
          "Location": "ElliottHouse",
          "MaxTimesAsked": 2
        }
      },
      "When":{
        "Weather":"Rain",
        "Hearts:Elliott": "{{Range: 3, 14}}"
      }
    },
```

------------

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

------------

## Known issues
None, as of now.

(Keep in mind, this framework updates its information once per game day. So, edits added OnLocationChange/OnTimeChange won't be applied.
The framework contains a time and location condition (for the dynamic content), so this is not a problem).

------------

## For more information
You can send me any question via [nexusmods](https://www.nexusmods.com/users/130944333) or in here.
