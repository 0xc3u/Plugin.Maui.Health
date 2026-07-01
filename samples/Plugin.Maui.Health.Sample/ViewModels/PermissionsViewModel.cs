using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using Plugin.Maui.Health.Sample.Services;

namespace Plugin.Maui.Health.Sample.ViewModels;

/// <summary>
/// Demonstrates the batched permission APIs: a single <see cref="IHealth.RequestPermissionsAsync"/> call
/// grants every permission the sample uses (one HealthKit sheet / one Health Connect launch), and
/// <see cref="IHealth.CheckPermissionsAsync"/> reports the current grant state without a string of prompts.
/// This is the "explain once, ask once" onboarding flow.
/// </summary>
public partial class PermissionsViewModel : BaseViewModel
{
	const PermissionType RW = PermissionType.Read | PermissionType.Write;

	// Every HealthParameter the sample screens read/write, grouped for a readable status list.
	static readonly (string Name, (HealthParameter, PermissionType)[] Params)[] Groups =
	{
		("Steps", new[] { (HealthParameter.StepCount, RW) }),
		("Heart rate", new[] { (HealthParameter.HeartRate, RW) }),
		("Body measurements", new[]
		{
			(HealthParameter.BodyMass, RW),
			(HealthParameter.Height, RW),
			(HealthParameter.BodyFatPercentage, RW),
		}),
		("Vitamins", new[]
		{
			(HealthParameter.DietaryVitaminC, RW),
			(HealthParameter.DietaryVitaminD, RW),
			(HealthParameter.DietaryVitaminE, RW),
			(HealthParameter.DietaryVitaminB6, RW),
			(HealthParameter.DietaryVitaminB12, RW),
			(HealthParameter.DietaryVitaminK, RW),
		}),
	};

	static (HealthParameter, PermissionType)[] AllParams
		=> Groups.SelectMany(g => g.Params).ToArray();

	bool initialized;

	[ObservableProperty]
	ObservableCollection<PermissionRow> rows = new();

	[ObservableProperty]
	string statusMessage = "Tap \"Grant all\" to request every permission the sample uses in one prompt.";

	public PermissionsViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{
		// Build the rows up-front so the list renders instantly (aggregate first, then per-group).
		Rows.Add(new PermissionRow("All sample data"));
		foreach (var group in Groups)
			Rows.Add(new PermissionRow(group.Name));
		Rows.Add(new PermissionRow("Workouts"));
	}

	public async Task InitializeAsync()
	{
		if (initialized)
			return;
		initialized = true;

		await RefreshStatusAsync();
	}

	/// <summary>Grants every permission the sample uses with a single batched request per API.</summary>
	[RelayCommand]
	async Task GrantAllAsync()
	{
		if (IsBusy || !Health.IsSupported)
			return;

		try
		{
			IsBusy = true;

			// One consent prompt for all quantity parameters …
			await Health.RequestPermissionsAsync(AllParams);
			// … plus the dedicated workout and sleep consent flows.
			await Health.RequestWorkoutPermissionAsync(RW);
			await Health.RequestSleepPermissionAsync(RW);

			await RefreshStatusAsync();
		}
		catch (HealthException ex)
		{
			StatusMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>Re-reads the current grant state using the batched check API (never prompts on Android).</summary>
	[RelayCommand]
	Task RefreshAsync() => RefreshStatusAsync();

	async Task RefreshStatusAsync()
	{
		if (!Health.IsSupported)
		{
			StatusMessage = "Health data is not available on this device.";
			return;
		}

		try
		{
			IsBusy = true;

			// Aggregate: a single CheckPermissionsAsync call for the whole sample surface.
			Rows[0].Granted = await Health.CheckPermissionsAsync(AllParams);

			// Per-group status, each a batched check of that group's parameters.
			for (int i = 0; i < Groups.Length; i++)
				Rows[i + 1].Granted = await Health.CheckPermissionsAsync(Groups[i].Params);

			// Workouts use their own permission pair.
			Rows[^1].Granted = await Health.CheckWorkoutPermissionAsync(RW);

			var grantedCount = Rows.Count(r => r.Granted);
			StatusMessage = grantedCount == Rows.Count
				? "All permissions granted."
				: $"{grantedCount} of {Rows.Count} groups granted.";
		}
		catch (HealthException ex)
		{
			StatusMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
		}
	}
}

/// <summary>A single row in the permission status list.</summary>
public partial class PermissionRow : ObservableObject
{
	public string Name { get; }

	public PermissionRow(string name) => Name = name;

	[ObservableProperty]
	bool granted;

	public string StatusText => Granted ? "Granted" : "Not granted";

	partial void OnGrantedChanged(bool value) => OnPropertyChanged(nameof(StatusText));
}
