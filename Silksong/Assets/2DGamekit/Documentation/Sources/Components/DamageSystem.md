Damage System
=============

The damage system in the kit is composed of two component : `Damager` and
`Damageable`

Adding a `Damager` script to an object make it able to damage any object that
have a `Damageable` component, as long as their settings match.

## Damager

The damager does **not** require a collider, it will display a box gizmo in the
sceneview you can use to tweak it's size & position.

#### Settings

- `Damage` : the amount of damage it will inflict on a damageable on hit
- `Offset` and `Size` : those are the two parameters that define the damager
"collision box". It is displayed in the SceneView and can be modified there, or
fine tunes using those two parameters
- `Offset Based On Sprite Facing` : use to make the damager change position
based on the facing of the sprite renderer set on the damager *E.g. this is used
by the player so the damager zone flip left and right depending where the player
face*
- `Sprite Renderer` : this is the sprite renderer that the above setting use
- `Can Hit Triggers` : does the damager collide with trigger (checking if they
contain a Damageable) or does it ignore trigger all together
- `Force Respawn` : if this toggle is set, when that damager hit a damageable
that contain a script handling that (i.e. the player) it force it to respawn
to the latets checkpoint instead of just being hit. (*e.g. this is used by the
water so falling in it the player back to checkpoint*)
- `Ignore Invincibility` : if set, this damager will still "hit" the damageable,
meaning the damageable will still receive OnHit message even if in invincible
(but won't get any health removed). Useful for object that should stop the
player all the time (e.g. water always need to respawn the player, even if they
are still invincible when falling in it)
- `Hittable Layers` : which layer that damager will consider when checking hit
(any damageable on different layers will be ignore and not hit by that damager)
- `On Damageable Hit` and `On Non Damageable Hit` : allow to call function on
scripts when those two event occurs. `On Non Damageable Hit` is called for
objects that are in the hittable layers but does not have a damageable attached

## Damageable

Contrary to the damager, the `Damageable` rely on colliders attached to the same
GameObject, so in addition to adding a `Damageable` script to your object don't
forget to add colliders too to define its hittable zones.

#### Settings

- `Starting Health` : how many health that damageable have when it get created.
Damager will remove their Damage from hit at each hit. Damageable die when this
reach 0
- `Invulnerable After Damage` : does this damageable become invulnerable for a
given time (see next setting) after being hit
- `Invulnerability Duration` : for how long (in second) this damageable is
considered invulnerable after being hit (only applicable if previous setting is
set)
- `Disable On Death` : if set, will disable the gameobject when it die
- `Center Offset` : this is used to define where the center of the damageable is
(an offset of (0,0,0) mean it is on the gameobject position). Used to compute
distance to damager

The different events are self explanatory and are called after the damageable
action (*e.g`On gain Health` will be called after the new health have been set)


*For the persistence system defined by the persistence type & data type, see
the Persistent Data documentation*

## Checkpoint

Checkpoint work only with the Player Character. To add a checkpoint, just add
the `Checkpoint` script to a GameObject. Then scale the collider to be sure that
the player will collide with it when passing. Each time the player enter a
checkpoint collider, this checkpoint become their active one. When they fall
into the water (or get hit by any damager that have the `Force Respawn` setting)
they will reappear at that checkpoint.

**Note** : The player will reappear at the gameobject position, so be sure it is
above the ground!
