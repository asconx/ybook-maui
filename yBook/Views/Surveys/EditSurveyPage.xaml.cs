using yBook.Views.Surveys;

namespace yBook.Views.Surveys;

public partial class EditSurveyPage : ContentPage
{
    public EditSurveyPage(EditSurveyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
