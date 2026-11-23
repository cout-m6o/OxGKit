using OxGKit.LoggingSystem;
using System.Collections.Generic;
using UnityEngine;

namespace OxGKit.NoticeSystem
{
    public static class NoticeManager
    {
        private static Dictionary<int, NoticeCondition> _dictNoticeConditions; // 針對 Cond Id 建立條件池緩存
        private static Dictionary<int, ListSet<NoticeItem>> _dictNoticeItems;  // 針對 Cond Id 註冊 NoticeItem 列表
        private static HashSet<int> _limiterConditionIds;                      // 緩存 Notify 限制唯一 Cond Id

        static NoticeManager()
        {
            _dictNoticeConditions = new Dictionary<int, NoticeCondition>();
            _dictNoticeItems = new Dictionary<int, ListSet<NoticeItem>>();
            _limiterConditionIds = new HashSet<int>();
        }

        #region Public Methods
        /// <summary>
        /// Register a notice condition
        /// </summary>
        /// <typeparam name="TNoticeCondition"></typeparam>
        public static void RegisterCondition<TNoticeCondition>() where TNoticeCondition : NoticeCondition, new()
        {
            var type = typeof(TNoticeCondition);
            var hash = type.GetHashCode();
            var noticeCondition = new TNoticeCondition();

            if (_dictNoticeConditions.ContainsKey(hash))
            {
                Logging.PrintWarning<Logger>($"[{nameof(NoticeSystem)}] Repeat Register Notice Condition {type.Name}");
                return;
            }

            noticeCondition.SetId(hash);
            _dictNoticeConditions.Add(hash, noticeCondition);
            Logging.Print<Logger>($"[{nameof(NoticeSystem)}] Register Notice Condition {type.Name}");
        }

        /// <summary>
        /// Get notice condition id
        /// </summary>
        /// <typeparam name="TNoticeCondition"></typeparam>
        /// <returns></returns>
        public static int GetConditionId<TNoticeCondition>() where TNoticeCondition : NoticeCondition
        {
            var type = typeof(TNoticeCondition);
            var hash = type.GetHashCode();

            _dictNoticeConditions.TryGetValue(hash, out var noticeCondition);
            return noticeCondition?.GetId() ?? 0;
        }

        /// <summary>
        /// Notify by single condition id, when data changes (non-alloc)
        /// </summary>
        /// <param name="conditionId"></param>
        public static void Notify(int conditionId)
        {
            // 檢查是否有符合 condition id 的條件池
            if (_dictNoticeConditions.ContainsKey(conditionId))
            {
                // 檢查是否有符合 condition id 的通知物件
                if (_dictNoticeItems.TryGetValue(conditionId, out ListSet<NoticeItem> noticeItems))
                {
                    for (int i = noticeItems.Count - 1; i >= 0; i--)
                    {
                        var noticeItem = noticeItems.GetList()[i];
                        if (noticeItem != null && !noticeItem.gameObject.IsDestroyed())
                        {
                            noticeItem.CheckConditionAndVisible();
                        }
                        else
                        {
                            noticeItems.RemoveAt(i);
                            Logging.PrintWarning<Logger>($"[{nameof(NoticeSystem)}] Removed NoticeItem since it was either missing or already destroyed!");
                        }
                    }
                }
            }
            else Logging.PrintError<Logger>($"[{nameof(NoticeSystem)}] Error Notice Condition Cannot find => Cond Id: {conditionId}");
        }

        /// <summary>
        /// Notify by condition ids, when data changes
        /// </summary>
        /// <param name="conditionIds"></param>
        public static void Notify(params int[] conditionIds)
        {
            if (conditionIds == null) return;

            foreach (int conditionId in conditionIds)
            {
                Notify(conditionId);
            }
        }

        /// <summary>
        /// Notify by notice items, when data changes
        /// </summary>
        /// <param name="noticeItems"></param>
        public static void Notify(params NoticeItem[] noticeItems)
        {
            if (noticeItems == null) return;

            foreach (var noticeItem in noticeItems)
            {
                // Collect notify condition id and remove duplicates
                foreach (var noticeInfo in noticeItem.GetNoticeInfos())
                {
                    NotifyCollector(noticeInfo.conditionId);
                }
            }

            // Notify all after collect 
            NotifyAll();
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Collect notify condition id
        /// </summary>
        /// <param name="conditionId"></param>
        internal static void NotifyCollector(int conditionId)
        {
            // HashSet.Add ignores the id that already exists (single lookup)
            _limiterConditionIds.Add(conditionId);
        }

        /// <summary>
        /// Notify all by condition id collector
        /// </summary>
        internal static void NotifyAll()
        {
            if (_limiterConditionIds.Count > 0)
            {
                foreach (int conditionId in _limiterConditionIds)
                {
                    Notify(conditionId);
                }

                // After notify all must clear limiter for next notify collection
                _limiterConditionIds.Clear();
            }
        }

        /// <summary>
        /// Check notice condition
        /// </summary>
        /// <param name="conditionId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool CheckCondition(int conditionId, object data)
        {
            // 檢查是否有符合的 condition id 的條件池
            if (_dictNoticeConditions.TryGetValue(conditionId, out NoticeCondition noticeCondition))
            {
                // 進行條件過濾（針對不同的數據進行判斷）
                return noticeCondition.ShowCondition(data);
            }
            else Logging.PrintError<Logger>($"[{nameof(NoticeSystem)}] Error Notice Condition Cannot find => Cond Id: {conditionId}");

            return false;
        }

        /// <summary>
        /// Register notice item by condition id
        /// </summary>
        /// <param name="redDotType"></param>
        /// <param name="item"></param>
        internal static void RegisterNoticeItem(int conditionId, NoticeItem noticeItem)
        {
            // The messages below read noticeItem.name (allocates a string on every access),
            // so they are only built when the logger will actually print them
            bool isLogActive = Logging.IsLogActive<Logger>(LogLevel.LogDebug);

            if (_dictNoticeItems.TryGetValue(conditionId, out ListSet<NoticeItem> noticeItems))
            {
                // If condition id already has same noticeItem just assign directly (renew)
                if (noticeItems.Contains(noticeItem))
                {
                    noticeItems.Assign(noticeItem);
                    if (isLogActive) Logging.Print<Logger>($"[{nameof(NoticeSystem)}] Renew Notice Item {noticeItem.name}");
                }
                else
                {
                    // Add a noticeItem
                    noticeItems.Add(noticeItem);
                    if (isLogActive) Logging.Print<Logger>($"[{nameof(NoticeSystem)}] Register Notice Item {noticeItem.name}");
                }
            }
            else
            {
                noticeItems = new ListSet<NoticeItem>();
                noticeItems.Add(noticeItem);
                _dictNoticeItems.Add(conditionId, noticeItems);
                if (isLogActive) Logging.Print<Logger>($"[{nameof(NoticeSystem)}] Register Notice Item {noticeItem.name}");
            }
        }

        /// <summary>
        /// Deregister notice item by condition id
        /// </summary>
        /// <param name="item"></param>
        internal static void DeregisterNoticeItem(int conditionId, NoticeItem noticeItem)
        {
            if (_dictNoticeItems.TryGetValue(conditionId, out ListSet<NoticeItem> noticeItems))
            {
                // Remove returns false when the noticeItem is not registered
                if (noticeItems.Remove(noticeItem))
                {
                    // Build the message (noticeItem.name allocates) only when the logger will actually print it.
                    // The item may already be destroyed here (deregistered from another script's OnDestroy), its name cannot be read then.
                    if (Logging.IsLogActive<Logger>(LogLevel.LogDebug))
                        Logging.Print<Logger>($"[{nameof(NoticeSystem)}] Deregister Notice Item {(noticeItem != null ? noticeItem.name : "(destroyed)")}");
                }
            }
        }
        #endregion
    }

    internal static class GameObjectExtensions
    {
        /// <summary>
        /// Checks if a GameObject has been destroyed.
        /// </summary>
        /// <param name="gameObject">GameObject reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }
    }
}
