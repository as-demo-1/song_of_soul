// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// The persistent position data component works with the PersistentDataManager to keep track 
    /// of a game object's position when saving and loading games or changing levels.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class PersistentPositionData : MonoBehaviour
    {

        /// <summary>
        /// (Optional) Normally, this component uses the game object's name as the name of the 
        /// actor in the Lua Actor[] table. If your actor is named differently in the Lua Actor[] 
        /// table (e.g., the actor has a different name in Chat Mapper or the DialogueDatabase), 
        /// then set this property to the Lua name.
        /// </summary>
        public string overrideActorName;

        /// <summary>
        /// If <c>true</c>, the object's current level is also recorded; and, on load, the 
        /// position is only applied if the current level matches the recorded level.
        /// </summary>
        public bool recordCurrentLevel = true;

        [HideInInspector] // Deprecated.
        public bool restoreCurrentLevelPosition = true;

        private string actorName
        {
            get { return string.IsNullOrEmpty(overrideActorName) ? DialogueActor.GetPersistentDataName(gameObject.transform) : overrideActorName; }
        }

        protected virtual void OnEnable()
        {
            PersistentDataManager.RegisterPersistentData(gameObject);
        }

        protected virtual void OnDisable()
        {
            PersistentDataManager.UnregisterPersistentData(gameObject);
        }

        public void Start()
        {
            if (string.IsNullOrEmpty(overrideActorName))
            {
                overrideActorName = DialogueActor.GetPersistentDataName(transform);
            }
        }

        /// <summary>
        /// Listens for the OnRecordPersistentData message and records the game object's position 
        /// and rotation into the Lua Actor[] table.
        /// </summary>
        public void OnRecordPersistentData()
        {
            string positionString = GetPositionString();
            var fieldName = recordCurrentLevel ? "Position_" + SanitizeLevelName(Tools.loadedLevelName) : "Position";
            if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Persistent Position Data Actor[" + actorName + "]." + fieldName + "='" + positionString + "'", this);
            DialogueLua.SetActorField(actorName, fieldName, positionString);
        }

        /// <summary>
        /// Listens for the OnApplyPersistentData message and retrieves the game object's position 
        /// and rotation from the Lua Actor[] table.
        /// </summary>
        public void OnApplyPersistentData()
        {
            string spawnpointName = DialogueLua.GetActorField(actorName, "Spawnpoint").asString;
            if (!string.IsNullOrEmpty(spawnpointName))
            {
                var spawnpoint = Tools.GameObjectHardFind(spawnpointName);
                if (spawnpoint == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Persistent Position Data found Actor[" + actorName + "].Spawnpoint value '" + spawnpointName + "' but can't find a GameObject with this name in the scene. Moving actor to saved position instead.", this);
                }
                else
                {
                    transform.position = spawnpoint.transform.position;
                    transform.rotation = spawnpoint.transform.rotation;
                    if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Persistent Position Data spawning " + actorName + " at spawnpoint " + spawnpoint, this);
                }
                DialogueLua.SetActorField(actorName, "Spawnpoint", string.Empty);
                if (spawnpoint != null) return;
            }
            var fieldName = recordCurrentLevel ? "Position_" + SanitizeLevelName(Tools.loadedLevelName) : "Position";
            var positionString = DialogueLua.GetActorField(actorName, fieldName).asString;
            if (!string.IsNullOrEmpty(positionString))
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Persistent Position Data restoring " + actorName + " to position " + positionString, this);
                ApplyPositionString(positionString);
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Persistent Position Data Actor[" + actorName + "]." + fieldName + " is blank. Not moving " + actorName, this);
            }
        }

        private string GetPositionString()
        {
            string optionalLevelName = recordCurrentLevel ? DialogueLua.DoubleQuotesToSingle("," + Tools.loadedLevelName) : string.Empty;
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6}{7}",
                new System.Object[] { transform.position.x, transform.position.y, transform.position.z,
                transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w,
                optionalLevelName });
        }

        private void ApplyPositionString(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Equals("nil")) return;
            string[] tokens = s.Split(',');
            if ((7 <= tokens.Length) && (tokens.Length <= 8))
            {
                if (recordCurrentLevel)
                {
                    if ((tokens.Length == 8) && !string.Equals(tokens[7], Tools.loadedLevelName))
                    {
                        return; // This is not the recorded level. Don't apply position.
                    }
                }
                float[] values = new float[7];
                for (int i = 0; i < 7; i++)
                {
                    values[i] = 0;
                    float.TryParse(tokens[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out values[i]);
                }
                transform.position = new Vector3(values[0], values[1], values[2]);
                transform.rotation = new Quaternion(values[3], values[4], values[5], values[6]);
            }
        }

        public static string SanitizeLevelName(string levelName)
        {
            return DialogueLua.StringToTableIndex(levelName).Replace(".", "_");
        }

    }

}
