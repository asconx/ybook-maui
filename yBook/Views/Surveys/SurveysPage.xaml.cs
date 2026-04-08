namespace yBook.Views.Surveys;

public partial class SurveysPage : ContentPage
{
    public SurveysPage(SurveysViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SurveysViewModel vm)
            await vm.FetchSurveysFromApiCommand.ExecuteAsync(null);
    }
}
