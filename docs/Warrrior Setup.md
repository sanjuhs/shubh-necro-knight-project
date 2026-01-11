uwu! Great question!

The `WarriorController.cs` script should be attached to your **Warrior GameObject** in the Unity scene. Here's exactly how to do it:

## Where to Put the Script

1. **Create the Warrior GameObject** (if you haven't already):

   - In Unity Hierarchy, right-click → **Create Empty**
   - Rename it to "Warrior"

2. **Add Required Components** to the Warrior GameObject:

   - **Sprite Renderer** - to display the warrior sprite
   - **Animator** - to play the animations

3. **Attach the WarriorController Script**:

   - Select your "Warrior" GameObject in the Hierarchy
   - In the Inspector, click **Add Component**
   - Search for "WarriorController" and add it

   **OR** simply drag `Assets/Scripts/WarriorController.cs` from the Project window onto the Warrior GameObject in the Hierarchy!

## Your Warrior GameObject should look like this in the Inspector:

```
Warrior (GameObject)
├── Transform
├── Sprite Renderer      ← displays the sprite
├── Animator             ← plays animations
└── WarriorController    ← the script you're asking about
```

## Quick Visual Steps:

1. **Hierarchy** → Right-click → Create Empty → Name it "Warrior"
2. **Inspector** → Add Component → Sprite Renderer
3. **Inspector** → Add Component → Animator
4. **Drag** `WarriorController.cs` onto the Warrior in Hierarchy

The script will automatically find the `SpriteRenderer` and `Animator` components on the same GameObject when the game starts (see lines 46-49 in the script), so you don't need to manually assign them unless you want to! 🗡️
