using UnityEngine;

namespace PixelCrushers
{

    /// <summary>
    /// Shows the cursor when this component is enabled. Typically add to a UI.
    /// When the UI GameObject is activated, the component becomes enabled and
    /// shows the cursor, if the InputDeviceManager is in Mouse mode or if there's
    /// no InputDeviceManager in the scene.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ShowCursorWhileEnabled : MonoBehaviour
    {
        void OnEnable()
        {
            if (InputDeviceManager.instance == null || InputDeviceManager.deviceUsesCursor)
            {
                CursorControl.SetCursorActive(true);
            }
        }

        void OnDisable()
        {
            if (InputDeviceManager.instance == null || InputDeviceManager.deviceUsesCursor)
            {
                CursorControl.SetCursorActive(false);
            }
        }
    }
}