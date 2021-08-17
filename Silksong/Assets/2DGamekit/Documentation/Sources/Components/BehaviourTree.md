Behaviour Tree
==============

**Note : This is a scripting only system, you need at least basic knowledge in
how scripts & C# work in Unity to understand how it works & use it**

As the name implies, Behaviour Tree are a way to code behaviour using a tree of
actions :  
```
                        +------+
                 +------+ Root +----+
                 |      +------+    |
                 |          |       |
            +----v---+  +---v--+ +--v---+
            |Selector|  | Test | | Test |
         +-----------+  +------+ +------+
         |       |         |          |
    +----v-+ +---v--+   +--v---+   +--v---+
    | Func | | Wait |   | Func |   | Func |
    +------+ +------+   +------+   +------+
```

## Theory
Each update of the game, the tree is "ticked", meaning it go through each child
node of the root, going further down if said node have children etc.

Each node will have actions associated, and will return one of 3 states to its
parent :

- `Success` : the node finish successfully its task
- `Failure` : the node failed the task
- `Continue` : the node didn't finish the task yet

Returned state is used by each node parent differently.

_ E.g.:_
- _`Selector` will make the next child active if the current one returned
`Failure` or `Success` and keep the current one active if it return `Continue`_
- _`Test` node would call their child and return the child state if the test is
true, and return `Failure` without calling their children if the test is false_
- _Etc._

## Gamekit Implementation

The way Behaviour Tree are used in gamekit is through script. Here is an example
of a very simple behaviour tree.

Need to add `using BTAI;` at the top of the file.

```csharp
Root aiRoot = BT.Root();
aiRoot.Do(
  BT.If(TestVisibleTarget).Do(
    BT.Call(Aim),
    BT.Call(Shoot)
  ),
  BT.Sequence().Do(
    BT.Call(Walk),
    BT.Wait(5.0f),
    BT.Call(Turn),
    BT.Wait(1.0f),
    BT.Call(Turn)
  )
);
```

`aiRoot` should be stored in the class as a member, because you need to call
`aiRoot.Tick()` in the `Update` function so the Tree actions are executed.

Let's walk through how the `Update` will go in the `aiRoot.Tick()` :

- Test if the function `TestVisibleTarget` return true. If it does, it goes to
execute the children, which are calling function `Aim` then `Shoot`
- If the test return false, the `If` node will return `Failure` so the root will
go to the next child. This is a `Sequence`, which start by executing its first
child
  - It call the function `Walk`. It return `Success` so the `Sequence` set the
  next child as active and executive it
  - The `Wait` node execute. Since it have to wait for 5 second and just was
  call for the 1st time, it still didn't reach the wait time, so it will return
  `Continue`
  - As the Sequence receive a `Continue` state from its active child, it don't
  change the active child, so it will start from that child on the next `Update`
- Once the Wait node will have been updated enough to reach its timer, it will
return `Sucess` so the sequence will go to the next child.
- Etc.

## Nodes List

#### Sequence

Execute children node one after another. If a child return :
- `Success` : the sequence will tick the next child the next frame
- `Failure` : the sequence will return to the first child next frame
- `Continue` : the sequence will call that node again next frame

#### RandomSequence

Execute a random child from its children list every time it is called.

You can specify a list of weights to apply to each child as an int array in the
constructor, to make some child more likely to be picked.

#### Selector

Execute all children in order until one return `Success`, then exit without
executing the remaining children nodes. If none return `Success` this node will
return `Failure`

#### Call

Call the given function, always return `Success`

#### If

Call the given function.
- If it return true, it call it current active child
and return its state.
- Otherwise return `Failure` without calling its children

#### While

Return `Continue` as long as the given function return true. (so next frame when
the tree is ticked, it will start from that node again without evaluating all
the previous nodes).

Children will be executed one after another

Will return `Failure` when the function return false & the loop is broken.

#### Condition

This node return `Success` if the given function return true, `Failure` if false

Useful chain with other node that depend on their children result (e.g Sequence,
Selector etc.)

#### Repeat

Will execute all children node for a given number of time consecutively.

Always return `Continue` until it reach the count, where it return `Success`

#### Wait

Will return `Continue` until the given time have been reached (starting when
  first called), it then return `Success`

#### Trigger

Allow to set a trigger (or unset if last argument is set to false) in the given
animator. Always return `Success`

#### SetBool

Allow to set the value of a Boolean Parameter in the given animator. Always
return `Success`

#### SetActive

Set active/unactive a given GameObject. Always return `Success`
