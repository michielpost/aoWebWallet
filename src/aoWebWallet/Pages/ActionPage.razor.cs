using aoWebWallet.Models;
using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components.Routing;
using System.Net;

namespace aoWebWallet.Pages
{
    public partial class ActionPage : MvvmComponentBase<MainViewModel>
    {
        public AoAction AoAction { get; set; } = new();

        protected override void OnInitialized()
        {
            GetQueryStringValues();
            WatchDataLoaderVM(BindingContext.TokenList);
            NavigationManager.LocationChanged += NavigationManager_LocationChanged;
        }

        private void NavigationManager_LocationChanged(object? sender, LocationChangedEventArgs e)
        {
            GetQueryStringValues();
            StateHasChanged();
        }

        void GetQueryStringValues()
        {
            var uri = new Uri(NavigationManager.Uri);
            var query = uri.Query;

            // Parsing query string
            var queryStringValues = System.Web.HttpUtility.ParseQueryString(query);

            AoAction = new AoAction();

            foreach (var key in queryStringValues.AllKeys)
            {
                if (key == null)
                    continue;

                var values = queryStringValues.GetValues(key);
                if (values == null || !values.Any())
                    continue;

                foreach(var val in values)
                {
                    string actionKey = key;
                    string? actionValue = val.ToString();
                    ActionParamType actionParamType = ActionParamType.Filled;

                    if (key.Equals("Target", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Target;
                    if (key.Equals("X-Quantity", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Quantity;
                    else if (key.Equals("X-Process", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Process;
                    else if (key.Equals("X-Int", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Integer;
                    else if (key.Equals("X-Input", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Input;

                    if (actionParamType != ActionParamType.Filled
                        && actionParamType != ActionParamType.Target)
                    {
                        actionKey = val;
                        actionValue = null;
                    }

                    AoAction.Params.Add(new ActionParam
                    {
                        Key = actionKey,
                        Value = actionValue,
                        ParamType = actionParamType
                    });

                }
            }

            StateHasChanged();
        }

       

        public void Dispose()
        {
            NavigationManager.LocationChanged -= NavigationManager_LocationChanged;
        }

        protected override async Task LoadDataAsync()
        {
            await BindingContext.LoadTokenList();

            await base.LoadDataAsync();

        }

    }
}
