using System;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public abstract class BaseNavigableViewModel: BaseUiViewModel
    {
        protected BaseNavigableViewModel()
        {
            this.Navigation = NavigateService.Instance;
        }

        public void Navigate(BasePageViewModel viewModel)
        {
            if(viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            var pageType = NavigateService.GetViewType(viewModel);
            if (pageType == null) return;

            this.OnUiThread(() => this.Navigation.Navigate(pageType));
        }

        /// <summary>
        /// Navigation interface to navigate views
        /// </summary>
        private INavigate _navigation;
        public INavigate Navigation
        {
            get { return _navigation; }
            set { SetField(ref _navigation, value); }
        }
    }
}
