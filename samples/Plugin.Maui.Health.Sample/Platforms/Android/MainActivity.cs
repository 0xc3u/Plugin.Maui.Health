using Android.App;
using Android.Content.PM;

namespace Plugin.Maui.Health.Sample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	// The Health Connect consent flow is driven by the plugin via
	// IHealth.RequestPermissionAsync — no per-app launcher wiring is required here.
}
