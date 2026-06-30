
using Plugin.Maui.Health.Sample.ViewModels;

namespace Plugin.Maui.Health.Sample.Views;

public partial class BodyMeasurementsView : ContentPage
{


	public BodyMeasurementsView(BodyMeasurementsViewViewModel bodyMeasurementsViewViewModel)
	{
		InitializeComponent();
		BindingContext = bodyMeasurementsViewViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is BodyMeasurementsViewViewModel vm)
			_ = vm.InitializeAsync();
	}
}