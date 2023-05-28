// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Database reset options used by DatabaseManager.Reset.
    /// 
    /// - KeepAllLoaded: Keeps all currently-loaded databases and just resets them to their
    /// initial states.
    /// - RevertToDefault: Removes all but the default database and resets it to its initial state.
    /// </summary>
    public enum DatabaseResetOptions
    {

        /// <summary>
        /// Keep all currently-loaded databases and just reset them to their initial states
        /// </summary>
        KeepAllLoaded,

        /// <summary>
        /// Remove all but the default database and reset it to its initial state
        /// </summary>
        RevertToDefault
    }

}
