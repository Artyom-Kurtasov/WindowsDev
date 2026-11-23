using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Factories;
using WindowsDev.ViewModels.Authorization.Dialogs;
using WindowsDev.ViewModels.Authorization.Dialogs.RecoverySteps;

namespace WindowsDev.ViewModels.Auth.Dialogs.Factories;

internal class RecoveryStepsFactory : IRecoveryStepsFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RecoveryStepsFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IReadOnlyList<object> CreateSteps(PasswordRecoveryData recoveryData)
    {
        return
        [
            ActivatorUtilities.CreateInstance<FirstStepViewModel>(_serviceProvider, recoveryData),
            ActivatorUtilities.CreateInstance<SecondStepViewModel>(_serviceProvider, recoveryData),
            ActivatorUtilities.CreateInstance<ThirdStepViewModel>(_serviceProvider, recoveryData),
        ];
    }
}