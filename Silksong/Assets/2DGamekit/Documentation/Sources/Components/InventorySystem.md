Inventory System
================

The inventory system allow any entity in the game (Player, enemy, chest, shop
  etc.) to hold items. Items can be used in combination with other gameplay
  object like locks.

It is formed of 2 thing : `InventoryController` which hold `InventoryItem`

## Inventory Item

An `InventoryItem` is a component added to the `GameObject` representing the
item. It also require a `CircleCollider2D` and will add one if one if missing.
That collider is set to trigger and used to detect when the player touch the
object.
The parameters that need to be set are :

- `InventoryKey` : this is an unique name for the item (e.g. "key_zone1",
"alien_artifact" etc.), used by the inventory to track if item is owned or not.
**NOTE : the inventory can only hold a single instance of each object, if an
object with the same key is already in the inventory, the add event *(see
inventory documentation below)* won't be called**
- `Layers` : use as a filter to what can gather the object (e.g. built-in
  inventory object are only triggered by the player layer)
- `Disable On Enter` : the object will get disabled on contact (i.e. when added
  to an inventory)
-  `Clip` : if not empty, the set audio clip that will be played when the object
 is gathered
- `Data Settings` : see the `Persistent Data` documentation for more info.

#### Persistent Data

The persistent data saved by that component is if the gameobject is enabled or
disabled. That way a gathered object with `Disable On Enter` set will stay
disabled (i.e. gathered) on scene re-enter

## Inventory Controller

The Inventory controller script should be added to the object that will gathers
`InventoryItem` (e.g. the player). It allow to automatically gather
`InventoryItem` on contact with their collider and to define events to happens
when one is collected.

- `Inventory Events` : list of events to happen on specific objects added to the
inventory. Just increase the size to add a new one. _(tips : you can delete an
event by right clicking on the name in the list and choosing
`Delete Array Element`)_
  - `Key` : this is the key of the `InventoryItem` _(cf. above)_
  for which the events will be called
  - `OnAdd` and `OnRemove` events :those events are called when the item with
  the key specified above is added or removed from the inventory.
- `Data Settings` : cf. Persistent Data documentation

#### Persistent Data

The Inventory Controller save the list of items it gathered so the player keep
them between scene.
