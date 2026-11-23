using System;
using System.Collections.Generic;

namespace OxGKit.TimeSystem
{
    public static class IntervalSetter
    {
        private const int _MAX_COUNT = 1 << 15;

        // Typed tables instead of a single Dictionary<object, IntervalTimer>:
        // int ids are no longer boxed on every lookup, and the tables grow on demand
        // instead of reserving _MAX_COUNT entries up front (the limit itself is still enforced)
        // Note: timers created without an id are keyed by their hash code, so they share the int table (same as before)
        private static Dictionary<int, IntervalTimer> _intervalTimersByInt = new Dictionary<int, IntervalTimer>();
        private static Dictionary<string, IntervalTimer> _intervalTimersByString = new Dictionary<string, IntervalTimer>();

        private static void _CheckCapacity()
        {
            if (_intervalTimersByInt.Count + _intervalTimersByString.Count >= _MAX_COUNT)
            {
                throw new InvalidOperationException("Interval timer capacity limit exceeded.");
            }
        }

        #region SetInterval
        public static void SetInterval(int id, Action action, int milliseconds, bool ignoreTimeScale = false)
        {
            _CheckCapacity();

            if (!_intervalTimersByInt.ContainsKey(id))
            {
                IntervalTimer intervalTimer = new IntervalTimer();
                _intervalTimersByInt.Add(id, intervalTimer);
                intervalTimer.SetInterval(action, milliseconds, ignoreTimeScale);
            }
        }

        public static void SetIntervalOnThread(int id, Action action, int milliseconds, bool ignoreTimeScale = false)
        {
            _CheckCapacity();

            if (!_intervalTimersByInt.ContainsKey(id))
            {
                IntervalTimer intervalTimer = new IntervalTimer();
                _intervalTimersByInt.Add(id, intervalTimer);
                intervalTimer.SetIntervalOnThread(action, milliseconds, ignoreTimeScale);
            }
        }

        public static void SetInterval(string id, Action action, int milliseconds, bool ignoreTimeScale = false)
        {
            _CheckCapacity();

            if (!_intervalTimersByString.ContainsKey(id))
            {
                IntervalTimer intervalTimer = new IntervalTimer();
                _intervalTimersByString.Add(id, intervalTimer);
                intervalTimer.SetInterval(action, milliseconds, ignoreTimeScale);
            }
        }

        public static void SetIntervalOnThread(string id, Action action, int milliseconds, bool ignoreTimeScale = false)
        {
            _CheckCapacity();

            if (!_intervalTimersByString.ContainsKey(id))
            {
                IntervalTimer intervalTimer = new IntervalTimer();
                _intervalTimersByString.Add(id, intervalTimer);
                intervalTimer.SetIntervalOnThread(action, milliseconds, ignoreTimeScale);
            }
        }

        public static IntervalTimer SetInterval(Action action, int milliseconds, bool ignoreTimeScale = false)
        {
            _CheckCapacity();

            var intervalTimer = new IntervalTimer();
            var id = intervalTimer.GetHashCode();
            if (!_intervalTimersByInt.ContainsKey(id))
            {
                _intervalTimersByInt.Add(id, intervalTimer);
                intervalTimer.SetInterval(action, milliseconds, ignoreTimeScale);
            }
            return intervalTimer;
        }

        public static IntervalTimer SetIntervalOnThread(Action action, int milliseconds, bool ignoreTimeScale = false)
        {
            _CheckCapacity();

            var intervalTimer = new IntervalTimer();
            var id = intervalTimer.GetHashCode();
            if (!_intervalTimersByInt.ContainsKey(id))
            {
                _intervalTimersByInt.Add(id, intervalTimer);
                intervalTimer.SetIntervalOnThread(action, milliseconds, ignoreTimeScale);
            }
            return intervalTimer;
        }

        public static bool CheckIsRunning(int id)
        {
            if (_intervalTimersByInt.TryGetValue(id, out var intervalTimer))
                return intervalTimer.IsRunning();
            return false;
        }

        public static bool TryClearInterval(int id)
        {
            if (_intervalTimersByInt.TryGetValue(id, out var intervalTimer))
            {
                intervalTimer.StopInterval();
                _intervalTimersByInt.Remove(id);
                return true;
            }
            return false;
        }

        public static bool TryClearInterval(string id)
        {
            if (_intervalTimersByString.TryGetValue(id, out var intervalTimer))
            {
                intervalTimer.StopInterval();
                _intervalTimersByString.Remove(id);
                return true;
            }
            return false;
        }

        public static bool TryClearInterval(IntervalTimer intervalTimer)
        {
            if (intervalTimer != null)
            {
                var id = intervalTimer.GetHashCode();
                bool isCleared = TryClearInterval(id);
                if (!isCleared)
                {
                    intervalTimer.StopInterval();
                    isCleared = true;
                }
                return isCleared;
            }
            return false;
        }

        public static void ClearAllIntervalTimers()
        {
            foreach (var intervalTimer in _intervalTimersByInt.Values)
            {
                intervalTimer.StopInterval();
            }
            _intervalTimersByInt.Clear();

            foreach (var intervalTimer in _intervalTimersByString.Values)
            {
                intervalTimer.StopInterval();
            }
            _intervalTimersByString.Clear();
        }
        #endregion
    }
}
