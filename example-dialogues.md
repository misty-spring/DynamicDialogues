## Dialogue examples

As mentioned before, these are the options you can use for dialogues:

name | description
-----|------------
Time | (\*) Time to set dialogue at. 
From | (\*\*) Min. time to apply dialogue at.
To | (\*\*) Max time to apply dialogue at
Location | (\*) Name of the map the NPC has to be in. 
Dialogue | The text to display.
ClearOnMove | (Optional) If `true` and dialogue isn't read, it'll disappear once the NPC moves. 
Override | (Optional) Removes any previous dialogue.
Force | (Optional) Will show this NPC's dialogue even if you're not in the location.
IsBubble | (Optional) `true`/`false`. Makes the dialogue a bubble over their head.
Jump | (Optional) If `true`, NPC will jump. 
Shake | (Optional) Shake for the milliseconds stated (e.g Shake 1000 for 1 second).
Emote | (Optional) Will display the emote at that index ([see list of emotes](https://docs.google.com/spreadsheets/d/18AtLClQPuC96rJOC-A4Kb1ZkuqtTmCRFAKn9JJiFiYE/edit#gid=693962458))
FaceDirection | (Optional) Changes NPC's facing direction. allowed values: `0` to `3` or `up`,`down`,`left`,`right`.


\* = You need either a time or location (or both) for the dialogue to load.

\*\* = Mutually exclusive with "Time". Useful if you want a dialogue to show up *only* when the player is present.


### Using From-To
From-To will only apply the changes when the player is present, and when the time fits the given range.
The time can be anywhere between 610 and 2550. 

_"Why not earlier/later?"_: The mod adds dialogue when time changes. 
- When a day starts (6 am), no time change has occurred yet. 
- At 2600 the day ends, so you wouldn't get to see the dialogues (most they'd do is load, and immediately get discarded by the game).

**Example**: 
Let's say you want Willy to jump and say something- *only* between 610 - 8am and at the beach. The patch would look like this:

```
"fishEscaped": {
          "From": 610,
          "To": 800,
          "Location": "Beach",
          "Dialogue": "Argh! The fish escaped!",
          "IsBubble": true,
          "Jump": true,
        },
```

So, if the player enters the beach (between the specified time), willy will do this. 

### Using ClearOnMove
This option is specific to "box" dialogues (ones you have to click to see). If used with `"IsBubble": true`, it won't do anything.

Basically, it will remove a dialogue if the NPC moves. This is useful if you need the dialogue to disappear once the npc changes locations / to avoid "out of context" messages.

**Example:**
This makes Leah say something at Pierre's. If she starts walking (e.g to exit), the dialogue will be removed.
```
"pricesWentUp": {
          "Location": "SeedShop",
          "Dialogue": "Hi, @. Buying groceries too?#$b#...Did the prices go up?$2",
          "ClearOnMove": true,
        },
```

### Using Override
This option is for "box" dialogues (ones you have to click to see). If used with `"IsBubble": true`, it won't do anything.

It will force the dialogue to be added- regardless of the current dialogue. Useful if you want the NPC to have a dialogue mid schedule <u>animation</u>.
**Note:** This will remove any previous dialogue, so use with caution.

**Example:**
If you want Emily to say something when she's working at Gus', you'll need to use Override. (Otherwise, the dialogue will get "buried" under the schedule one).
```
"SaloonTime": {
          "Location": "Saloon",
          "Dialogue": "Did you come buy something?",
          "Override": true,
        },
```
