using CommunityToolkit.Maui;
using Plugin.Maui.Health.Sample.Services;
using Plugin.Maui.Health.Sample.ViewModels;
using Plugin.Maui.Health.Sample.Views;

namespace Plugin.Maui.Health.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.UseMauiCommunityToolkit();

		builder.Services.AddSingleton<INavigationService, NavigationService>();
		builder.Services.AddSingleton(HealthDataProvider.Default);
        builder.Services.AddSingletonWithShellRoute<MainPage, MainPageViewModel>(nameof(MainPage));
		builder.Services.AddSingletonWithShellRoute<VitaminsView, VitaminsViewViewModel>(nameof(VitaminsView));
		builder.Services.AddSingletonWithShellRoute<BodyMeasurementsView, BodyMeasurementsViewViewModel>(nameof(BodyMeasurementsView));
		builder.Services.AddSingletonWithShellRoute<HeartRateView, HeartRateViewViewModel>(nameof(HeartRateView));
		builder.Services.AddSingletonWithShellRoute<WorkoutsView, WorkoutsViewViewModel>(nameof(WorkoutsView));
		builder.Services.AddSingletonWithShellRoute<SleepView, SleepViewViewModel>(nameof(SleepView));
		builder.Services.AddSingletonWithShellRoute<PermissionsView, PermissionsViewModel>(nameof(PermissionsView));


		return builder.Build();
    }
}