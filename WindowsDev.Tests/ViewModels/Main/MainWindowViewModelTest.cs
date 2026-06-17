using Moq;
using WindowsDev.Factories.Interfaces;
using WindowsDev.NavigationManager;
using WindowsDev.ViewModels;
using WindowsDev.ViewModels.Main;

namespace WindowsDev.Tests.ViewModels.Main
{
    public class MainWindowViewModelTest
    {
        private readonly Mock<IViewModelFactory> _factoryMock;
        private readonly NavigationStore _navigationStore;

        public MainWindowViewModelTest()
        {
            _factoryMock = new Mock<IViewModelFactory>();
            _navigationStore = new NavigationStore();
        }

        private MainWindowViewModel CreateVm()
        {
            return new MainWindowViewModel(_navigationStore, _factoryMock.Object);
        }

        [Fact]
        public void CurrentViewModel_ReturnsStoreValue()
        {
            var vm = CreateVm();
            var expected = new ViewModelBase();

            _navigationStore.CurrentViewModel = expected;

            Assert.Equal(expected, vm.CurrentViewModel);
        }

        [Fact]
        public void NavigationStoreChange_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CurrentViewModel))
                    raised = true;
            };

            _navigationStore.CurrentViewModel = new ViewModelBase();

            Assert.True(raised);
        }
    }
}