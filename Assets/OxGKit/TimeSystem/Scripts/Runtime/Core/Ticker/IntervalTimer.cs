using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace OxGKit.TimeSystem
{
    public class IntervalTimer
    {
        private bool _isRunning = false;
        private CancellationTokenSource _cts = null;

        public IntervalTimer()
        {
        }

        ~IntervalTimer()
        {
            this.StopInterval();
        }

        private async UniTaskVoid _SetInterval(bool switchToThread, Action action, int milliseconds, bool ignoreTimeScale, CancellationTokenSource cts)
        {
            try
            {
#if !UNITY_WEBGL
                if (switchToThread) await UniTask.SwitchToThreadPool();
#endif
                do
                {
                    await UniTask.Delay(milliseconds, ignoreTimeScale, PlayerLoopTiming.Update, (cts == null) ? default : cts.Token);
                    action?.Invoke();
                    // Keep looping only while this loop still owns the current run
                    // (StopInterval + SetInterval starts a new loop with a new cts, the previous loop must end here)
                } while (this._isRunning && ReferenceEquals(this._cts, cts));
            }
            catch (Exception ex)
            {
                // A cancelled loop arrives here one tick after StopInterval, if SetInterval was called again
                // in between, this loop must not stop the run that was just started
                if (ReferenceEquals(this._cts, cts))
                    this.StopInterval();
                throw ex;
            }
        }

        public void SetInterval(Action action, int milliseconds, bool ignoreTimeScale = false)
        {
            if (this._isRunning) return;
            this._isRunning = true;
            if (this._cts == null) this._cts = new CancellationTokenSource();
            this._SetInterval(false, action, milliseconds, ignoreTimeScale, this._cts).Forget();
        }

        public void SetIntervalOnThread(Action action, int milliseconds, bool ignoreTimeScale = false)
        {
            if (this._isRunning) return;
            this._isRunning = true;
            if (this._cts == null) this._cts = new CancellationTokenSource();
            this._SetInterval(true, action, milliseconds, ignoreTimeScale, this._cts).Forget();
        }

        public void StopInterval()
        {
            this._isRunning = false;
            if (this._cts == null) return;
            this._cts.Cancel();
            this._cts.Dispose();
            this._cts = null;
        }

        public bool IsRunning()
        {
            return this._isRunning;
        }
    }
}