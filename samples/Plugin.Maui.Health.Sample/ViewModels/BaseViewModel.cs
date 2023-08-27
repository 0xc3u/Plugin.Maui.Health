﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Plugin.Maui.Health.Sample.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
	{
		bool isBusy;
		public bool IsBusy
		{
			get => isBusy;
			set => SetProperty(ref isBusy, value);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void RaisePropertyChanged(string propertyName)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		/// <summary>
		/// Set a property and raise a property changed event if it has changed
		/// </summary>
		protected bool SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(property, value))
			{
				return false;
			}

			property = value;
			RaisePropertyChanged(propertyName);
			return true;
		}

		public abstract void OnAppearing(object param);
	}
