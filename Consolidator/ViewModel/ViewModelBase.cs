using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Consolidator.ViewModel
{
    public abstract  class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase()
        {
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String inPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(inPropertyName));
            }
            
        }
    }
}
