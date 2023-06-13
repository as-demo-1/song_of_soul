// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers
{

    /// <summary>
    /// Specifies the reason why SaveSystem.validateSceneName is being called.
    /// </summary>
    public enum SceneValidationMode
    {
        /// <summary>
        /// Just loading a new scene.
        /// </summary>
        LoadingScene, 
        
        /// <summary>
        /// Loading a saved game.
        /// </summary>
        LoadingSavedGame, 
        
        /// <summary>
        /// Resetting the game state and loading the first gameplay scene.
        /// </summary>
        RestartingGame
    }

}
