using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webvNext.DataLoader
{
    public partial class DataLoaderViewModel<T> : ObservableObject where T : class
    {
        [ObservableProperty]
        private DataLoader dataLoader = new DataLoader();

        [ObservableProperty]
        private T? data;

        public void ForcePropertyChanged()
        {
            OnPropertyChanged(nameof(Data));
        }
    }
}
