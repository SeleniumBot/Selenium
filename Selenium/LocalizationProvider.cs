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

            var token = (string)language.SelectToken(key);

            // disable resharper heuristic here because it is actually reachable,
            // just doesn't realise it.

            // ReSharper disable HeuristicUnreachableCode
            return token switch
            {
                "" when lang == "en" => key,
                null when lang == "en" => key,
                "" => GetLocalization(key, "en"),
                null => GetLocalization(key, "en"),
                _ => token
            };
            // ReSharper restore HeuristicUnreachableCode
        }

        /**
        * Function for localizing strings
        */
#nullable enable
        public string _(string key, string lang, string? emoji = null, params object?[] formatting)
#nullable disable
        {
            var trueInput = Instance.GetLocalization(key, lang);
            if (emoji != null)
            {
                trueInput = trueInput.Replace("{E}", emoji);
            }

            return string.Format(trueInput, formatting);
        }
    }
}
