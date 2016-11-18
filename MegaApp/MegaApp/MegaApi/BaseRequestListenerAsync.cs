using System;
using System.Net.Sockets;
using System.Threading.Tasks;using mega;
using MegaApp.Classes;

namespace MegaApp.MegaApi
{
    internal abstract class BaseRequestListenerAsync<T>: MRequestListenerInterface
    {
        protected TaskCompletionSource<T> Tcs;

        public async Task<T> ExecuteAsync(Action action)
        {
            Tcs = new TaskCompletionSource<T>();

            action.Invoke();

            return await Tcs.Task;
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            // Do nothing
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // Do nothing
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Do nothing
        }

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            if (e.getErrorCode() != MErrorType.API_EBLOCKED) return;
            
            // If the account has been blocked, always logout
            api.logout(new LogOutRequestListener(false));
            // Throw task exception to catch and do nothing on logging out
            Tcs?.TrySetException(new BlockedAccountException());
        }
    }
}
