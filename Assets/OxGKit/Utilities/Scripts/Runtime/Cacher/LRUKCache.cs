using System.Collections.Generic;
using System.Linq;

namespace OxGKit.Utilities.Cacher
{
    public class LRUKCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly int _k;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cache;
        private readonly LinkedList<CacheItem> _lruList;
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

        public LRUKCache(int capacity, int k)
        {
            this._capacity = capacity;
            this._k = k;
            this._cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
            this._lruList = new LinkedList<CacheItem>();
            this._removeCacheHandler = new UnityObjectRemoveCacheHandler<TKey, TValue>();
        }

        public LRUKCache(int capacity, int k, IRemoveCacheHandler<TKey, TValue> removeCacheHandler) : this(capacity, k)
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
                    if (node.Value.Counter < this._k)
                    {
                        node.Value.Counter++;
                    }

                    // 移動到 LRU 列表尾部
                    this._MoveToEndOfLRU(node);
                    return node.Value.Value;
                }
                return default;
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (this._syncRoot)
            {
                if (this._cache.TryGetValue(key, out var node))
                {
                    node.Value.Value = value;
                    if (node.Value.Counter < this._k)
                    {
                        node.Value.Counter++;
                    }

                    this._MoveToEndOfLRU(node);
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

                    newNode.Value.Counter = 1;
                    this._cache.Add(key, newNode);
                    this._lruList.AddLast(newNode);
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (this._syncRoot)
            {
                if (this._cache.TryGetValue(key, out var node))
                {
                    // For remove handler
                    var item = node.Value.Value;
                    this._removeCacheHandler?.RemoveCache(key, item);
                    this._lruList.Remove(node);
                    this._cache.Remove(key);

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
                this._lruList.Clear();
                this._cache.Clear();
            }
        }

        /// <summary>
        /// 將節點移動到 LRU 列表的尾部（最新使用）
        /// </summary>
        private void _MoveToEndOfLRU(LinkedListNode<CacheItem> node)
        {
            // 已經在尾部，則無需移動
            if (this._lruList.Last == node)
                return;

            this._lruList.Remove(node);
            this._lruList.AddLast(node);
        }

        /// <summary>
        /// 淘汰一個最久未使用且人氣最低的項目
        /// </summary>
        protected void Evict()
        {
            lock (this._syncRoot)
            {
                this._EvictNode();
            }
        }

        /// <summary>
        /// 淘汰一個最久未使用且人氣最低的項目，並返回已自列表移除的節點（未淘汰任何項目時返回 null）
        /// </summary>
        /// <returns></returns>
        private LinkedListNode<CacheItem> _EvictNode()
        {
            // 由最久未使用往最新使用尋找 Counter 最小的項目，Counter 相同時取最久未使用者
            // （等同於先求出最小 Counter，再由最久未使用開始尋找第一個符合者）
            LinkedListNode<CacheItem> victim = null;
            int minCounter = int.MaxValue;
            for (var node = this._lruList.First; node != null; node = node.Next)
            {
                if (node.Value.Counter < minCounter)
                {
                    minCounter = node.Value.Counter;
                    victim = node;

                    // Counter 不會小於 0，已是最小值，無需繼續尋找
                    if (minCounter <= 0)
                        break;
                }
            }

            if (victim == null || minCounter > this._k)
                return null;

            var key = victim.Value.Key;
            var item = victim.Value.Value;
            this._removeCacheHandler?.RemoveCache(key, item);
            this._cache.Remove(key);

            // 淘汰時對其餘項目進行衰減
            this.DecrementCounters();
            this._lruList.Remove(victim);

            return victim;
        }

        /// <summary>
        /// 更新 Counter
        /// <para> 最小 Counter 已改為直接由緩存項目求得，不再維護額外的排序結構，此方法僅為相容既有呼叫而保留（不做任何處理）。 </para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="oldCounter"></param>
        /// <param name="newCounter"></param>
        protected void UpdateMinHeap(TKey key, int oldCounter, int newCounter)
        {
        }

        /// <summary>
        /// 尋找目前所有項目的最小 Counter
        /// </summary>
        protected int FindMinCounter()
        {
            lock (this._syncRoot)
            {
                int minCounter = int.MaxValue;
                foreach (var node in this._cache.Values)
                {
                    if (node.Value.Counter < minCounter)
                        minCounter = node.Value.Counter;
                }
                return minCounter;
            }
        }

        /// <summary>
        /// 淘汰後對所有項目進行衰減
        /// </summary>
        protected void DecrementCounters()
        {
            lock (this._syncRoot)
            {
                foreach (var node in this._cache.Values)
                {
                    if (node.Value.Counter > 0)
                    {
                        node.Value.Counter--;
                    }
                }
            }
        }


        private class CacheItem
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public int Counter { get; set; }

            public CacheItem(TKey key, TValue value)
            {
                this.Key = key;
                this.Value = value;
                this.Counter = 0;
            }
        }
    }
}