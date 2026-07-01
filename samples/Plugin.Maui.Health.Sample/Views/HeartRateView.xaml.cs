using Plugin.Maui.Health.Sample.ViewModels;

namespace Plugin.Maui.Health.Sample.Views;

public partial class HeartRateView : ContentPage
{
	public HeartRateView(HeartRateViewViewModel heartRateViewViewModel)
	{
		InitializeComponent();
		BindingContext = heartRateViewViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is HeartRateViewViewModel vm)
			_ = vm.InitializeAsync();
	}
}
