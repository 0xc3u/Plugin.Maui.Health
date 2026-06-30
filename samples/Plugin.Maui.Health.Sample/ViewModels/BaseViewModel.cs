using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Health.Enums;
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

	/// <summary>
	/// Ensures the given permission is granted, requesting it through the platform consent UI if needed.
	/// The plugin handles the platform differences: on Android it launches the Health Connect consent
	/// UI (when not already granted); on iOS HealthKit presents its own sheet.
	/// </summary>
	protected Task<bool> EnsurePermissionAsync(HealthParameter parameter, PermissionType permissionType)
		=> Health.RequestPermissionAsync(parameter, permissionType);
}
