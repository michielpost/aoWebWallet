using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace webvNext.DataLoader
{
    /// <summary>
    /// Possible loading states for the DataLoader
    /// </summary>
    public enum LoadingState
    {
        /// <summary>None</summary>
        None,
        /// <summary>Loading</summary>
        Loading,
        /// <summary>Finished</summary>
        Finished,
        /// <summary>Error</summary>
        Error
    }

    /// <summary>
    /// Helper model to easily show loaders when loading data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class DataLoader : ObservableObject
    {
        // <summary>
        /// DataLoader constructors
        /// </summary>
        /// <param name="swallowExceptions">Swallows exceptions. Defaults to true. It's a more common scenario to swallow exceptions and just bind to the IsError property. You don't want to surround each DataLoader with a try/catch block. You can listen to the error callback at all times to get the error.</param>
        public DataLoader(bool swallowExceptions = true)
        {
            _swallowExceptions = swallowExceptions;
        }


        private bool _swallowExceptions;

        [ObservableProperty]
        private LoadingState loadingState;

        [ObservableProperty]
        private string? progressMsg;

        public DateTimeOffset? LoadedDateTime { get; set; }

        /// <summary>
        ///  Load data. Errors will be in errorcallback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loadingMethod"></param>
        /// <param name="resultCallback"></param>
        /// <param name="errorCallback">optional error callback. Fires when exceptino is thrown in loadingMethod</param>
        /// <returns></returns>
        public async Task<T?> LoadAsync<T>(Func<Task<T?>> loadingMethod, Action<T?>? resultCallback = null, Action<Exception>? errorCallback = null) where T : class
        {
            //await semaphoreSlim.WaitAsync();
            //try
            //{
            //Set loading state
            LoadingState = LoadingState.Loading;

            T? result = default;

            try
            {
                result = await loadingMethod();

                //Set finished state
                LoadingState = LoadingState.Finished;
                LoadedDateTime = DateTimeOffset.UtcNow;

                if (resultCallback != null)
                {
                    resultCallback(result);
                }

            }
            catch (Exception e)
            {
                //Set error state
                LoadingState = LoadingState.Error;

                if (errorCallback != null)
                    errorCallback(e);
                else if (!_swallowExceptions) //swallow exception if _swallowExceptions is true
                    throw; //throw error if no callback is defined

            }

            return result;
            //}
            //finally
            //{
            //    semaphoreSlim.Release();
            //}
        }

        /// <summary>
        /// First returns result callback with result from cache, then from refresh method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheLoadingMethod"></param>
        /// <param name="refreshLoadingMethod"></param>
        /// <param name="resultCallback"></param>
        /// <param name="errorCallback"></param>
        /// <returns></returns>
        public async Task LoadCacheThenRefreshAsync<T>(Func<Task<T>> cacheLoadingMethod, Func<Task<T>> refreshLoadingMethod, Action<T>? resultCallback = null, Action<Exception>? errorCallback = null) where T : class, new()
        {
            //Set loading state
            LoadingState = LoadingState.Loading;

            T? cacheResult = default;
            T? refreshResult = default;

            try
            {
                cacheResult = await cacheLoadingMethod();

                if (resultCallback != null)
                    resultCallback(cacheResult);

                refreshResult = await refreshLoadingMethod();

                if (resultCallback != null)
                    resultCallback(refreshResult);

                //Set finished state
                LoadingState = LoadingState.Finished;

            }
            catch (Exception e)
            {
                //Set error state
                LoadingState = LoadingState.Error;

                if (errorCallback != null)
                    errorCallback(e);
                else if (!_swallowExceptions) //swallow exception if catchexception is true
                    throw; //throw error if no callback is defined

            }

        }

        /// <summary>
        /// Loads data from source A, if this fails, load it from source B (cache)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="refreshLoadingMethod"></param>
        /// <param name="cacheLoadingMethod"></param>
        /// <param name="resultCallback"></param>
        /// <param name="errorCallback"></param>
        /// <returns></returns>
        public async Task LoadFallbackToCacheAsync<T>(Func<Task<T>> refreshLoadingMethod, Func<Task<T>> cacheLoadingMethod, Action<T>? resultCallback = null, Action<Exception>? errorCallback = null) where T : class, new()
        {
            //Set loading state
            LoadingState = LoadingState.Loading;

            T? refreshResult = default;
            T? cacheResult = default;

            bool refreshSourceFail = false;

            try
            {
                refreshResult = await refreshLoadingMethod();
                if (resultCallback != null)
                    resultCallback(refreshResult);

                //Set finished state
                LoadingState = LoadingState.Finished;
            }
            catch (Exception e)
            {
                refreshSourceFail = true;

                if (errorCallback != null)
                    errorCallback(e);
            }

            //Did the loading fail? Load data from source B (cache)
            if (refreshSourceFail)
            {
                try
                {
                    cacheResult = await cacheLoadingMethod();
                    if (resultCallback != null)
                        resultCallback(cacheResult);

                    //Set finished state
                    LoadingState = LoadingState.Finished;
                }
                catch (Exception e)
                {
                    //Set error state
                    LoadingState = LoadingState.Error;

                    if (errorCallback != null)
                        errorCallback(e);
                }
            }
        }
    }
}
