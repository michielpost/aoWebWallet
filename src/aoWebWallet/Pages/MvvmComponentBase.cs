using aoWebWallet.ViewModels;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Diagnostics;
using webvNext.DataLoader;

namespace aoWebWallet.Pages
{
    public abstract class MvvmComponentBase<T> : ComponentBase, IDisposable where T : class, System.ComponentModel.INotifyPropertyChanged
    {
        [Inject]
        public T BindingContext { get; set; } = default!;

        public List<INotifyPropertyChanged> ObjWatch { get; set; } = new();

        protected  override void OnInitialized()
        {
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;

            foreach(var obj in ObjWatch)
            {
                obj.PropertyChanged += ObjWatch_PropertyChanged;
            }

            base.OnInitialized();
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            await base.OnInitializedAsync();
        }

        //protected override async Task OnParametersSetAsync()
        //{
        //    await LoadDataAsync();
        //    await base.OnParametersSetAsync();
        //}

        internal async void BindingContext_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.ComputeUnitUrl))
            {
                await LoadDataAsync();
                this.StateHasChanged();
            }
        }

        internal async void ObjWatch_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.StateHasChanged();
            await ChartRenderAsync();
        }

        protected virtual Task ChartRenderAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }

        protected void WatchDataLoaderVM<D>(DataLoaderViewModel<D> vm) where D : class
        {
            ObjWatch.Add(vm);
            ObjWatch.Add(vm.DataLoader);
        }

        public virtual void Dispose()
        {
            BindingContext.PropertyChanged -= BindingContext_PropertyChanged;

            foreach (var obj in ObjWatch)
            {
                obj.PropertyChanged -= ObjWatch_PropertyChanged;
            }
        }
    }
}
