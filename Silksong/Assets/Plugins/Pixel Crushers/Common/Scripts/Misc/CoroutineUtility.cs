// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers
{

    /// <summary>
    /// Utility functions for working with coroutines.
    /// Provides a static endOfFrame that is set to aWaitForEndOfFrame object under
    /// normal operation but is set to null if BATCH_MODE is defined, to allow for
    /// batch mode testing in which WaitForEndOfFrame isn't supported.
    /// </summary>
    public static class CoroutineUtility
    {

#if BATCH_MODE
        public static WaitForEndOfFrame endOfFrame = null;
#else
        public static WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
#endif

//--- Some compilers have issue with this, and it's not really needed anyway.
//#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
//        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//        static void InitStaticVariables()
//        {
//#if BATCH_MODE
//            endOfFrame = null;
//#else
//            endOfFrame = new WaitForEndOfFrame();
//#endif
//        }
//#endif
    }

}
