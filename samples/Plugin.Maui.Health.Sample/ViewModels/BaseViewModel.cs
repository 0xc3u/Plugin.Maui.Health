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
	/// Checks whether the given permission is granted. On Android, if not granted,
	/// launches the Health Connect consent UI and re-checks after the user returns.
	/// On iOS, HealthKit manages its own consent sheet on first access.
	/// </summary>
	protected async Task<bool> EnsurePermissionAsync(HealthParameter parameter, PermissionType permissionType)
	{
		var hasPermission = await Health.CheckPermissionAsync(parameter, permissionType);
		if (!hasPermission)
		{
#if ANDROID
			if (MainActivity.Current is { } activity)
			{
				await activity.RequestHealthPermissionsAsync(parameter, permissionType);
				hasPermission = await Health.CheckPermissionAsync(parameter, permissionType);
			}
#endif
		}
		return hasPermission;
	}
}
