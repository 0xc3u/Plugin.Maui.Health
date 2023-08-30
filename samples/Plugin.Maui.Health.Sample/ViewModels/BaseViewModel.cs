using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Health.Sample.Interfaces;
using Plugin.Maui.Health.Sample.Services;

namespace Plugin.Maui.Health.Sample.ViewModels;

public partial class BaseViewModel : ObservableObject, IViewModel
{
	public IHealth Health { get; }
	public INavigationService Navigationservice { get; }

	[ObservableProperty]
	bool isBusy;

	public BaseViewModel(IHealth health, INavigationService navigationService)
	{
		Health = health;
		Navigationservice = navigationService;
	}

	public virtual void OnAppearing(object param) { }

	public virtual Task RefreshAsync()
	{
		return Task.CompletedTask;
	}
}
