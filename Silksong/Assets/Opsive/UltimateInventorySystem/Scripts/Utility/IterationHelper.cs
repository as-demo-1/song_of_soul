/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Utility
{
    using Opsive.Shared.Utility;
    using System;
    using System.Collections.Generic;

    public static class IterationHelper
    {
        public const int c_StartSize = 10;

        public delegate int ArrayFuncRef<T>(T mainThis, ref T[] x);
        public static void IterateWithGenericPool<T>(T mainThis, ArrayFuncRef<T> arrayFunc, Action<T, int, T> action) where T : class
        {
            var pooledArray = GenericObjectPool.Get<T[]>();
            var count = arrayFunc.Invoke(mainThis, ref pooledArray);
            for (int i = 0; i < count; i++) {
                action.Invoke(mainThis, i, pooledArray[i]);
            }
            GenericObjectPool.Return(pooledArray);
        }

        public static int GetAllRecursive<T>(ref T[] array, bool includeThis, T mainThis,
            Func<T, IList<T>> func, bool breadthFirstSearch) where T : class
        {
            var count = 0;
            if (includeThis) {
                if (array == null || array.Length == 0) { array = new T[c_StartSize]; }
                array[0] = mainThis;
                count++;
            }

            var list = func.Invoke(mainThis);
            if (list == null) { return count; }

            if (breadthFirstSearch) {
                var queue = new Queue<T>();
                for (int i = 0; i < list.Count; ++i) {
                    queue.Enqueue(list[i]);
                    AddToArray(ref array, ref count, list[i], true);
                }

                while (queue.Count > 0) {
                    var nextElement = queue.Dequeue();
                    count = GetAllRecursiveBFS(ref array, count, nextElement, func, queue);
                }
            } else {
                for (int i = 0; i < list.Count; ++i) {
                    count = GetAllRecursiveDFS(ref array, count, list[i], func);
                }
            }

            return count;
        }

        static void AddToArray<T>(ref T[] array, ref int index, T element, bool avoidDuplicate)
        {
            if (array == null || array.Length == 0) { array = new T[c_StartSize]; } else if (array.Length == index) {
                Array.Resize(ref array, index * 2);
            }

            if (avoidDuplicate) {
                for (int i = 0; i < index; i++) {
                    if (ReferenceEquals(array[i], element)) { return; }
                }
            }

            array[index] = element;
            index++;
        }

        static int GetAllRecursiveBFS<T>(ref T[] array, int index, T subObject, Func<T, IList<T>> func, Queue<T> queue)
            where T : class
        {
            var list = func.Invoke(subObject);
            if (list == null) { return index; }

            for (int i = 0; i < list.Count; ++i) {
                queue.Enqueue(list[i]);
                AddToArray(ref array, ref index, list[i], true);
            }

            return index;
        }

        /// <summary>
        /// Get all the elements using Recursive Depth First Search.
        /// </summary>
        /// <param name="array">Reference to the array containing all the elements.</param>
        /// <param name="index">The index of depth.</param>
        /// <param name="subObject">The current object to look at.</param>
        /// <param name="func">The function to use to look within the object.</param>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <returns>The count.</returns>
        public static int GetAllRecursiveDFS<T>(ref T[] array, int index, T subObject, Func<T, IList<T>> func)
            where T : class
        {
            if (array == null || array.Length == 0) { array = new T[c_StartSize]; } else if (array.Length == index) {
                Array.Resize(ref array, index * 2);
            }

            //Check if it was already taken into account
            for (int i = 0; i < index; i++) {
                if (ReferenceEquals(array[i], subObject)) { return index; }
            }

            array[index] = subObject;
            index++;
            var arrayStruct = func.Invoke(subObject);
            if (arrayStruct != null) {
                for (int i = 0; i < arrayStruct.Count; ++i) {
                    index = GetAllRecursiveDFS(ref array, index, arrayStruct[i], func);
                }
            }

            return index;
        }

        /// <summary>
        /// Get all the elements using Recursive Depth First Search.
        /// </summary>
        /// <param name="array">Reference to the array.</param>
        /// <param name="includeThis">Should the search include the object.</param>
        /// <param name="mainThis">The object to look at.</param>
        /// <param name="func">The function to look within the object.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>The count.</returns>
        public static int GetAllRecursiveDFS<T>(ref T[] array, bool includeThis, T mainThis,
            Func<T, T> func) where T : class
        {
            var index = 0;
            if (includeThis) {
                if (array == null || array.Length == 0) { array = new T[c_StartSize]; }
                array[0] = mainThis;
                index++;
            }

            var count = GetAllRecursiveDFS(ref array, index, func.Invoke(mainThis), func);

            return count;
        }

        /// <summary>
        /// Get all the elements using Recursive Depth First Search.
        /// </summary>
        /// <param name="array">Reference to the array containing all the elements.</param>
        /// <param name="index">The index of depth.</param>
        /// <param name="subObject">The current object to look at.</param>
        /// <param name="func">The function to use to look within the object.</param>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <returns>The count.</returns>
        private static int GetAllRecursiveDFS<T>(ref T[] array, int index, T subObject,
            Func<T, T> func) where T : class
        {
            if (subObject == null) { return index; }

            if (array == null || array.Length == 0) { array = new T[c_StartSize]; } else if (array.Length == index) {
                Array.Resize(ref array, index * 2);
            }

            array[index] = subObject;
            index++;

            var count = GetAllRecursiveDFS(ref array, index, func.Invoke(subObject), func);

            return count;
        }

        /// <summary>
        /// Get the leafs within the object tree.
        /// </summary>
        /// <param name="array">Reference to the array containing all the elements.</param>
        /// <param name="mainThis">The object to look at.</param>
        /// <param name="func">The function to look within the object.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>The count of element.</returns>
        public static int GetLeafRecursiveDFS<T>(ref T[] array, T mainThis,
            Func<T, IList<T>> func) where T : class
        {
            var arrayStruct = func.Invoke(mainThis);

            if (arrayStruct.Count == 0) {
                if (array == null || array.Length == 0) { array = new T[1]; }
                array[0] = mainThis;
                return 1;
            }

            var index = 0;
            for (int i = 0; i < arrayStruct.Count; ++i) {
                index = GetLeafRecursiveDFS(ref array, index, arrayStruct[i], func);
            }

            return index;
        }

        /// <summary>
        /// Get the leafs within the object tree.
        /// </summary>
        /// <param name="array">Reference to the array containing all the elements.</param>
        /// <param name="index">The index of depth.</param>
        /// <param name="subObject">The current object to look at.</param>
        /// <param name="func">The function to use to look within the object.</param>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <returns>The count.</returns>
        private static int GetLeafRecursiveDFS<T>(ref T[] array, int index, T subObject, Func<T, IList<T>> func)
        where T : class
        {
            if (array == null || array.Length == 0) { array = new T[c_StartSize]; } else if (array.Length == index) {
                Array.Resize(ref array, index * 2);
            }

            var arrayStruct = func.Invoke(subObject);
            if (arrayStruct.Count == 0) {
                for (int i = 0; i < index; i++) {
                    if (ReferenceEquals(array[i], subObject)) { return index; }
                }
                array[index] = subObject;
                return index + 1;
            }

            for (int i = 0; i < arrayStruct.Count; ++i) {
                index = GetLeafRecursiveDFS(ref array, index, arrayStruct[i], func);
            }

            return index;
        }

        /// <summary>
        /// Get the leafs within the object tree with a condition.
        /// </summary>
        /// <param name="array">Reference to the array containing all the elements.</param>
        /// <param name="mainThis">The object to look at.</param>
        /// <param name="func">The function to look within the object.</param>
        /// <param name="condition">The condition.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>The count.</returns>
        public static int GetLeafConditionalRecursiveDFS<T>(ref T[] array, T mainThis,
            Func<T, IList<T>> func, Func<T, bool> condition) where T : class
        {
            var arrayStruct = func.Invoke(mainThis);

            if (arrayStruct.Count == 0 && condition.Invoke(mainThis)) {
                if (array == null || array.Length == 0) { array = new T[1]; }
                array[0] = mainThis;
                return 1;
            }

            var index = 0;
            for (int i = 0; i < arrayStruct.Count; ++i) {
                if (!condition.Invoke(arrayStruct[i])) { continue; }

                index = GetLeafConditionalRecursiveDFS(ref array, index, arrayStruct[i], func, condition);
            }

            if (index == 0) {
                if (array == null || array.Length == 0) { array = new T[1]; }
                array[0] = mainThis;
                return 1;
            }

            return index;
        }

        /// <summary>
        /// Get the leafs within the object tree.
        /// </summary>
        /// <param name="array">Reference to the array containing all the elements.</param>
        /// <param name="index">The index of depth.</param>
        /// <param name="subObject">The current object to look at.</param>
        /// <param name="func">The function to look within the object.</param>
        /// <param name="condition">The condition.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>The count.</returns>
        private static int GetLeafConditionalRecursiveDFS<T>(ref T[] array, int index, T subObject, Func<T, IList<T>> func, Func<T, bool> condition)
            where T : class
        {
            if (array == null || array.Length == 0) { array = new T[c_StartSize]; } else if (array.Length == index) {
                Array.Resize(ref array, index * 2);
            }

            var arrayStruct = func.Invoke(subObject);
            if (arrayStruct.Count == 0 && condition.Invoke(subObject)) {
                for (int i = 0; i < index; i++) {
                    if (ReferenceEquals(array[i], subObject)) { return index; }
                }
                array[index] = subObject;
                return index + 1;
            }

            var match = false;
            for (int i = 0; i < arrayStruct.Count; ++i) {
                if (!condition.Invoke(arrayStruct[i])) { continue; }

                match = true;
                index = GetLeafConditionalRecursiveDFS(ref array, index, arrayStruct[i], func, condition);
            }

            if (match != false) { return index; }


            for (int i = 0; i < index; i++) {
                if (ReferenceEquals(array[i], subObject)) { return index; }
            }
            array[index] = subObject;
            return index + 1;


        }
    }
}
