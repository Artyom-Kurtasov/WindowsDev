using WindowsDev.Application.Common.Utils.Localization;

namespace WindowsDev.ViewModels;

internal class LocalizedViewModelBase : ViewModelBase
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