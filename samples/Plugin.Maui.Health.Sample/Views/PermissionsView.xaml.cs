using Plugin.Maui.Health.Sample.ViewModels;

namespace Plugin.Maui.Health.Sample.Views;

public partial class PermissionsView : ContentPage
{
	public PermissionsView(PermissionsViewModel permissionsViewModel)
	{
		InitializeComponent();
		BindingContext = permissionsViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is PermissionsViewModel vm)
			_ = vm.InitializeAsync();
	}
}
