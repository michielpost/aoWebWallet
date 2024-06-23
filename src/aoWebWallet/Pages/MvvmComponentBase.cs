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

        public List<string> PropWatch { get; set; } = new();
        public List<INotifyPropertyChanged> ObjWatch { get; set; } = new();
        public List<INotifyCollectionChanged> CollectionWatch { get; set; } = new();

        protected  override void OnInitialized()
        {
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;

            foreach (var obj in ObjWatch)
            {
                obj.PropertyChanged += ObjWatch_PropertyChanged;
            }

            foreach (var obj in CollectionWatch)
            {
                obj.CollectionChanged += Obj_CollectionChanged;
            }

            base.OnInitialized();
        }

        private void BindingContext_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null)
                return;

            if (PropWatch.Contains(e.PropertyName))
            {
                Console.WriteLine("Changed! " + e.PropertyName);
                this.StateHasChanged();
            }
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

        protected void WatchProp(string propName)
        {
            PropWatch.Add(propName);
        }

        public virtual void Dispose()
        {
            BindingContext.PropertyChanged -= BindingContext_PropertyChanged;

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
