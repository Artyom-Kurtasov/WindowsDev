using WindowsDev.ViewModels.Authorization.Dialogs;

namespace WindowsDev.Factories;

public interface IRecoveryStepsFactory
{
    IReadOnlyList<object> CreateSteps(PasswordRecoveryData recoveryData);
}