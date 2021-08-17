Moving Platform
===============

To create a moving platform, just add the `MovingPlatform` script to a
Gameobject.

If you need it to be able to move objects that fall on it also, you will
need to add a `PlatformCatcher` script to that same gameobject

The parameters available on the `MvingPlatform` script are :

- `PlatformCatcher` : this is only if your platform catcher is on a different
gameobject than the same as the `MovingPlatform`, otherwise ignore, the script
will find the `PlatformCatcher` by itself
- `PreviewPosition` : allow to preview the position of the platform at a given
percentage of the path. Useful to avoid intersection with the level
- `Start Moving`: if this is set, platform will start moving as soon as it is
enabled. Otherwise the function `StartMoving` have to be called (e.g. through
an event)
    - `When becoming visible` : this option only appear if the platform is set
    to start moving. If this is enabled, the platform will start moving only
    when it first become visible on screen, not when the level is loaded
- `Looping` : define how the platform react once it reach the end of its path
    - BACK_FORTH : will move back and forth between starting and ending point
    - LOOP : once reaching the last point of the path, will move toward the
    start in a straight line to restart the cycle, making the path a loop
    - ONCE : will stop reaching the end.
- `Speed` : the speed at which the platform move (in unit per second)

Then there is control related to the nodes of the platform path

## Nodes

The platform works by going from node to node. All nodes are expressed in the
platform **local space**. This allow to move the platform without having to redo
the whole path.

- To add a new node, just click the **Add Node** button.
- To remove a node, click the **delete button** under its name

Each node have 2 parameters :

- position : (in local space). This is for fine tuning, otherwise, you have
move gizmo appearing in the scene view to move node broadly where you want them
- Wait Time : if this is greater than 0, the platform will wait the given time
when reaching this node before starting to move to the next one.

Exception to that is the Node 0 that don't have position, as Node 0 is always
the platform position. It also can't be deleted.
