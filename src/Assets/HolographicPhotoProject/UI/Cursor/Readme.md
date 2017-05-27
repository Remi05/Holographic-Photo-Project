# Cursor
A contextual marker that points to where the gaze is. By default the cursor comes with a few pre-built cursors so you can keep or remove them as you like.

## Cursor objects hierarchy
![Object Hierarchy](../../../../../images/cursor/cursor_object.png)

## Cursor States
[Reference](https://github.com/Microsoft/HoloToolkit-Unity/blob/master/Assets/HoloToolkit/Input/Scripts/Cursor/Cursor.cs#L16)

# How to

## Add a cursor to your scene?
Drag and drop the `Cursor.prefab` under `Assets/ProjectMemuro/UI/Cursor/Resources/Prefabs` into the Unity scene.

## Add a new cursor for a `state`?
- Under the Cursor `gameobject`, increment the `CursorStateData` properties array count by as many new cursors you would like to add to create empty slots for each new cursor.
- For each empty slot
    - Add a name (anything you like!)
    - Select the state for this new cursor. Ex. `Observe`, `Interact`
    - Select a `gameobject` for the physical appearence of the cursor

## Add a new cursor for a `tag`?
- Under the Cursor `gameobject`, increment the `CursorStateData` properties array count by as many new cursors you would like to add to create empty slots for each new cursor.
- For each empty slot
    - Set the name to exactly match the gameobject tag for which you want the cursor to change
    - Set the state to `Contextual`
    - Select a `gameobject` for the physical appearence of the cursor 

## Cursor script properties
![Cursor script](../../../../../images/cursor/cursor_script.png)
- `Cursor State Data`
    - The size represents the number of cursor slots that are displayed.
    - A Cursor Slot
        - Name = Name of cursor slot
        - CursorState = Selection of HoloToolkit Cursor States
        - CursorObject = The object that becomes the cursor when the cursor is in a specific state or it is gazing at an object with a specific tag.

## Cursor demo scene
![Cursor demo scene](../../../../../images/cursor/scene_demo.png)
