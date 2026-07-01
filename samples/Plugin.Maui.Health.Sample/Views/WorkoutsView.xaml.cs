using Plugin.Maui.Health.Sample.ViewModels;

namespace Plugin.Maui.Health.Sample.Views;

public partial class WorkoutsView : ContentPage
{
	public WorkoutsView(WorkoutsViewViewModel workoutsViewViewModel)
	{
		InitializeComponent();
		BindingContext = workoutsViewViewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is WorkoutsViewViewModel vm)
			_ = vm.InitializeAsync();
	}
}
