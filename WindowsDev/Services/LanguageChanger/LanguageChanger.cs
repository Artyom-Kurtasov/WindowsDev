using System.IO;
using System.Windows;
using WindowsDev.Application.Services.Localization;

namespace WindowsDev.Services.LanguageChanger
{
    public class LanguageChanger : ILanguageChanger
    {
        public void ChangeLanguage(string languageCode)
        {
            var languageDictionary = LoadDictionary(languageCode) ?? LoadDictionary("en");

            var oldDictionaryies = App.Current.Resources.MergedDictionaries.FirstOrDefault(
                x => x.Source != null && x.Source.OriginalString.Contains("Lang")
            );

            if (oldDictionaryies != null)
            {
                App.Current.Resources.MergedDictionaries.Remove(oldDictionaryies);
            }

            App.Current.Resources.MergedDictionaries.Add(languageDictionary);
        }

        public string Translate(string key)
        {
            return App.Current?.TryFindResource(key) as string ?? $"[{key}]";
        }

        private ResourceDictionary? LoadDictionary(string languageCode)
        {
            try
            {
                return new ResourceDictionary
                {
                    Source = new Uri(
                        $"/Localization/Language.{languageCode}.xaml",
                        UriKind.Relative
                    ),
                };
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}
