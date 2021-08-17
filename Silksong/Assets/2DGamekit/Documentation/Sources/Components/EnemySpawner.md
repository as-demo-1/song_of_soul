Enemy Spawner
=============

To use the enemy spawner, just add an `EnemySpawner` script to a gameobject.
(Or use one of the prefabs)

The parameters availables are :

- `Prefab` : a prefab of the enemy used by that spawner
- `Initial Pool Count` : Spawner pool the enemies they will spawn. A good value
to enter here is equal to the number of concurrent enemy that need to be spawned
- `Total Enemis to be spawned` : the number of enemies the spawner will spawn
until it stop spawning enemies totally.
- `Concurrent Enemies to be spawned` : the number of enemies that the spawner
will spawn in the world `at the same time`. *E.g. if this is 5 with 10 total
enemies, the spawner will spawn 5 enemies. Once one is killed it will spawn a
new one unless it already spawned 10 in total*
- `Spawn Area` : This is the extend of the spawn zone around the spawnpoint.
Enemies will be spawn at random positions in that zones. This can be visualize
in the scene view by the *white rectangle around the spawn point*
- `Spawn Delay` : this is the delay between the death of an enemy and the spawn
of the next one
- `Removal Delay` : the time before an enemy is sent back to the pool
(removed from the world). Useful if your enemy don't "remove" itself and leave
something like a ragdol etc..
