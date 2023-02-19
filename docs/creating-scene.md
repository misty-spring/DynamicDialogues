## Event Scenes

Event scenes work just like vanilla scenes (for example, Caroline Tea or the Onions event.)

### Adding a Scene


First, you must load the scene to `Scenes/<name-of-scene>`. 

Files with a height of **112** will be automatically centered on the screen.


**Example:**

```
{
  "Action": "Load",
  "Target": "Scenes/MyScene",
  "FromFile": "assets/Scenes/MyScene.png"
}
```

Now that the scene is loaded, we can use it for events.

| field | required | description |
----------------------------------
| AddScene | yes | The command. |
| FileName | yes | The name of the scene to load. |
| ID | yes | Number assigned to this scene. Reccomended to use mod's UniqueID.
| Frames | no | Used for animations. How many frames this scene has. |
| Milliseconds | no | Used for animations. How many milliseconds each frame will have. |

**Example:**

Let's say our scene is a 112x112 image.

`/AddScene MyScene 12195`

This will add the scene *immediately.* 
(For a fade effect, add `/fade/viewport -300 -300/` before this command)

### Removing a Scene

To remove a scene, simply use the ID we assigned.

`/RemoveScene <ID>`

This will immediately remove it from the event.

**Example:**
`/fade/RemoveScene 12195/unfade`

First, the scene will fade out. Then, the scene will be removed.
When the game unfades, our scene will be gone.
