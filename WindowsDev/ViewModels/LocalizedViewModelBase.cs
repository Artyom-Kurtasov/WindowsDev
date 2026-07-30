using WindowsDev.Application.Services.Localization;

namespace WindowsDev.ViewModels
{
    public class LocalizedViewModelBase : ViewModelBase
    {
        protected readonly ILanguageChanger LanguageChanger;

        protected LocalizedViewModelBase(ILanguageChanger languageChanger)
        {
            LanguageChanger = languageChanger;
        }

        protected string Translate(string key)
        {
            return LanguageChanger.Translate(key);
        }
    }
}
