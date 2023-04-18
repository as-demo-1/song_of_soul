/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.Utility
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Helper class which will get an InspectorDrawer for the specified type.
    /// </summary>
    public class InspectorDrawerUtility
    {
        private static Dictionary<Type, InspectorDrawer> s_InspectorDrawerTypeMap;
        private static List<System.Reflection.Assembly> s_LoadedAssemblies;
        private static Dictionary<string, List<Type>> s_TypeLookup = new Dictionary<string, List<Type>>();

        /// <summary>
        /// The object has been enabled again.
        /// </summary>
        public static void OnEnable()
        {
            s_InspectorDrawerTypeMap = null;
        }

        /// <summary>
        /// Returns the InspectorDrawer for the specified type.
        /// </summary>
        /// <param name="type">The type to retrieve the InspectorDrawer of.</param>
        /// <returns>The found InspectorDrawer. Can be null.</returns>
        public static InspectorDrawer InspectorDrawerForType(Type type)
        {
            if (type == null) {
                return null;
            }

            if (s_InspectorDrawerTypeMap == null) {
                BuildInspectorDrawerMap();
            }

            InspectorDrawer inspectorDrawer;
            if (!s_InspectorDrawerTypeMap.TryGetValue(type, out inspectorDrawer)) {
                return InspectorDrawerForType(type.BaseType);
            }

            return inspectorDrawer;
        }

        /// <summary>
        /// Builds the dictionary which contains all of the InspectorDrawers.
        /// </summary>
        private static void BuildInspectorDrawerMap()
        {
            s_InspectorDrawerTypeMap = new Dictionary<Type, InspectorDrawer>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must derive from InspectorDrawer.
                    if (!typeof(InspectorDrawer).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    // Create the InspectorDrawer if the type has the InspectorDrawerAttribute.
                    InspectorDrawerAttribute[] attribute;
                    if ((attribute = assemblyTypes[j].GetCustomAttributes(typeof(InspectorDrawerAttribute), false) as InspectorDrawerAttribute[]).Length > 0 &&
                        !s_InspectorDrawerTypeMap.ContainsKey(attribute[0].Type)) {
                        var inspectorDrawer = Activator.CreateInstance(assemblyTypes[j]) as InspectorDrawer;
                        s_InspectorDrawerTypeMap.Add(attribute[0].Type, inspectorDrawer);
                    }
                }
            }
        }

        /// <summary>
        /// Searches through all of the loaded assembies for the types within the specified namespace.
        /// </summary>
        /// <param name="name">The string value of the namespace.</param>
        /// <returns>The found types. Can be null.</returns>
        public static List<Type> GetAllTypesWithinNamespace(string name)
        {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }

            List<Type> types;
            // Cache the results for quick repeated lookup.
            if (s_TypeLookup.TryGetValue(name, out types)) {
                return types;
            }

            types = new List<Type>();
            if (s_LoadedAssemblies == null) {
                s_LoadedAssemblies = new List<System.Reflection.Assembly>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; ++i) {
                    s_LoadedAssemblies.Add(assemblies[i]);
                }
            }

            for (int i = 0; i < s_LoadedAssemblies.Count; ++i) {
                var assemblyTypes = s_LoadedAssemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    if (!assemblyTypes[j].IsClass || assemblyTypes[j].IsAbstract || assemblyTypes[j].Namespace != name ) {
                        continue;
                    }
                    types.Add(assemblyTypes[j]);
                }
            }

            s_TypeLookup.Add(name, types);
            return types;
        }
    }
}