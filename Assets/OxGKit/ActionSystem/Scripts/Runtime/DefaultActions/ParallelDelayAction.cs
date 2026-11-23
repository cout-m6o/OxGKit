using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace OxGKit.ActionSystem
{
    public class ParallelDelayAction : ActionBase
    {
        protected QueueSet<ActionBase> _queueRunningActions = new QueueSet<ActionBase>();
        protected float _delayTime = 0f;

        /// <summary>
        /// Actions still waiting for their staggered start (must not be updated before RunStart)
        /// </summary>
        private HashSet<ActionBase> _pendingStartActions;

        #region Default Constructor
        public ParallelDelayAction()
        {
            this.name = nameof(ParallelDelayAction);
        }

        public ParallelDelayAction(int uid) : this()
        {
            this.uid = uid;
        }

        public ParallelDelayAction(string name)
        {
            this.name = string.IsNullOrEmpty(name) ? nameof(ParallelDelayAction) : name;
        }

        public ParallelDelayAction(string name, int uid) : this(name)
        {
            this.uid = uid;
        }
        #endregion

        public ParallelDelayAction(float delayTime) : this()
        {
            this._delayTime = delayTime;
        }

        protected override void OnStart()
        {
            this.SetDuration(-1);

            this.StartParallelActions();
        }

        protected override void OnUpdate(float dt)
        {
            this.UpdateParallelActions(dt);

            if (this._IsAllDone())
            {
                this.MarkAsDone();
                this.ClearActions();
            }
        }

        public ParallelDelayAction AddAction(ActionBase action)
        {
            if (action != null)
            {
                if (!this._queueRunningActions.Contains(action))
                {
                    this._queueRunningActions.Enqueue(action);
                }
            }
            return this;
        }

        protected void ClearActions()
        {
            this._queueRunningActions.Clear();
        }

        protected async void StartParallelActions()
        {
            var actions = this._queueRunningActions.ToArray();

            // With a delay the actions start one after another across frames,
            // so mark them as pending first (the update loop leaves them alone until they have started)
            HashSet<ActionBase> pendingStartActions = null;
            if (this._delayTime > 0f && actions.Length > 0)
            {
                pendingStartActions = new HashSet<ActionBase>(actions);
                this._pendingStartActions = pendingStartActions;
            }

            foreach (var action in actions)
            {
                pendingStartActions?.Remove(action);
                action.RunStart();
                if (this._delayTime > 0f) await UniTask.Delay(TimeSpan.FromSeconds(this._delayTime));
            }
        }

        protected void UpdateParallelActions(float dt)
        {
            foreach (var action in this._queueRunningActions.ToArray())
            {
                // Skip the action that is already done and keep updating the others
                // (returning here stopped every action behind the first finished one)
                if (action.IsAllDone()) continue;

                // Not started yet, updating it now would finish it immediately (its duration is only set on start)
                if (this._pendingStartActions != null &&
                    this._pendingStartActions.Count > 0 &&
                    this._pendingStartActions.Contains(action))
                    continue;

                action.RunUpdate(dt);
            }
        }

        private bool _IsAllDone()
        {
            foreach (var action in this._queueRunningActions.ToArray())
            {
                if (!action.IsAllDone()) return false;
            }

            return true;
        }
    }
}