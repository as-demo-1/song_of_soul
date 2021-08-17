VFXController
=============

`VFXController` is an asset in your project folder (inside Resources folder) that
allow to list all the vfx your game will use. It will create a pool of instances
of those VFX to be used, moving the cost of instantiating the VFX prefab at the
beginning of the game instead of everytime they are used.

Scripts like the `PlayerController` or the `StateMachineBehaviour` `TriggerVFX` will
then trigger by name those VFX.

Like the audio player, it also allow you to define override per tile. E.g you
set an override to the vfx use for each footstep depending on which surface the
footstep land on.

(In the case of the VFXController shipped with this project, you can see an
example of that on the DustPuff VFX that use an override for when the player
walk on stone)

The overriding tile is given by the script that trigger the vfx (e.g. footstep
will pass the current surface, a bullet hitting a surface will pass which tile
is that surface etc.)

*It is possible to setup the override tile on non-tilemap object, refer to the
sound documentation Sounds.txt for how to setup which override a GameObject
should correspond to*
