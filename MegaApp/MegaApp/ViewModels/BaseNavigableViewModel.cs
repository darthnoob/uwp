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

        public async void Navigate(BasePageViewModel viewModel)
        {
            if(viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            var pageType = NavigateService.GetViewType(viewModel);
            if (pageType == null) throw new ArgumentException("Viewmodel is not bound to a view");

            await this.OnUiThread(() => this.Navigation.Navigate(pageType));
        }

        /// <summary>
        /// Navigation interface to navigate views
        /// </summary>
        public INavigate Navigation { get; }
       
    }
}
