using Plugin.Maui.Health.Sample.ViewModels;

namespace Plugin.Maui.Health.Sample.Views;

public partial class SleepView : ContentPage
{
	public SleepView(SleepViewViewModel sleepViewViewModel)
	{
		InitializeComponent();
		BindingContext = sleepViewViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is SleepViewViewModel vm)
			_ = vm.InitializeAsync();
	}
}
