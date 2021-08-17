RandomAudioPlayer
=================

This script allow to play a random sound chosen from a list of clip. You can
see it used on the different sound on Ellen prefab (check under the SoundSources
child gameobject of Ellen). E.g. if you look at FootstepSource, you can see it
define a list of 4 footstep sound that will be randomly pick by the footstep,
with an added pitch randomization.
That example also include the use of override per specific tile : there is an
override for the Alien ruin tile, which play different footstep sound.

To add override to object that aren't a tilemap, check the AudioSurface section
below.

#Scripting side

On the scripting side, RandomAudioPlayer have a PlayRandomSound function that is
called by other scripts when a sound need to be played (e.g. footstep sound is
trigger by the function PlayFootstep in the PlayerController, function itself
called by the animation clip on frame where the foot touch the ground.)

This function can take a TileBase to choose override sounds (e.g. footstep use
the current surface the Player is on, a bullet colliding will use the Tile of
the surface it just collide with etc.)

AudioSurface
============

Audio surface is a script you can add to any objects that is not a tilemap (e.g
a moving or passthrough platform, a moving box etc...) that allow to define what
Tile should be use when the audio player look for override sound (e.g. a stone
box would use an AudioSurface with the Alien Tile as the tile setup, so walking
on it would trigger the "stone footstep" sound and not the normal "ground" one)
