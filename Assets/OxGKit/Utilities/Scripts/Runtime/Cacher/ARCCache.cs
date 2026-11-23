using System;
using System.Collections.Generic;
using System.Linq;

namespace OxGKit.Utilities.Cacher
{
    /// <summary>
    /// 以 T1（新加入的項目）與 T2（加入後再次被存取的項目）兩個列表管理的緩存
    /// <para> 淘汰順序：T1 尚有項目時，優先淘汰 T1 中最舊的項目；T1 為空時，才淘汰 T2 中最久未使用的項目。 </para>
    /// </summary>
    public class ARCCache<TKey, TValue>
    {
        private readonly int _capacity;

        // key 對應其所在的列表節點（節點同時保存值），查找、移動與移除皆為 O(1)
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cache;
        private readonly LinkedList<CacheItem> _t1;
        private readonly LinkedList<CacheItem> _t2;
        private readonly object _syncRoot = new object();

        /// <summary>
        /// 特殊處理
        /// </summary>
        private IRemoveCacheHandler<TKey, TValue> _removeCacheHandler;

        public int Count
        {
            get
            {
                lock (this._syncRoot)
                {
                    return this._cache?.Count ?? 0;
                }
            }
        }

        public ARCCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero.");

            this._capacity = capacity;
            this._cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
            this._t1 = new LinkedList<CacheItem>();
            this._t2 = new LinkedList<CacheItem>();
            this._removeCacheHandler = new UnityObjectRemoveCacheHandler<TKey, TValue>();
        }

        public ARCCache(int capacity, IRemoveCacheHandler<TKey, TValue> removeCacheHandler) : this(capacity)
        {
            this._removeCacheHandler = removeCacheHandler;
        }

        public TKey[] GetKeys()
        {
            lock (this._syncRoot)
            {
                return this._cache.Keys.ToArray();
            }
        }

        public bool Contains(TKey key)
        {
            lock (this._syncRoot)
            {
                return this._cache.ContainsKey(key);
            }
        }

        public TValue Get(TKey key)
        {
            lock (this._syncRoot)
            {
                if (this._cache.TryGetValue(key, out var node))
                {
                    this._MoveToFirst(node, this._t2);
                    return node.Value.Value;
                }
                return default;
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (this._syncRoot)
            {
                if (this._cache.TryGetValue(key, out var existingNode))
                {
                    existingNode.Value.Value = value;
                    this._MoveToFirst(existingNode, this._t2);
                }
                else
                {
                    LinkedListNode<CacheItem> newNode = null;

                    if (this._cache.Count >= this._capacity)
                    {
                        // 被淘汰的節點已自列表移除，可直接重複使用（滿載後新增不再配置新的節點與項目）
                        newNode = this._EvictNode();
                    }

                    if (newNode != null && newNode.List == null)
                    {
                        newNode.Value.Key = key;
                        newNode.Value.Value = value;
                    }
                    else
                    {
                        newNode = new LinkedListNode<CacheItem>(new CacheItem(key, value));
                    }

                    this._cache.Add(key, newNode);
                    this._t1.AddFirst(newNode);
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (this._syncRoot)
            {
                if (this._cache.TryGetValue(key, out var node))
                {
                    node.List?.Remove(node);
                    this._RemoveCacheEntry(node);
                    return true;
                }
                return false;
            }
        }

        public void Clear()
        {
            lock (this._syncRoot)
            {
                var keys = this.GetKeys();
                foreach (var key in keys)
                {
                    this.Remove(key);
                }
                this._t1.Clear();
                this._t2.Clear();
                this._cache.Clear();
            }
        }

        protected void PerformEviction()
        {
            lock (this._syncRoot)
            {
                this._EvictNode();
            }
        }

        /// <summary>
        /// 淘汰一個項目（T1 優先，其次 T2），並返回已自列表移除的節點（無項目可淘汰時返回 null）
        /// </summary>
        /// <returns></returns>
        private LinkedListNode<CacheItem> _EvictNode()
        {
            LinkedListNode<CacheItem> node = null;

            if (this._t1.Count > 0)
            {
                node = this._t1.Last;
                this._t1.RemoveLast();
            }
            else if (this._t2.Count > 0)
            {
                node = this._t2.Last;
                this._t2.RemoveLast();
            }

            if (node != null)
                this._RemoveCacheEntry(node);

            return node;
        }

        private void _RemoveCacheEntry(LinkedListNode<CacheItem> node)
        {
            // For remove handler
            var key = node.Value.Key;
            var item = node.Value.Value;
            this._removeCacheHandler?.RemoveCache(key, item);
            this._cache.Remove(key);
            // 釋放對值的引用
            node.Value.Value = default;
        }

        /// <summary>
        /// 將已緩存的 key 移動到 T1 最前面（key 不在緩存內時不處理）
        /// </summary>
        /// <param name="key"></param>
        protected void MoveToT1(TKey key)
        {
            lock (this._syncRoot)
            {
                if (this._cache.TryGetValue(key, out var node))
                    this._MoveToFirst(node, this._t1);
            }
        }

        /// <summary>
        /// 將已緩存的 key 移動到 T2 最前面（key 不在緩存內時不處理）
        /// </summary>
        /// <param name="key"></param>
        protected void MoveToT2(TKey key)
        {
            lock (this._syncRoot)
            {
                if (this._cache.TryGetValue(key, out var node))
                    this._MoveToFirst(node, this._t2);
            }
        }

        /// <summary>
        /// 將節點移動到指定列表的最前面（最新）
        /// </summary>
        /// <param name="node"></param>
        /// <param name="list"></param>
        private void _MoveToFirst(LinkedListNode<CacheItem> node, LinkedList<CacheItem> list)
        {
            // 已經在目標列表的最前面，則無需移動
            if (list.First == node)
                return;

            node.List?.Remove(node);
            list.AddFirst(node);
        }

        private class CacheItem
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }

            public CacheItem(TKey key, TValue value)
            {
                this.Key = key;
                this.Value = value;
            }
        }
    }
}