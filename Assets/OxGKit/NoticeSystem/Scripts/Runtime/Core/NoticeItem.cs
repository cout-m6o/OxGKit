using OxGKit.LoggingSystem;
using System.Collections.Generic;
using UnityEngine;

namespace OxGKit.NoticeSystem
{
    [DisallowMultipleComponent]
    [AddComponentMenu("OxGKit/NoticeSystem/" + nameof(NoticeItem))]
    public class NoticeItem : MonoBehaviour
    {
        private Dictionary<int, NoticeInfo> _dictNoticeInfos = new Dictionary<int, NoticeInfo>();

        /// <summary>
        /// Get notice infos
        /// </summary>
        /// <returns></returns>
        internal Dictionary<int, NoticeInfo>.ValueCollection GetNoticeInfos()
        {
            return this._dictNoticeInfos.Values;
        }

        /// <summary>
        /// Check has condition id
        /// </summary>
        /// <param name="conditionId"></param>
        /// <returns></returns>
        public bool HasCondition(int conditionId)
        {
            return this._dictNoticeInfos.ContainsKey(conditionId);
        }

        /// <summary>
        /// When data changes must call notify to changes visible
        /// </summary>
        public void Notify()
        {
            foreach (var noticeInfo in this._dictNoticeInfos.Values)
            {
                NoticeManager.Notify(noticeInfo.conditionId);
            }
        }

        /// <summary>
        /// Renew data for value type (Method Chaining)
        /// </summary>
        /// <param name="noticeInfos"></param>
        /// <returns></returns>
        public NoticeItem RenewNotice(NoticeInfo noticeInfo)
        {
            bool isRenewed = false;

            int id = noticeInfo.conditionId;
            if (this.HasCondition(id))
            {
                this._dictNoticeInfos[id] = noticeInfo;
                isRenewed = true;
            }

            // If is not existing will auto register
            if (!isRenewed) this.RegisterNotice(noticeInfo);

            return this;
        }

        /// <summary>
        /// Register notice infos and auto check condition with visible (If register again will assign and renew)
        /// </summary>
        /// <param name="noticeInfos"></param>
        public void RegisterNotice(params NoticeInfo[] noticeInfos)
        {
            if (noticeInfos == null) return;

            for (int i = 0; i < noticeInfos.Length; i++)
            {
                // The indexer adds the id when it is new and overwrites it when it exists (single lookup)
                int id = noticeInfos[i].conditionId;
                this._dictNoticeInfos[id] = noticeInfos[i];
            }

            foreach (var noticeInfo in this._dictNoticeInfos.Values)
            {
                // Register noticeItem by condition id
                NoticeManager.RegisterNoticeItem(noticeInfo.conditionId, this);
            }

            // Auto check condition with visible
            this.CheckConditionAndVisible();
        }

        /// <summary>
        /// Deregister notice infos
        /// </summary>
        /// <param name="conditionIds"></param>
        public void DeregisterNotice(params int[] conditionIds)
        {
            // 自身的 OnDestroy 已完成註銷並釋放緩存之後（例如場景卸載或結束 Play 模式時，其他腳本的 OnDestroy 才調用註銷），不需要再處理。
            if (this._dictNoticeInfos == null) return;

            if (conditionIds == null || conditionIds.Length == 0)
            {
                // Deregister by self conditions
                foreach (var noticeInfo in this._dictNoticeInfos.Values)
                {
                    // Deregister condition and notice item
                    NoticeManager.DeregisterNoticeItem(noticeInfo.conditionId, this);
                }

                // After deregister to clear cache
                this._dictNoticeInfos.Clear();
            }
            else
            {
                for (int i = 0; i < conditionIds.Length; i++)
                {
                    if (this.HasCondition(conditionIds[i]))
                    {
                        NoticeManager.DeregisterNoticeItem(conditionIds[i], this);
                        this._dictNoticeInfos.Remove(conditionIds[i]);
                    }
                }
            }

            // 物件已被銷毀時（未曾啟用過的物件不會收到 OnDestroy，會在銷毀之後才被其他腳本調用註銷），只需要完成註銷，無法也不需要再刷新顯示。
            if (this == null) return;

            this.CheckConditionAndVisible();
        }

        /// <summary>
        /// Check notice condition and visible
        /// </summary>
        internal void CheckConditionAndVisible()
        {
            foreach (var noticeInfo in this._dictNoticeInfos.Values)
            {
                // 將 NoticeItem 的獨立數據交給 NoticeManager 進行條件判斷（條件方法池）
                bool active = NoticeManager.CheckCondition(noticeInfo.conditionId, noticeInfo.data);

                // 任一條件為激活狀態，則開啟顯示
                if (active)
                {
                    if (!this.gameObject.activeSelf)
                    {
                        this.gameObject.SetActive(true);
                        // Build the message (this.name allocates) only when the logger will actually print it
                        if (Logging.IsLogActive<Logger>(LogLevel.LogInfo))
                            Logging.PrintInfo<Logger>($"[{nameof(NoticeSystem)}] (Match Condition) Notify Notice Item {this.name}");
                    }
                    return;
                }
            }

            // 無任何激活條件，則關閉顯示
            this.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            this.DeregisterNotice();
            this._dictNoticeInfos = null;
        }
    }
}