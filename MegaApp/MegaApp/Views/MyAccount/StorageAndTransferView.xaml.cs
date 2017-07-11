using System;
using Windows.UI.Xaml.Controls;
using MegaApp.ViewModels.MyAccount;

namespace MegaApp.Views.MyAccount
{
    public sealed partial class StorageAndTransferView : UserControl
    {
        public EventHandler GoToUpgrade;

        public StorageAndTransferView()
        {
            this.InitializeComponent();

            this.ViewModel = new StorageAndTransferViewModel();
            this.ViewModel.GoToUpgrade += (sender, args) =>
                GoToUpgrade?.Invoke(this, EventArgs.Empty);

            this.DataContext = this.ViewModel;
        }

        public StorageAndTransferViewModel ViewModel { get; }

        public StackPanel ViewArea => this.MainStackPanel;
    }
}
