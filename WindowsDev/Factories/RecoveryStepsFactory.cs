using WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps;

namespace WindowsDev.ViewModels.Auth.Dialogs.Factories
{
    public class RecoveryStepsFactory : IRecoveryStepsFactory
    {
        private readonly FirstStepViewModel _firstStepViewModel;
        private readonly SecondStepViewModel _secondStepViewModel;
        private readonly ThirdStepViewModel _thirdStepViewModel;

        public RecoveryStepsFactory(FirstStepViewModel firstStepViewModel,
            SecondStepViewModel secondStepViewModel,
            ThirdStepViewModel thirdStepViewModel)
        {
            _firstStepViewModel = firstStepViewModel;
            _secondStepViewModel = secondStepViewModel;
            _thirdStepViewModel = thirdStepViewModel;
        }

        public IReadOnlyList<object> CreateSteps()
        {
            return
            [
                _firstStepViewModel,
                _secondStepViewModel,
                _thirdStepViewModel
            ];
        }
    }
}
