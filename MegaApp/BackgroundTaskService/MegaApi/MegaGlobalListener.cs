using System;
using System.Threading.Tasks;
using mega;

namespace BackgroundTaskService.MegaApi
{
    class MegaGlobalListener: MGlobalListenerInterface
    {
        private TaskCompletionSource<object> _tcs;

        public async Task<object> ExecuteAsync(Action action)
        {
            _tcs = new TaskCompletionSource<object>();

            action.Invoke();

            return await _tcs.Task;
        }

        public void onUsersUpdate(MegaSDK api, MUserList users)
        {
            // Users update
        }

        public void onNodesUpdate(MegaSDK api, MNodeList nodes)
        {
            // If the SDK has resumed the possible pending transfers
            if (nodes != null) return;
            
            // If no pending transfers to resume start a new upload
            // Else it will start when finish the current transfer
            if (api.getTransferData().getNumDownloads() == 0)
            {
                _tcs.TrySetResult(null);
            }
        }

        public void onAccountUpdate(MegaSDK api)
        {
            // Account update
        }

        public void onContactRequestsUpdate(MegaSDK api, MContactRequestList requests)
        {
            // Contact update
        }

        public void onReloadNeeded(MegaSDK api)
        {
            // Reload is needed
        }

        public void onEvent(MegaSDK api, MEvent ev)
        {
            // Event received
        }
    }
}
