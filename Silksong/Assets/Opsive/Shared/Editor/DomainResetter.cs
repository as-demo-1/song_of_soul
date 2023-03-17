/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor
{
#if UNITY_2019_3_OR_NEWER
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using UnityEngine;
#endif

    /// <summary>
    /// Enables the shared classes to work with Unity's Fast Enter Playmode feature:
    /// https://docs.unity3d.com/Manual/ConfigurableEnterPlayMode.html.
    /// </summary>
    public static class DomainResetter
    {
        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void DomainReset()
        {
            EventHandler.DomainReset();
            GameObjectExtensions.DomainReset();
            ObjectPoolBase.DomainReset();
            SchedulerBase.DomainReset();
        }
#endif
    }
}