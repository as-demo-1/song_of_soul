Getting Started
===============

This guide will walk you through setting up an empty scene to start creating a
new level.

## Creating the scene

The project contains an utility to setup a default scene.

In the menu "Kit Tools", choose "Create New Scene..." and enter a name for your
new scene.

A new scene will be created in the Assets/Scenes folder and the scene will be
added to the build settings for you.

That new scene will also be open in the editor so you can start working on it.

## Painting tilemap

To create the level shape, open the Tile Palette (found in the Window menu)

Two tile are included in the project : a grassy rock tile and an alien structure
tile.

Those tiles use the "scriptable tile" system of the unity tilemap : they were
setup to automaticaly choose the right sprite. Try it by selecting one of the
2 and paint in the Scene View by pressing right mouse button and dragging it
across the view.

## Adding a door

The "Prefabs" folder contains multiple object used in the game. Look for one
called **Door**. Drag it in the Scene view and place it where you want to block
the path of the player.

**Note : if the Tile Palette is still open, you may still be in _Paint mode_.
Click on the move tool on the top left corner of the Editor to go back to
manipulating GameObject**

## Making the door open

The door is open by playing its animation. To trigger the animation, let's use
a pressure plate.

Look for the prefab **PressurePad** in the Prefabs folder.

Drag it close to the ground in front of the door.
<br/>*Note : if the player seem to be blocked by the pressure pad instead of stepping
on it, lower it. The player get stuck against the collider*

In the Inspector, find the Pressure Pad component.

Some events are already defined to play the sound & change the pad sprite when
it is pressed.

- In the On Pressed list, click the + at the bottom right to add a new event
- Drag the door from the Hierarchy to the `None(Object)` field in the event
- In the `No Function` dropdown, find `Animator > Play(string)`
- in the text box that appeared under the dropdown, input `DoorOpening`

## Moving to another Level

To allow the player to change level, the **TransitionStart** prefab have to be
used. It can be found in the Prefabs/SceneControl folder.

Drag it to the scene (for example behind the door)

Place it at a position where the player will collide with the collider (the
  green box) when walking.

In the TransitionStart component, drag the **Ellen** gameobject into the
"Transitioning Game Object" slot. Then select

- Transition Type : Different level
- New Scene Name : Choose the scene to which that transition send the player.
(for example, choose Zone 1)
- Transition destination tag : all transition **destination** have an associated
letter. Leaving A will send you to the Destination tagged A in the Zone1 scene
- Transition When : choose On Trigger Enter, so the transition is done when the
player collide with the collider, not when they press a key or through event.

Play, open the door, and walking where the transition is should send you to the
zone 1!


## Having Fun

Most object play with that system of events seen in the pressure pad setup.

Use the existing scenes (Zone 1 to 5) to see how some object are setup.

Also refer to the Components documentation of the project, listing Components
and their parameters.

Happy Level designing!
