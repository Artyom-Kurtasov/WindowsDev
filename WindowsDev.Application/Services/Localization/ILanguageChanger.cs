namespace WindowsDev.Application.Services.Localization
{
    public interface ILanguageChanger
    {
        void ChangeLanguage(string languageCode);
        string Translate(string key);
    }
}
