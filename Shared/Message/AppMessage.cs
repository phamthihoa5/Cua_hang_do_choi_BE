using Shared.Logger;
using Newtonsoft.Json;

namespace Shared.Message;

public class AppMessage
{
    public static readonly Dictionary<string, Dictionary<string, string>> LOCALES =
        new Dictionary<string, Dictionary<string, string>>();

    public static string GetMessage(string locale, string key)
    {
        if (!LOCALES.ContainsKey(locale)) return key;
        var locales = LOCALES[locale];
        return !locales.ContainsKey(key) ? key : locales[key];
    }
    public static string GetMessage(string locale, string key, params object[] obj)
    {
        if (!LOCALES.ContainsKey(locale)) return key;
        var locales = LOCALES[locale];
        return !locales.ContainsKey(key) ? key : string.Format(locales[key], obj);
    }
    public static async Task ReadTranslateFile(string folder)
    {
        if (Directory.Exists(folder))
        {
            var files = Directory.GetFiles(folder).Where(file => file.EndsWith(".json"));
            foreach (var pathFile in files)
            {
                var keyName = Path.GetFileNameWithoutExtension(pathFile);
                try
                {
                    var locale = JsonConvert.DeserializeObject<Dictionary<string, string>>(await File.ReadAllTextAsync(pathFile));
                    if (locale != null)
                    {
                        LOCALES.Add(keyName, locale);
                    }
                }
                catch (Exception e)
                {
                    Logging.Error("Error read translate file: " + pathFile);
                }
            }

            Logging.Info("Read all data from locales folder");
            return;
        }
        Logging.Error($"Folder {folder} not found");
    }
}