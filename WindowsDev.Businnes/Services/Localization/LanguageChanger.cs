using System.Windows;

namespace WindowsDev.Business.Services.Localization
{
    public class LanguageChanger
    {
        public void ChangeLanguage(string languageCode)
        {
            var languageDictionary = new ResourceDictionary
            {
                Source = new Uri($"/Localization/Lang.{languageCode}.xaml", UriKind.Relative)
            };

            var oldDictionaryies = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(x => x.Source != null && x.Source.OriginalString.Contains("Lang"));

            if (oldDictionaryies != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldDictionaryies);
            }

            Application.Current.Resources.MergedDictionaries.Add(languageDictionary);
        }
    }
}

