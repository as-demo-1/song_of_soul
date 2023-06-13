// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static class of general purpose functions used by the Dialogue System.
    /// </summary>
    public static class Tools
    {

        public static void DeprecationWarning(MonoBehaviour mb, string extraInfo = null)
        {
#if !SUPPRESS_DEPRECATION_WARNINGS
            if (mb == null) return;
            Debug.LogWarning("Dialogue System: " + mb.GetType().Name + " is deprecated and will be removed in the next version. " + extraInfo + "\nTo supress this message, add the scripting define symbol SUPPRESS_DEPRECATION_WARNINGS", mb);
#endif
        }

        /// <summary>
        /// Determines if a GameObject reference is a non-instantiated prefab or a scene object.
        /// If `go` is `null`, active in the scene, or its parent is active in the scene, it's
        /// considered a scene object. Otherwise this method searches all scene objects for
        /// matches. If it doesn't find any matches, this is a prefab.
        /// </summary>
        /// <returns><c>true</c> if a prefab; otherwise, <c>false</c>.</returns>
        /// <param name="go">GameObject.</param>
        public static bool IsPrefab(GameObject go)
        {
            if (go == null) return false;
            if (go.activeInHierarchy) return false;
            if ((go.transform.parent != null) && go.transform.parent.gameObject.activeSelf) return false;
            var list = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == go) return false;
            }
            return true;
        }

        /// <summary>
        /// Utility function to convert a hex string to byte value.
        /// </summary>
        /// <returns>
        /// The byte value of the hex string.
        /// </returns>
        /// <param name='hex'>
        /// The hex string (e.g., "f0").
        /// </param>
        public static byte HexToByte(string hex)
        {
            return byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Determines whether an object is a numeric type.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the object is a numeric type; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='o'>
        /// The object to check.
        /// </param>
        public static bool IsNumber(object o)
        {
            return (o is int) || (o is float) || (o is double);
        }

        /// <summary>
        /// Converts a string to an int.
        /// </summary>
        /// <returns>
        /// The int, or <c>0</c> if the string can't be parsed to an int.
        /// </returns>
        /// <param name='s'>
        /// The string.
        /// </param>
        public static int StringToInt(string s)
        {
            return SafeConvert.ToInt(s);
        }

        /// <summary>
        /// Converts a string to a float, culture invariant (i.e., uses '.' for decimal point).
        /// </summary>
        /// <returns>
        /// The float, or <c>0</c> if the string can't be parsed to a float.
        /// </returns>
        /// <param name='s'>
        /// The string.
        /// </param>
        public static float StringToFloat(string s)
        {
            return SafeConvert.ToFloat(s);
        }

        /// <summary>
        /// Converts a string to a bool.
        /// </summary>
        /// <returns>
        /// The bool, or <c>false</c> if the string can't be parsed to a bool.
        /// </returns>
        /// <param name='s'>
        /// The string.
        /// </param>
        public static bool StringToBool(string s)
        {
            return (string.Compare(s, "True", System.StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Determines if a string is null, empty or whitespace.
        /// </summary>
        /// <returns><c>true</c> if the string null, empty or whitespace; otherwise, <c>false</c>.</returns>
        /// <param name="s">The string to check.</param>
        public static bool IsStringNullOrEmptyOrWhitespace(string s)
        {
            return string.IsNullOrEmpty(s) || string.IsNullOrEmpty(s.Trim());
        }

        /// <summary>
        /// Returns the remainder of a string after all forward slashes, or
        /// the enter string if it doesn't contain any forward slashes.
        /// </summary>
        public static string GetAllAfterSlashes(string s)
        {
            if (string.IsNullOrEmpty(s) || !s.Contains("/")) return s;
            var pos = s.LastIndexOf("/") + 1;
            return (0 < pos && pos < s.Length) ? s.Substring(pos) : s;
        }

        /// <summary>
        /// Gets the name of the object, or null if the object is null.
        /// </summary>
        /// <returns>
        /// The object name.
        /// </returns>
        /// <param name='o'>
        /// The object.
        /// </param>
        public static string GetObjectName(UnityEngine.Object o)
        {
            return (o != null) ? o.name : "null";
        }

        /// <summary>
        /// Gets the name of a component's GameObject.
        /// </summary>
        /// <returns>The game object name.</returns>
        /// <param name="c">A component</param>
        public static string GetGameObjectName(Component c)
        {
            return (c == null) ? string.Empty : c.name;
        }

        /// <summary>
        /// Gets the full name of a GameObject, following the hierarchy down from the root.
        /// </summary>
        /// <returns>The full name.</returns>
        /// <param name="go">A GameObject.</param>
        public static string GetFullName(GameObject go)
        {
            string fullName = string.Empty;
            if (go != null)
            {
                fullName = go.name;
                Transform t = go.transform.parent;
                while (t != null)
                {
                    fullName = t.name + '.' + fullName;
                    t = t.parent;
                }
            }
            return fullName;
        }

        /// <summary>
        /// Returns the first non-null argument. This function replaces C#'s null-coalescing
        /// operator (??), which doesn't work with component properties because, under the hood, 
        /// they're always non-null.
        /// </summary>
        /// <param name='args'>
        /// List of elements to select from.
        /// </param>
        public static Transform Select(params Transform[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null)
                {
                    return args[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the first non-null argument. This function replaces C#'s null-coalescing
        /// operator (??), which doesn't work with component properties because, under the hood, 
        /// they're always non-null.
        /// </summary>
        /// <param name='args'>
        /// List of elements to select from.
        /// </param>
        public static MonoBehaviour Select(params MonoBehaviour[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null)
                {
                    return args[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Sends a message to all GameObjects in the scene.
        /// </summary>
        /// <param name="message">Message.</param>
        public static void SendMessageToEveryone(string message)
        {
            GameObject[] gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            for (int i = 0; i < gameObjects.Length; i++)
            {
                var go = gameObjects[i];
                go.SendMessage(message, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Sends a message to all GameObjects in the scene.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="arg">Argument.</param>
        public static void SendMessageToEveryone(string message, string arg)
        {
            GameObject[] gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            for (int i = 0; i < gameObjects.Length; i++)
            {
                var go = gameObjects[i];
                go.SendMessage(message, arg, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Sends a message to all GameObjects in the scene in batches.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="gameObjectsPerFrame">Number of GameObjects to handle each frame.</param>
        public static IEnumerator SendMessageToEveryoneAsync(string message, int gameObjectsPerFrame)
        {
            GameObject[] gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            int count = 0;
            for (int i = 0; i < gameObjects.Length; i++)
            {
                var go = gameObjects[i];
                go.SendMessage(message, SendMessageOptions.DontRequireReceiver);
                count++;
                if (count >= gameObjectsPerFrame)
                {
                    count = 0;
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Sets the component's game object active or inactive.
        /// </summary>
        /// <param name="component">Component.</param>
        /// <param name="value">The value to set.</param>
        public static void SetGameObjectActive(Component component, bool value)
        {
            if (component != null) component.gameObject.SetActive(value);
        }

        /// <summary>
        /// Sets a game object active or inactive.
        /// </summary>
        /// <param name="gameObject">GameObject.</param>
        /// <param name="value">The value to set.</param>
        public static void SetGameObjectActive(GameObject gameObject, bool value)
        {
            if (gameObject != null) gameObject.SetActive(value);
        }

        /// <summary>
        /// Checks if a float value is approximately zero (accounting for rounding error).
        /// </summary>
        /// <returns>
        /// <c>true</c> if the value is approximately zero.
        /// </returns>
        /// <param name='x'>
        /// The float to check.
        /// </param>
        public static bool ApproximatelyZero(float x)
        {
            return (x < 0.0001f);
        }

        /// <summary>
        /// Converts a web color string to a Color.
        /// </summary>
        /// <returns>
        /// The color.
        /// </returns>
        /// <param name='colorCode'>
        /// A web RGB-format color code of the format "\#rrggbb", where rr, gg, and bb are 
        /// hexadecimal values (e.g., \#ff0000 for red).
        /// </param>
        public static Color WebColor(string colorCode)
        {
            byte r = (colorCode.Length > 2) ? Tools.HexToByte(colorCode.Substring(1, 2)) : (byte)0;
            byte g = (colorCode.Length > 4) ? Tools.HexToByte(colorCode.Substring(3, 2)) : (byte)0;
            byte b = (colorCode.Length > 6) ? Tools.HexToByte(colorCode.Substring(5, 2)) : (byte)0;
            return new Color32(r, g, b, 255);
        }

        /// <summary>
        /// Converts a color of to a web color string.
        /// </summary>
        /// <returns>
        /// The web RGB-format color code of the format "\#rrggbb".
        /// </returns>
        /// <param name='color'>
        /// Color.
        /// </param>
        public static string ToWebColor(Color color)
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}{3:x2}", (int)(255 * color.r), (int)(255 * color.g), (int)(255 * color.b), (int)(255 * color.a));
        }

        public static string StripRichTextCodes(string s)
        {
            if (!s.Contains("<")) return s;
            return Regex.Replace(s, @"<b>|</b>|<i>|</i>|<p>|</p>|<\\/p>|<color=[#]?\w+>|</color>", string.Empty);
        }

        public static string StripTextMeshProTags(string s)
        {
            if (!s.Contains("<")) return s;
            return Regex.Replace(s, @"<[Bb]>|</[Bb]>|<[Ii]>|</[Ii]>|<color=[#]?\w+>|<color=""\w+"">|</color>|" +
                @"<align=\w+>|</align>|<font=[^>]+>|</font>|<indent=\w+\%>|<indent=\w+>|</indent>|" +
                @"<line-height=\w+%>|<line-height=\w+>|</line-height>|<line-indent=\w+\%>|<line-ident=\w+>|</line-ident>|" +
                @"<link=""[^""]+"">|</link>|<lowercase>|</lowercase>|<uppercase>|</uppercase>|" +
                @"<smallcaps>|</smallcaps>|<margin=.+>|<margin-?\w+=.+>|</margin>|<mark=#\w+>|</mark>|" +
                @"<nobr>|</nobr>|<size=\w+\%>|<size=\w+>|</size>|<sprite=.+>|<[Ss]>|</[Ss]>|<[Uu]>|</[Uu]>|" +
                @"<sup>|</sup>|<sub>|</sub>|<p>|</p>|<\\/p>", string.Empty);
        }

        /// <summary>
        /// Determines whether an animation clip is in the animation list.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the clip is in the animation list.
        /// </returns>
        /// <param name='animation'>
        /// The legacy Animation component.
        /// </param>
        /// <param name='clipName'>
        /// The clip name.
        /// </param>
        public static bool IsClipInAnimations(Animation animation, string clipName)
        {
            if (animation != null)
            {
                foreach (AnimationState state in animation)
                {
                    if (string.Equals(state.name, clipName)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds an in-scene GameObject even if it's inactive.
        /// </summary>
        /// <param name="goName">Name of the GameObject.</param>
        /// <returns>The GameObject, or null if not found.</returns>
        public static GameObject GameObjectHardFind(string goName)
        {
            return GameObjectUtility.GameObjectHardFind(goName);
        }

        /// <summary>
        /// Finds an in-scene GameObject matching a name and tag even if it's inactive.
        /// </summary>
        /// <param name="goName">Name of the GameObject.</param>
        /// <param name="tag">Tag.</param>
        /// <returns>The GameObject, or null if not found.</returns>
        public static GameObject GameObjectHardFind(string goName, string tag)
        {
            return GameObjectUtility.GameObjectHardFind(goName, tag);
        }

        /// <summary>
        /// Finds all GameObjects with a specified tag, even inactive GameObjects.
        /// </summary>
        /// <param name="tag">Tag.</param>
        /// <returns>Array of GameObjects with a tag.</returns>
        public static GameObject[] FindGameObjectsWithTagHard(string tag)
        {
            var list = new List<GameObject>();
            var rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                GameObjectSearchForTags(rootGameObjects[i].transform, tag, list);
            }
            return list.ToArray();
        }

        private static void GameObjectSearchForTags(Transform t, string tag, List<GameObject> list)
        {
            if (t == null) return;
            if (string.Equals(t.tag, tag)) list.Add(t.gameObject);
            foreach (Transform child in t)
            {
                GameObjectSearchForTags(child, tag, list);
            }
        }

        /// <summary>
        /// Like GetComponentInChildren(), but also searches parents.
        /// </summary>
        /// <returns>The component, or <c>null</c> if not found.</returns>
        /// <param name="gameObject">Game object to search.</param>
        /// <typeparam name="T">The component type.</typeparam>
        public static T GetComponentAnywhere<T>(GameObject gameObject) where T : Component
        {
            if (!gameObject) return null;
            T component = gameObject.GetComponentInChildren<T>();
            if (component) return component;
            Transform ancestor = gameObject.transform.parent;
            while (!component && ancestor)
            {
                component = ancestor.GetComponentInChildren<T>();
                ancestor = ancestor.parent;
            }
            return component;
        }

        /// <summary>
        /// Gets the height of the game object based on its collider. This only works if the
        /// game object has a CharacterController, CapsuleCollider, BoxCollider, or SphereCollider.
        /// </summary>
        /// <returns>The game object height if it has a recognized type of collider; otherwise <c>0</c>.</returns>
        /// <param name="gameObject">Game object.</param>
        public static float GetGameObjectHeight(GameObject gameObject)
        {
            CharacterController controller = gameObject.GetComponent<CharacterController>();
            if (controller != null)
            {
                return controller.height;
            }
            else
            {
                CapsuleCollider capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
                if (capsuleCollider != null)
                {
                    return capsuleCollider.height;
                }
                else
                {
                    BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
                    if (boxCollider != null)
                    {
                        return boxCollider.center.y + boxCollider.size.y;
                    }
                    else
                    {
                        SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
                        if (sphereCollider != null)
                        {
                            return sphereCollider.center.y + sphereCollider.radius;
                        }
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Sets a component's enabled state to a specified state.
        /// </summary>
        /// <param name="component">Component to set.</param>
        /// <param name="state">State to set the component to (true, false, or flip).</param>
        public static void SetComponentEnabled(Component component, Toggle state)
        {
            bool newValue;
            if (component == null) return;
            if (component is Renderer)
            {
                Renderer targetRenderer = component as Renderer;
                newValue = ToggleUtility.GetNewValue(targetRenderer.enabled, state);
                targetRenderer.enabled = newValue;
            }
            else if (component is Collider)
            {
                Collider targetCollider = component as Collider;
                newValue = ToggleUtility.GetNewValue(targetCollider.enabled, state);
                targetCollider.enabled = newValue;
            }
            else if (component is Animation)
            {
                Animation animationComponent = component as Animation;
                newValue = ToggleUtility.GetNewValue(animationComponent.enabled, state);
                animationComponent.enabled = newValue;
            }
            else if (component is Animator)
            {
                Animator animator = component as Animator;
                newValue = ToggleUtility.GetNewValue(animator.enabled, state);
                animator.enabled = newValue;
            }
            else if (component is AudioSource)
            {
                AudioSource audioSource = component as AudioSource;
                newValue = ToggleUtility.GetNewValue(audioSource.enabled, state);
                audioSource.enabled = newValue;
            }
            else if (component is Behaviour)
            {
                Behaviour behaviour = component as Behaviour;
                newValue = ToggleUtility.GetNewValue(behaviour.enabled, state);
                behaviour.enabled = newValue;
            }
            else
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Don't know how to enable/disable {1}.{2}", new System.Object[] { DialogueDebug.Prefix, component.name, component.GetType().Name }));
                return;
            }
            if (DialogueDebug.logInfo) Debug.Log(string.Format("{0}: {1}.{2}.enabled = {3}", new System.Object[] { DialogueDebug.Prefix, component.name, component.GetType().Name, newValue }));
        }

        public static bool IsCursorActive()
        {
            return IsCursorVisible() && !IsCursorLocked();
        }

        public static void SetCursorActive(bool value)
        {
            ShowCursor(value);
            LockCursor(!value);
        }

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7

		public static bool IsCursorVisible() {
			return Screen.showCursor;
		}

		public static bool IsCursorLocked() {
			return Screen.lockCursor;
		}

		public static void ShowCursor(bool value) {
			Screen.showCursor = value;
		}

		public static void LockCursor(bool value) {
			Screen.lockCursor = value;
		}

#else

        public static bool IsCursorVisible()
        {
            return Cursor.visible;
        }

        public static bool IsCursorLocked()
        {
            return Cursor.lockState != CursorLockMode.None;
        }

        private static CursorLockMode previousLockMode = CursorLockMode.Locked;

        public static void ShowCursor(bool value)
        {
            Cursor.visible = value;
        }

        public static void LockCursor(bool value)
        {
            if (value == false && IsCursorLocked())
            {
                previousLockMode = Cursor.lockState;
            }
            Cursor.lockState = value ? previousLockMode : CursorLockMode.None;
        }

#endif

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2

		public static void LoadLevel(int index) {
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Loading level #" + index);
			Application.LoadLevel(index);
		}

		public static void LoadLevel(string name) {
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Loading level " + name);
            Application.LoadLevel(name);
		}

		public static AsyncOperation LoadLevelAsync(string name) {
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Asynchronously loading level " + name);
            return Application.LoadLevelAsync(name);
		}

        public static AsyncOperation LoadLevelAsync(int index)
        {
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Asynchronously loading level " + index);
            return Application.LoadLevelAsync(index);
        }

        public static string loadedLevelName {
			get { return Application.loadedLevelName; }
		}

#else

        public static void LoadLevel(int index)
        {
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Loading level #" + index);
            SceneManager.LoadScene(index);
        }

        public static void LoadLevel(string name)
        {
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Loading level " + name);
            SceneManager.LoadScene(name);
        }

        public static AsyncOperation LoadLevelAsync(string name)
        {
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Asynchronously loading level " + name);
            return SceneManager.LoadSceneAsync(name);
        }

        public static AsyncOperation LoadLevelAsync(int index)
        {
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Asynchronously loading level " + index);
            return SceneManager.LoadSceneAsync(index);
        }

        public static string loadedLevelName
        {
            get { return SceneManager.GetActiveScene().name; }
        }

#endif

    }

}
