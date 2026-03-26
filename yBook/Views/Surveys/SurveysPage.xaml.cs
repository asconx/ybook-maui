using yBook.Views.Surveys;

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
        if (BindingContext is SurveysViewModel viewModel)
        {
            await viewModel.LoadSurveysCommand.ExecuteAsync(null);
        }
    }
}
