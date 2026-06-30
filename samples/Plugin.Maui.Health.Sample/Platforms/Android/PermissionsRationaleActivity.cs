using Android.App;
using Android.OS;
using Android.Widget;

namespace Plugin.Maui.Health.Sample;

/// <summary>
/// Privacy-policy / rationale screen for Health Connect.
/// <para>
/// Health Connect shows a "privacy policy" link on its permission sheet and requires the app to
/// own a screen that explains why it accesses health data. This is wired up two ways:
/// </para>
/// <list type="bullet">
/// <item>Android ≤13 reaches it through the <c>androidx.health.ACTION_SHOW_PERMISSIONS_RATIONALE</c>
/// intent filter declared below.</item>
/// <item>Android 14+ (API 34+) reaches it through the <c>ViewPermissionUsageActivity</c>
/// <c>&lt;activity-alias&gt;</c> in <c>AndroidManifest.xml</c>, which targets this activity.</item>
/// </list>
/// <para>
/// On Android 14+ this declaration is mandatory: without it the system permission screen never
/// presents the request, so the <see cref="MainActivity"/> permission launcher never receives a
/// result and the caller hangs.
/// </para>
/// </summary>
[Activity(
	Name = "com.companyname.pluginsample.PermissionsRationaleActivity",
	Exported = true)]
[IntentFilter(new[] { "androidx.health.ACTION_SHOW_PERMISSIONS_RATIONALE" })]
public class PermissionsRationaleActivity : Activity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		var text = new TextView(this)
		{
			Text =
				"This app reads and writes your health and fitness data — such as steps, heart rate, " +
				"body measurements, nutrition and workouts — so it can display your activity and let " +
				"you record new entries. Your data stays on your device and is never shared.",
		};
		text.SetPadding(48, 48, 48, 48);
		SetContentView(text);
	}
}
