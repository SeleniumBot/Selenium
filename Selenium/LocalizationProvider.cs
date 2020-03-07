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

        public readonly List<string> LanguageCodes = new List<string>();

        private readonly Dictionary<string, JObject> _languages
            = new Dictionary<string, JObject>();

        private LocalizationProvider()
        {
            var langDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang");

            var languages = Directory.GetFiles(langDir);
            foreach (var language in languages)
            {
                var languageCode = Path.GetFileNameWithoutExtension(language);

                LanguageCodes.Add(languageCode);
            }

            LoadLanguage("en");
        }

        public bool IsLanguageValid(string langCode)
            => LanguageCodes.Contains(langCode);

        // dynamically load languages that are actually used to save memory
        public void LoadLanguage(string code)
        {
            if (!IsLanguageValid(code))
            {
                Bot.Logger.Warning("Attempted to load non-existent language {Code}", code);
                return;
            }

            var lang = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang", code + ".json");

            Bot.Logger.Debug("Loading language {LangCode}", code);

            var jsonData = File.ReadAllText(lang);

            JObject obj;

            try
            {
                obj = JObject.Parse(jsonData);
            }
            catch (JsonReaderException e)
            {
                Bot.Logger.Warning(e, "Failed to load language {Lang}", lang);
                return;
            }

            _languages[code] = obj;
        }

        public string GetLocalization(string key, string lang)
        {
            if (!_languages.ContainsKey(lang))
            {
                if (IsLanguageValid(lang))
                    LoadLanguage(lang);
                else
                    lang = "en"; // fall back to english
            }

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
            try
            {
                var trueInput = Instance.GetLocalization(key, lang);
                if (emoji != null)
                {
                    trueInput = trueInput.Replace("{E}", emoji);
                }

                return string.Format(trueInput, formatting);
            }
            catch (Exception e)
            {
                return $"{Constants.ErrorEmoji} Failed to localize `{key}` in language `{lang}`: ```{e}```";
            }
        }
    }
}
