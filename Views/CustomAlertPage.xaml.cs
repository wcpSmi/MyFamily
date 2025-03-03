namespace MyFamily;

public partial class CustomAlertPage : ContentPage
{


	public CustomAlertPage(string title,string message)
	{
        InitializeComponent();
        lblTitle.Text = title;
        lblText.Text = message;

	}

    private async void OnOkButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();  // Modal oldal bezárása
    }
}