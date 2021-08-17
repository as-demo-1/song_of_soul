SceneLinkSMB
============

`SceneLinkSMB` is a class that extend how the `StateMachineBehaviour` work. It
allow for Behaviour on states in the `Animator` state machine to keep a
reference to the Monobehaviour to which they are attached.

## Usage

To code a new Behaviour using the `SceneLinkSMB`, you need to :

- Make your new class inherit `SceneLinkSMB`. As the class is a Generic, you
need to specify the type of the Monobehaviour that it will hold.
e.g. `public class EnemyAttackState : SceneLinkSMB<EnemyScript>`
- Initialize on every object with an animator using that state. In our previous
example, we could add in the `Start()` function of `EnemyScript` the line
`SceneLinkedSMB<EnemyScript>.Initialise(animator, this);` where animator is
that object animator recovered with `GetComponent<Animator>()` _note : this is
only needed a single time, even if you have 10 differents scripts on multiples
states. As long as they all inherit from ` SceneLinkSMB<EnemyScript>` they will
all get initialize with that single line._
- Override in your script `EnemyAttackState` any state function (enter, exit...)
you may need

Now in your StateBehaviour `EnemyAttackState` you can access a protected member
called `m_Monobehaviour` that is a of type `EnemyScript` and is the instance on
which that state behaviour just got called.

## Example

### EnemyAttackState.cs

```csharp
public class EnemyAttackState : SceneLinkSMB<EnemyScript>
{
  public override void OnSLStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    //m_MonoBehaviour is of type EnemyScript  
    m_MonoBehaviour.TrickerAttack ();
  }
}
```
### EnemyScript.cs

```csharp
public class EnemyScript : MonoBehaviour
{
  Animator animator;
  void Start()
  {
    animator = GetComponent<Animator>();
    SceneLinkedSMB<EnemyScript>.Initialise(animator, this);
  }
}

```
