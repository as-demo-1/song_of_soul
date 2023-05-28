/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy and remove the lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]". Then add your code where the comments indicate.
 * [REMOVE THIS LINE] If your code references scripts or assets that are outside of the Plugins
 * [REMOVE THIS LINE] folder, move this script outside of the Plugins folder, too.
 * [REMOVE THIS LINE]



using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

public class PersistentDataTemplate : MonoBehaviour //<--Copy this file. Rename the file and class name.
{ 

    public void OnRecordPersistentData()
    {
        // Add your code here to record data into the Lua environment.
        // Typically, you'll record the data using a line similar to:
        //    DialogueLua.SetVariable("someVarName", someData);
        // or:
        //    DialogueLua.SetActorField("myName", "myFieldName", myData);
        //
        // Note that you can use this static method to get the actor 
        // name associated with this GameObject:
        //    var actorName = OverrideActorName.GetActorName(transform);
    }

    public void OnApplyPersistentData()
    {
        // Add your code here to get data from Lua and apply it (usually to the game object).
        // Typically, you'll use a line similar to:
        // myData = DialogueLua.GetActorField(name, "myFieldName").AsSomeType;
        //
        // When changing scenes, OnApplyPersistentData() is typically called at the same 
        // time as Start() methods. If your code depends on another script having finished 
        // its Start() method, use a coroutine to wait one frame. For example, in 
        // OnApplyPersistentData() call StartCoroutine(DelayedApply());
        // Then define DelayedApply() as:
        // IEnumerator DelayedApply() {
        //     yield return null; // Wait 1 frame for other scripts to initialize.
        //     <your code here>
        // }
    }

    public void OnEnable()
    {
        // This optional code registers this GameObject with the PersistentDataManager.
        // One of the options on the PersistentDataManager is to only send notifications
        // to registered GameObjects. The default, however, is to send to all GameObjects.
        // If you set PersistentDataManager to only send notifications to registered
        // GameObjects, you need to register this component using the line below or it
        // won't receive notifications to save and load.
        PersistentDataManager.RegisterPersistentData(this.gameObject);
    }

    public void OnDisable()
    {
        // Unsubscribe the GameObject from PersistentDataManager notifications:
        PersistentDataManager.RegisterPersistentData(this.gameObject);
    }

    //--- Uncomment this method if you want to implement it:
    //public void OnLevelWillBeUnloaded() 
    //{
    // This will be called before loading a new level. You may want to add code here
    // to change the behavior of your persistent data script. For example, the
    // IncrementOnDestroy script disables itself because it should only increment
    // the variable when it's destroyed during play, not because it's being
    // destroyed while unloading the old level.
    //}

}



/**/
