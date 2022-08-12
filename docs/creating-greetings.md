## Adding greetings

"Greetings" refers to the text NPCs will use when passing by another NPC. 
This can occur, for example, when two NPCs walk by each other on the way to their schedules.

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
