using System;
using System.Collections.Generic;

namespace Gamekit2D
{
    /// <summary>
    /// A fast priority queue or heap implementation.
    /// </summary>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        /// <summary>
        /// The internal list used by the queue. Use with care.
        /// </summary>
        public readonly List<T> items;

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public PriorityQueue()
        {
            items = new List<T>();
        }

        public bool Empty
        {
            get { return items.Count == 0; }
        }

        public T First
        {
            get
            {
                if (items.Count > 1)
                {
                    return items[0];
                }
                return items[items.Count - 1];
            }
        }

        public void Push(T item)
        {
            lock (this)
            {
                items.Add(item);
                SiftDown(0, items.Count - 1);
            }
        }

        public T Pop()
        {
            lock (this)
            {
                T item;
                var last = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                if (items.Count > 0)
                {
                    item = items[0];
                    items[0] = last;
                    SiftUp(0);
                }
                else
                {
                    item = last;
                }
                return item;
            }
        }

        int Compare(T A, T B)
        {
            return A.CompareTo(B);
        }

        void SiftDown(int startpos, int pos)
        {
            var newitem = items[pos];
            while (pos > startpos)
            {
                var parentpos = (pos - 1) >> 1;
                var parent = items[parentpos];
                if (Compare(parent, newitem) <= 0)
                    break;
                items[pos] = parent;
                pos = parentpos;
            }
            items[pos] = newitem;
        }

        void SiftUp(int pos)
        {
            var endpos = items.Count;
            var startpos = pos;
            var newitem = items[pos];
            var childpos = 2 * pos + 1;
            while (childpos < endpos)
            {
                var rightpos = childpos + 1;
                if (rightpos < endpos && Compare(items[rightpos], items[childpos]) <= 0)
                    childpos = rightpos;
                items[pos] = items[childpos];
                pos = childpos;
                childpos = 2 * pos + 1;
            }
            items[pos] = newitem;
            SiftDown(startpos, pos);
        }
    }
}