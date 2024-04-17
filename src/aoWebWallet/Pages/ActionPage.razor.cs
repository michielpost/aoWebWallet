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
            WatchDataLoaderVM(BindingContext.WalletList);
            WatchDataLoaderVM(BindingContext.BalanceDataList);

            NavigationManager.LocationChanged += NavigationManager_LocationChanged;

            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BindingContext.CheckHasArConnectExtension();

                await BindingContext.LoadWalletList();
                await BindingContext.LoadTokenList();
            }

            await base.OnAfterRenderAsync(firstRender);
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

                    var actionValueSplit = actionValue.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    actionValue = actionValueSplit.FirstOrDefault();
                    List<string> args = actionValueSplit.Skip(1).ToList();

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
                        Args = args,
                        ParamType = actionParamType
                    });

                }
            }

            StateHasChanged();
        }

       

        public override void Dispose()
        {
            NavigationManager.LocationChanged -= NavigationManager_LocationChanged;

            base.Dispose();
        }

        //protected override async Task LoadDataAsync()
        //{
        //    await BindingContext.LoadTokenList();

        //    await base.LoadDataAsync();

        //}

    }
}
