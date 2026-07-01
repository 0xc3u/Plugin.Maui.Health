using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using Plugin.Maui.Health.Sample.ViewModels;

namespace Plugin.Maui.Health.Sample;

public partial class MainPage : ContentPage
{
	// The dashboard hides the Shell nav bar, so it manages the status bar itself: a light bar with dark
	// icons to match the page. Detail pages keep the purple Shell chrome, so we restore that on leaving.
	static readonly Color DashboardStatusBar = Color.FromArgb("#F4F6FB"); // PageBackground
	static readonly Color ShellStatusBar = Color.FromArgb("#512BD4");     // Primary

	MainPageViewModel mainPageViewModel;

	public MainPage(MainPageViewModel mainPageViewModel)
	{
		InitializeComponent();
		this.mainPageViewModel = mainPageViewModel;
		BindingContext = mainPageViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		SetStatusBar(DashboardStatusBar, StatusBarStyle.DarkContent);
		mainPageViewModel.OnAppearing(null);
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		// Navigating to a detail page: hand the purple status bar back to the Shell nav bar.
		SetStatusBar(ShellStatusBar, StatusBarStyle.LightContent);
	}

	static void SetStatusBar(Color color, StatusBarStyle style)
	{
		try
		{
			StatusBar.SetColor(color);
			StatusBar.SetStyle(style);
		}
		catch
		{
			// Status-bar theming is best-effort; ignore platforms/states that don't support it.
		}
	}
}
