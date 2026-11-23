using System.IO;
using System.Windows;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Services.LanguageChanger;

internal class LanguageChanger : ILanguageChanger
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
        if (!string.IsNullOrWhiteSpace(key))
        {
            var translation = App.Current?.TryFindResource(key) as string;
            if (!string.IsNullOrWhiteSpace(translation))
                return translation;
        }

        return App.Current?.TryFindResource(CommonErrors.UnexpectedError) as string
            ?? "An unexpected error occurred.";
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