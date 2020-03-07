using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Selenium
{
    public class LocalizationProvider
    {
        #region singleton
        private static LocalizationProvider _instance;

        public static LocalizationProvider Instance => 
            _instance ??= new LocalizationProvider();
        #endregion

        private readonly Dictionary<string, JObject> _languages
            = new Dictionary<string, JObject>();

        private LocalizationProvider()
        {
            var langDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang");

            var languages = Directory.GetFiles(langDir);
            foreach (var language in languages)
            {
                var path = Path.Combine(langDir, language);
                var languageCode = Path.GetFileNameWithoutExtension(language);

                Bot.Logger.Debug("Loading language {LangCode}", languageCode);

                var jsonData = File.ReadAllText(path);

                JObject obj;

                try
                {
                    obj = JObject.Parse(jsonData);
                }
                catch (JsonReaderException e)
                {
                    Bot.Logger.Warning(e, "Failed to load language {Lang}", language);
                    continue;
                }

                _languages[languageCode] = obj;
            }
        }

        public bool IsLanguageValid(string langCode)
            => _languages.ContainsKey(langCode);

        public string GetLocalization(string key, string lang)
        {
            if (!_languages.ContainsKey(lang))
                lang = "en"; // fall back to english

            var language = _languages[lang];

            var token = language.SelectToken(key);

            return (string)token switch
            {
                "" when lang == "en" => key,
                "" => GetLocalization(key, "en"),
                _ => (string) token
            };
        }
    }
}
