// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This static utility class for working with dialogue databases.
    /// </summary>
    public static class DatabaseUtility
    {

        public static DialogueDatabase CreateDialogueDatabaseInstance(bool createDefaultAssets = false)
        {
            var template = Template.FromDefault();
            var wrapperType = RuntimeTypeUtility.GetWrapperType(typeof(DialogueDatabase)) ?? typeof(DialogueDatabase);
            var database = ScriptableObjectUtility.CreateScriptableObject(wrapperType) as DialogueDatabase;
            database.ResetEmphasisSettings();
            if (createDefaultAssets)
            {
                database.actors.Add(template.CreateActor(1, "Player", true));
                database.variables.Add(template.CreateVariable(1, "Alert", string.Empty));
            }
            return database;
        }

    }

}
