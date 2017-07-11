using System;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    public sealed partial class GeneralView : UserControl
    {
        public EventHandler GoToUpgrade;

        public GeneralView()
        {
            this.InitializeComponent();

            this.ViewModel = new GeneralViewModel();
            this.ViewModel.GoToUpgrade += (sender, args) =>
                GoToUpgrade?.Invoke(this, EventArgs.Empty);

            this.DataContext = this.ViewModel;
        }

        public GeneralViewModel ViewModel { get; }

        public StackPanel ViewArea => this.MainStackPanel;
    }
}
