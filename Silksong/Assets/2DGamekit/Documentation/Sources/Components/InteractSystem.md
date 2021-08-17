Interaction System
==================

Interaction with GameObject by the player is made through multiple components
all name **InteractOn***

- InteractOnCollision2D
- InteractOnTrigger2D
- InteractOnButton2D

## InteractOnCollision2D

Will call the events when an object part of the set `layers` will enter in collision
with this object.

## InteractOnTrigger2D/InteractOnButton2D

The two share the same setup and both send event on the player entering the
trigger on that GameObject. InteractOnButton2D allow to additionally set events
to happen when the player press the interact button (see `PlayerInput`) when
inside the trigger.

### Inventory Check

Both InteractOnTrigger2D and InteractOnButton2D allow to also have check for
object in the object on interaction (e.g. to open a door only if player have the
  key in its inventory when entering the trigger in front of the door)

- Add an element in the Inventory Checks array
- Add as many items you want to check in the Inventory Items list (the names the
  system expect are the same as the one set on the Item, see
  [Inventory System](./InventoySystem.md) documentation)
- The system will call On Have Items if the player have **all** the items,
otherwise it will call On Does Not Have Items
