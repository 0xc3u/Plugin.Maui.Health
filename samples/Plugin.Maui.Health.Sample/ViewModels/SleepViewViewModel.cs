using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using Plugin.Maui.Health.Models;
using Plugin.Maui.Health.Sample.Services;

namespace Plugin.Maui.Health.Sample.ViewModels;

public partial class SleepViewViewModel : BaseViewModel
{
	const string SeededSleepKey = "seeded.sleep";

	// A representative night's hypnogram (stage, minutes), laid out sequentially.
	static readonly (SleepStage Stage, int Minutes)[] DemoNight =
	{
		(SleepStage.InBed, 8), (SleepStage.Light, 42), (SleepStage.Deep, 55), (SleepStage.Light, 28),
		(SleepStage.Rem, 24), (SleepStage.Light, 36), (SleepStage.Deep, 40), (SleepStage.Awake, 6),
		(SleepStage.Light, 30), (SleepStage.Rem, 32), (SleepStage.Light, 24), (SleepStage.Rem, 34),
		(SleepStage.Light, 18), (SleepStage.Awake, 5), (SleepStage.Light, 22), (SleepStage.Deep, 18),
		(SleepStage.Rem, 26), (SleepStage.Light, 12),
	};

	bool initialized;

	[ObservableProperty]
	ObservableCollection<SleepStageSample> stages = new();

	[ObservableProperty]
	string sleepDateText = "Last night";

	[ObservableProperty]
	string asleepText = "—";

	[ObservableProperty]
	string inBedText = "—";

	[ObservableProperty]
	double deepMinutes;

	[ObservableProperty]
	double remMinutes;

	[ObservableProperty]
	double lightMinutes;

	[ObservableProperty]
	double awakeMinutes;

	public SleepViewViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{
		ShowDemoData();
	}

	public async Task InitializeAsync()
	{
		if (initialized)
			return;
		initialized = true;

		if (!Health.IsSupported)
			return; // keep demo data

		try
		{
			IsBusy = true;

			await Health.RequestSleepPermissionAsync(PermissionType.Read | PermissionType.Write);

			var work = SeedAndLoadAsync();
			var finished = await Task.WhenAny(work, Task.Delay(TimeSpan.FromSeconds(15)));
			if (finished == work)
				await work;
		}
		catch (HealthException)
		{
			// Leave demo data in place.
		}
		finally
		{
			IsBusy = false;
		}
	}

	async Task SeedAndLoadAsync()
	{
		await SeedSleepOnceAsync();
		await LoadSleepAsync();
	}

	async Task SeedSleepOnceAsync()
	{
		var key = $"{SeededSleepKey}.{DateTime.Now:yyyyMMdd}";
		if (Preferences.Get(key, false))
			return;

		await Health.WriteSleepAsync(BuildNight());
		Preferences.Set(key, true);
	}

	async Task LoadSleepAsync()
	{
		var sessions = await Health.ReadSleepAsync(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now);
		if (sessions.Count == 0)
			return;

		var latest = sessions.OrderByDescending(s => s.From).First();
		Apply(latest.Stages.OrderBy(s => s.From).ToList(), latest.From, latest.Until);
	}

	void ShowDemoData()
	{
		var night = BuildNight();
		Apply(night.Stages.ToList(), night.From, night.Until);
	}

	void Apply(List<SleepStageSample> stageList, DateTimeOffset from, DateTimeOffset until)
	{
		Stages = new ObservableCollection<SleepStageSample>(stageList);

		double Minutes(SleepStage stage) => stageList
			.Where(s => s.Stage == stage)
			.Sum(s => (s.Until - s.From).TotalMinutes);

		DeepMinutes = Math.Round(Minutes(SleepStage.Deep));
		RemMinutes = Math.Round(Minutes(SleepStage.Rem));
		LightMinutes = Math.Round(Minutes(SleepStage.Light) + Minutes(SleepStage.Sleeping));
		AwakeMinutes = Math.Round(Minutes(SleepStage.Awake) + Minutes(SleepStage.InBed));

		var asleep = TimeSpan.FromMinutes(DeepMinutes + RemMinutes + LightMinutes);
		var inBed = until - from;
		AsleepText = $"{(int)asleep.TotalHours}h {asleep.Minutes:00}m";
		InBedText = $"{(int)inBed.TotalHours}h {inBed.Minutes:00}m in bed";
		SleepDateText = from.ToLocalTime().ToString("ddd, MMM d");
	}

	static SleepSession BuildNight()
	{
		// Anchor the night so it ends this morning (~07:00) for a realistic "last night".
		var wake = new DateTimeOffset(DateTime.Now.Date.AddHours(7));
		var totalMinutes = DemoNight.Sum(x => x.Minutes);
		var cursor = wake.AddMinutes(-totalMinutes);

		var stages = new List<SleepStageSample>();
		foreach (var (stage, minutes) in DemoNight)
		{
			var end = cursor.AddMinutes(minutes);
			stages.Add(new SleepStageSample { From = cursor, Until = end, Stage = stage });
			cursor = end;
		}

		var from = stages[0].From;
		return new SleepSession
		{
			From = from,
			Until = wake,
			DurationInSeconds = (wake - from).TotalSeconds,
			Stages = stages,
		};
	}
}
