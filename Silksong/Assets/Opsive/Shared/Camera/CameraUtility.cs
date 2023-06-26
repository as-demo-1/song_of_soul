/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Camera
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Contains a set of utility functions useful for interacting with the camera.
    /// </summary>
    public class CameraUtility
    {
        private static Dictionary<GameObject, UnityEngine.Camera> s_GameObjectCameraMap = new Dictionary<GameObject, UnityEngine.Camera>();

        /// <summary>
        /// Returns the camera with the MainCamera tag or the camera with the CameraController attached.
        /// </summary>
        /// <param name="character">The character that the camera is attached to.</param>
        /// <returns>The found camera (if any).</returns>
        public static UnityEngine.Camera FindCamera(GameObject character)
        {
            UnityEngine.Camera camera;
            if (character != null) {
                if (s_GameObjectCameraMap.TryGetValue(character, out camera)) {
                    // The reference may be null if the scene changed.
                    if (camera != null) {
                        return camera;
                    }
                    // The reference is null - search for the camera again.
                    s_GameObjectCameraMap.Remove(character);
                }
            }
            // First try to find the camera with the character attached. If no camera has the character attached the return the first camera with the CameraController.
            camera = SearchForCamera(character);
            if (camera == null) {
                camera = SearchForCamera(null);
                if (camera != null) {
                    // The camera controller's character field must be null or equal to the existing character.
                    var cameraController = camera.GetComponent<Shared.Camera.ICamera>();
                    if (cameraController.Character != null && cameraController.Character != character) {
                        camera = null;
                    }
                }
            }
            if (camera != null && character != null) {
                s_GameObjectCameraMap.Add(character, camera);
            }
            return camera;
        }

        /// <summary>
        /// Loops through the cameras searching for a camera with the character assigned.
        /// </summary>
        /// <param name="character">The character to search for. Can be null.</param>
        /// <returns>The camera with the character assigned.</returns>
        private static UnityEngine.Camera SearchForCamera(GameObject character)
        {
            Shared.Camera.ICamera cameraController;
            UnityEngine.Camera mainCamera;
            if ((mainCamera = UnityEngine.Camera.main) != null &&
                (cameraController = mainCamera.GetComponent<Shared.Camera.ICamera>()) != null && (character == null || cameraController.Character == character)) {
                return mainCamera;
            }
            var cameras = UnityEngine.Object.FindObjectsOfType<Camera>();
            for (int i = 0; i < cameras.Length; ++i) {
                var iCamera = cameras[i].GetComponent<Shared.Camera.ICamera>();
                if (iCamera == null) {
                    continue;
                }
                if (character == null || iCamera.Character == character) {
                    return cameras[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Clears the Unity Engine Utility cache.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ClearCache()
        {
            if (s_GameObjectCameraMap != null) { s_GameObjectCameraMap.Clear(); }
        }
    }
}