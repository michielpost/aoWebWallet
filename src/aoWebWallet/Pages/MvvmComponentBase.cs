using Microsoft.AspNetCore.Components;
using System.Collections.Specialized;
using System.ComponentModel;
using webvNext.DataLoader;

namespace aoWebWallet.Pages
{
    public abstract class MvvmComponentBase<T> : ComponentBase, IDisposable where T : class, System.ComponentModel.INotifyPropertyChanged
    {
        [Inject]
        public T BindingContext { get; set; } = default!;

        public List<INotifyPropertyChanged> ObjWatch { get; set; } = new();
        public List<INotifyCollectionChanged> CollectionWatch { get; set; } = new();

        protected  override void OnInitialized()
        {
            foreach(var obj in ObjWatch)
            {
                obj.PropertyChanged += ObjWatch_PropertyChanged;
            }

            foreach (var obj in CollectionWatch)
            {
                obj.CollectionChanged += Obj_CollectionChanged;
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

        internal void ObjWatch_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.StateHasChanged();
            //Console.WriteLine("Obj State changed: " + sender?.ToString());
        }
        private void Obj_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            this.StateHasChanged();
            //Console.WriteLine("Obj Collection changed: " + sender?.ToString());

        }

        protected virtual Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }

        protected void WatchObject<D>(D obj) where D : INotifyPropertyChanged
        {
            ObjWatch.Add(obj);
        }

        protected void WatchCollection<D>(D obj) where D : INotifyCollectionChanged
        {
            CollectionWatch.Add(obj);
        }

        protected void WatchDataLoaderVM<D>(DataLoaderViewModel<D> vm) where D : class
        {
            ObjWatch.Add(vm);
            ObjWatch.Add(vm.DataLoader);
        }

        public virtual void Dispose()
        {
            foreach (var obj in ObjWatch)
            {
                obj.PropertyChanged -= ObjWatch_PropertyChanged;
            }

            foreach (var obj in CollectionWatch)
            {
                obj.CollectionChanged -= Obj_CollectionChanged;
            }
        }
    }
}
