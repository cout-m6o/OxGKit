using OxGKit.LoggingSystem;

namespace OxGKit.ActionSystem
{
    internal static class Reporter
    {
        public static void OnDone(string targetActionName, string ownerActionName)
        {
            // Build the message only when the logger will actually print it (called every time an action finishes)
            if (!Logging.IsLogActive<Logger>(LogLevel.LogInfo)) return;

            Logging.PrintInfo<Logger>($"[{nameof(ActionSystem)}] {targetActionName} is done and removed from {ownerActionName}.");
        }

        public static void OnRemove(string targetActionName, int uid, string ownerActionName)
        {
            // Build the message only when the logger will actually print it
            if (!Logging.IsLogActive<Logger>(LogLevel.LogDebug)) return;

            Logging.Print<Logger>($"[{nameof(ActionSystem)}] UID: {uid} Removed {targetActionName} from {ownerActionName}");
        }
    }
}
