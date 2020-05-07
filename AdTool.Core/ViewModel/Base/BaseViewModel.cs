using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using AdTool.Core;

namespace AdTool.Core
{
    
    public class BaseViewModel : INotifyPropertyChanged
    {
       
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region same expression
        // same expression
        //public event PropertyChangedEventHandler PropertyChanged = (sender, e) => {};
        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChanged(this, new PropertyChangedEventArgs(name));
        //}
        //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-
        #endregion

        public async Task RunCommandAsync(Expression<Func<bool>> updatingFlag, Func<Task> action)
        {
            if (updatingFlag.GetPropertyValue())
                return;

            updatingFlag.SetPropertyValue(true);

            try
            {
                await action();
            }
            finally
            {
                updatingFlag.SetPropertyValue(false);
            }
        }



    }
}
