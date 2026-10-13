namespace WindowsDev.Application.Common.Localization;

public interface ILanguageChanger
{
    void ChangeLanguage(string languageCode);
    string Translate(string key);
}