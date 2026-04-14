using yBook.Models;
using yBook.Services;

namespace yBook.Views.Finanse;

public partial class FinancialDocumentsPage : ContentPage
{
    private readonly FinancialService _service;
    private int _serviceId = 17;

    public FinancialDocumentsPage(FinancialService service)
    {
        _service = service;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            var start = DatePickerStart != null ? $"{DatePickerStart.Date:yyyy-MM-dd}" : string.Empty;
            var end = DatePickerEnd != null ? $"{DatePickerEnd.Date:yyyy-MM-dd}" : string.Empty;
            var res = await _service.GetByServiceId(_serviceId, 0, start, end);
            if (res != null)
                DocsList.ItemsSource = res.Items;
            else
                await DisplayAlert("Błąd", "Nie udało się pobrać dokumentów.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", ex.Message, "OK");
        }
    }

    async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadData();
    }

    async void OnAddClicked(object sender, EventArgs e)
    {
        // Zapytaj użytkownika o podstawowe wartości przed wysłaniem
        var amountStr = await DisplayPromptAsync("Kwota brutto", "Podaj kwotę brutto:", initialValue: "100.00");
        if (string.IsNullOrWhiteSpace(amountStr)) return;
        if (!decimal.TryParse(amountStr, out var amount))
        {
            await DisplayAlert("Błąd", "Nieprawidłowa kwota.", "OK");
            return;
        }

        var desc = await DisplayPromptAsync("Opis", "Opis dokumentu:", initialValue: "Z aplikacji MAUI");
        var docNumber = await DisplayPromptAsync("Numer dokumentu", "Numer dokumentu:", initialValue: "YB/APP/" + DateTime.UtcNow.Ticks);

        var dto = new FinancialCreateDto
        {
            ServiceId = _serviceId,
            AmountGross = amount,
            Description = desc,
            IssueDate = DatePickerStart != null ? $"{DatePickerStart.Date:yyyy-MM-dd}" : null,
            DocumentNumber = docNumber
        };

        var ok = await _service.CreateDocument(dto);
        if (ok)
        {
            await DisplayAlert("Sukces", "Utworzono dokument na serwerze.", "OK");
            await LoadData();
        }
        else
        {
            await DisplayAlert("Błąd", "Nie udało się utworzyć dokumentu.", "OK");
        }
    }

    async void OnFilterClicked(object sender, EventArgs e)
    {
        await LoadData();
    }
}
