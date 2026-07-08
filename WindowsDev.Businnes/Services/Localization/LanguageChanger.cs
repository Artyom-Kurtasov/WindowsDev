using System.Windows;
using WindowsDev.Business.Services.Localization.Interfaces;

namespace WindowsDev.Business.Services.Localization
{
    public class LanguageChanger : ILanguageChanger
    {
        public void ChangeLanguage(string languageCode)
        {
            var languageDictionary = LoadDictionary(languageCode) ?? LoadDictionary("en");

            var oldDictionaryies = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(x => x.Source != null && x.Source.OriginalString.Contains("Lang"));

            if (oldDictionaryies != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldDictionaryies);
            }

            Application.Current.Resources.MergedDictionaries.Add(languageDictionary);
        }

        public string Translate(string key)
        {
            return Application.Current?.TryFindResource(key) as string ?? $"[{key}]";
        }

        private ResourceDictionary? LoadDictionary(string languageCode)
        {
            try
            {
                return new ResourceDictionary
                {
                    Source = new Uri($"/Localization/Language.{languageCode}.xaml", UriKind.Relative)
                };
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}


