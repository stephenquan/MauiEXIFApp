// MainPage.xaml.cs

namespace MauiEXIFApp;

public partial class MainPage : ContentPage
{
	public MainViewModel VM { get; } = new();

	public MainPage()
	{
		VM.ReadMauiVersionFromAssembly(typeof(MainPage).Assembly);
		BindingContext = VM;
		InitializeComponent();
	}
}
