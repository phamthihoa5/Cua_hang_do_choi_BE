using Serilog;

namespace Shared.Logger;

public class Logging
{
    public static void Info(string message, params object[] obj)
    {
        Log.Logger.Information(message, obj);
    }

    public static void Debug(string message, params object[] obj)
    {
        Log.Logger.Debug(message, obj);
    }

    public static void Error(string message, params object[] obj)
    {
        Log.Logger.Error(message, obj);
    }

    public static void Error(string message, Exception e)
    {
        Log.Logger.Error($"{message}:\n{e.Message}", e);
    }

    public static void Warning(string message, params object[] obj)
    {
        Log.Logger.Warning(message, obj);
    }
}