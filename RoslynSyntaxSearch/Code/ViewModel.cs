using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RoslynSyntaxSearch.Code
{
	public abstract class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void SetPropertyField<T>(ref T previousValue, T newValue, [CallerMemberName] string callerMemberName = null)
		{
			if (!object.Equals(previousValue, newValue))
			{
				previousValue = newValue;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerMemberName));
			}
		}
	}
}
