using Plugin.Maui.Health.Sample.ViewModels;

namespace Plugin.Maui.Health.Sample;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageViewModel mainPageViewModel)
	{
		InitializeComponent();
		BindingContext = mainPageViewModel;
	}
}
