## Randomized text

NPCs will say a random text when they have no more dialogue. The text is chosen from a list. This action is checked every 30 in-game minutes.

**How to use:** Just add an entry with the key "random" to the NPC's vanilla dialogue.

Example:


```
{
  "Action":"EditData",
  "Target":"Characters/Dialogue/Krobus",
  "Entries":{
    "Random.001": "@, what does your name mean?",
    "Random.002": "Nobody comes to the sewer...#$b#It's perfect for hiding.",
    "Random.003": "My body is too sensitive to the sun."
  }
}
```

The mod will grab all dialogues that start with 'random', add them to a pool, and choose randomly.
In this case, "Random.001" has a 30% chance of being chosen

After a dialogue has been used, it won't appear again for the day.
