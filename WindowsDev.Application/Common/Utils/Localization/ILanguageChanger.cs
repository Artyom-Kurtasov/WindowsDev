namespace WindowsDev.Application.Common.Utils.Localization;

public interface ILanguageChanger
{
    void ChangeLanguage(string languageCode);
    string Translate(string key);
}