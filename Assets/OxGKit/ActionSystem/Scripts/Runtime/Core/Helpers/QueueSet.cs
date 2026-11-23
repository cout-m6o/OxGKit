using System;
using System.Collections.Generic;

namespace OxGKit.ActionSystem
{
    public class QueueSet<T>
    {
        // Created on first Enqueue (every action owns a sub action QueueSet that usually stays empty)
        private HashSet<T> _set;
        private Queue<T> _queue;

        /// <summary>
        /// Cached snapshot for ToArray (rebuilt only when the collection changed)
        /// </summary>
        private T[] _snapshot;

        public QueueSet()
        {
        }

        public void Enqueue(T item)
        {
            if (this._set == null)
            {
                this._set = new HashSet<T>();
                this._queue = new Queue<T>();
            }

            // HashSet.Add returns false when the item already exists (single lookup)
            if (this._set.Add(item))
            {
                this._queue.Enqueue(item);
                this._snapshot = null;
            }
        }

        public T Dequeue()
        {
            if (this._queue == null)
            {
                this._set = new HashSet<T>();
                this._queue = new Queue<T>();
            }

            // Throws InvalidOperationException when empty (same as before)
            T item = this._queue.Dequeue();
            this._set.Remove(item);
            this._snapshot = null;
            return item;
        }

        public bool Remove(T item)
        {
            if (this._set != null && this._set.Remove(item))
            {
                // Rotate the queue once in place and drop the matching item
                // (keeps the order, without rebuilding a new Queue through LINQ)
                var comparer = EqualityComparer<T>.Default;
                int count = this._queue.Count;
                for (int i = 0; i < count; i++)
                {
                    T current = this._queue.Dequeue();
                    if (!comparer.Equals(current, item))
                        this._queue.Enqueue(current);
                }
                this._snapshot = null;
                return true;
            }
            return false;
        }

        public void Clear()
        {
            if (this._set != null)
            {
                this._set.Clear();
                this._queue.Clear();
            }
            this._snapshot = null;
        }

        public bool Contains(T item)
        {
            return this._set != null && this._set.Contains(item);
        }

        public int Count
        {
            get { return this._queue != null ? this._queue.Count : 0; }
        }

        public T[] ToArray()
        {
            // Reuse the cached snapshot while the collection is unchanged (avoids per-frame allocations)
            if (this._snapshot == null)
                this._snapshot = (this._queue == null || this._queue.Count == 0) ? Array.Empty<T>() : this._queue.ToArray();
            return this._snapshot;
        }
    }
}