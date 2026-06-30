using Plugin.Maui.Health.Sample.ViewModels;

namespace Plugin.Maui.Health.Sample.Views;

public partial class VitaminsView : ContentPage
{
	public VitaminsView(VitaminsViewViewModel vitaminsViewViewModel)
	{
		InitializeComponent();
		BindingContext = vitaminsViewViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is VitaminsViewViewModel vm)
			_ = vm.InitializeAsync();
	}
}