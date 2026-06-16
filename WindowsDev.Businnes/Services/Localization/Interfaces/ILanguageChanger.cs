namespace WindowsDev.Business.Services.Localization.Interfaces
{
    public interface ILanguageChanger
    {
        void ChangeLanguage(string languageCode);
        string Translate(string key);
    }
}
