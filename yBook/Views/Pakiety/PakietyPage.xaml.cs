using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using yBook.Services;

namespace yBook.Views.Pakiety;

public partial class PakietyPage : ContentPage
{
    private readonly List<Pakiet> _all = new();
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public PakietyPage(IAuthService authService)
    {
        InitializeComponent();

        _authService = authService;
        _httpClient = new HttpClient();

        // eventy UI
        BtnBack.Clicked += OnBackClicked;
        BtnAdd.Clicked += OnDodajClicked;
        BtnActivate.Clicked += async (_, __) => await ToggleActiveSelected(true);
        BtnDeactivate.Clicked += async (_, __) => await ToggleActiveSelected(false);
        BtnDeleteSelected.Clicked += async (_, __) => await DeleteSelectedAsync();
        BtnExport.Clicked += async (_, __) => await ExportSelectedToCsvAsync();
        BtnDuplicate.Clicked += async (_, __) => await DuplicateSelectedAsync();

        Search.TextChanged += (_, __) => ApplyFilter();
        PickerSort.SelectedIndexChanged += (_, __) => ApplyFilter();

        // inicjalnie: sort domyœlny
        PickerSort.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPakiety();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (Shell.Current.Navigation.NavigationStack.Count > 1)
            await Shell.Current.Navigation.PopAsync();
        else
            await Shell.Current.GoToAsync("//");
    }

    private async void OnDodajClicked(object sender, EventArgs e)
    {
        var form = new PakietFormPage(_authService, pakiet: null, modif: false, refresh: ApplyFilter);
        form.OnSaved += _ => MainThread.BeginInvokeOnMainThread(async () => await LoadPakiety());
        await Shell.Current.Navigation.PushAsync(form);
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement ve && ve.BindingContext is Pakiet p)
        {
            var form = new PakietFormPage(_authService, pakiet: p, modif: true, refresh: ApplyFilter);
            form.OnSaved += updated =>
            {
                var idx = _all.IndexOf(p);
                if (idx >= 0) _all[idx] = updated;
                MainThread.BeginInvokeOnMainThread(ApplyFilter);
            };
            await Shell.Current.Navigation.PushAsync(form);
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement ve && ve.BindingContext is Pakiet p)
        {
            var ok = await DeletePakietAsync(p.Id);
            if (ok)
            {
                _all.Remove(p);
                ApplyFilter();
            }
        }
    }

    private async Task LoadPakiety()
    {
        try
        {
            if (!await _auth_service_is_authenticated_safe()) return;

            var token = await GetTokenSafeAsync();
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("B³¹d", "Nie znaleziono tokenu.", "OK");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.ybook.pl/entity/package");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("B³¹d", $"Nie uda³o siê za³adowaæ pakietów: {response.StatusCode}", "OK");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<PackageResponse>(json, options);

            _all.Clear();
            if (result?.Items != null)
            {
                foreach (var item in result.Items)
                {
                    _all.Add(new Pakiet
                    {
                        Id = item.Id,
                        Nazwa = item.Name,
                        Cena = item.Price,
                        ZdjecieUrl = item.ImageUrl,
                        DataOd = item.DateFrom,
                        DataDo = item.DateTo,
                        Opis = item.Description,
                        IsActive = false,
                        IsSelected = false
                    });
                }
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", $"B³¹d: {ex.Message}", "OK");
        }
    }

    private async Task<bool> DeletePakietAsync(int id)
    {
        try
        {
            var token = await GetTokenSafeAsync();
            if (string.IsNullOrEmpty(token)) return false;

            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.ybook.pl/entity/package/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task TryUpdateActiveRemote(int id, bool isActive)
    {
        try
        {
            var token = await GetTokenSafeAsync();
            if (string.IsNullOrEmpty(token)) return;

            var req = new HttpRequestMessage(HttpMethod.Patch, $"https://api.ybook.pl/entity/package/{id}/active")
            {
                Content = new StringContent(JsonSerializer.Serialize(new { active = isActive }), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await _httpClient.SendAsync(req);
        }
        catch
        {
            // ignoruj — nie blokujemy UI
        }
    }

    private async Task<string?> GetTokenSafeAsync()
    {
        try
        {
            return await _authService.GetTokenAsync();
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> _auth_service_is_authenticated_safe()
    {
        try
        {
            if (!await _authService.IsAuthenticatedAsync())
            {
                await DisplayAlert("B³¹d", "Brak autoryzacji. Zaloguj siê ponownie.", "OK");
                return false;
            }
            return true;
        }
        catch
        {
            await DisplayAlert("B³¹d", "B³¹d autoryzacji.", "OK");
            return false;
        }
    }

    private async Task ToggleActiveSelected(bool makeActive)
    {
        var selected = _all.Where(p => p.IsSelected).ToList();
        if (!selected.Any()) return;

        foreach (var p in selected)
        {
            p.IsActive = makeActive;
            p.IsSelected = false;
            _ = TryUpdateActiveRemote(p.Id, p.IsActive);
        }
        ApplyFilter();
    }

    private async Task DeleteSelectedAsync()
    {
        var selected = _all.Where(p => p.IsSelected).ToList();
        if (!selected.Any()) return;

        foreach (var p in selected)
        {
            var ok = await DeletePakietAsync(p.Id);
            if (ok) _all.Remove(p);
        }
        ApplyFilter();
    }

    private async Task DuplicateSelectedAsync()
    {
        var selected = _all.Where(p => p.IsSelected).ToList();
        if (!selected.Any()) return;

        foreach (var p in selected)
        {
            var copy = new Pakiet
            {
                Id = 0,
                Nazwa = p.Nazwa + " (kop.)",
                Cena = p.Cena,
                ZdjecieUrl = p.ZdjecieUrl,
                DataOd = p.DataOd,
                DataDo = p.DataDo,
                Opis = p.Opis,
                IsActive = false,
                IsSelected = false
            };
            _all.Add(copy);
        }
        ApplyFilter();
    }

    private async Task ExportSelectedToCsvAsync()
    {
        var selected = _all.Where(p => p.IsSelected).ToList();
        if (!selected.Any()) return;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id;Nazwa;Cena;DataOd;DataDo;Opis;IsActive");
            foreach (var p in selected)
            {
                var line = $"{p.Id};\"{EscapeCsv(p.Nazwa)}\";{p.Cena};{p.DataOd};{p.DataDo};\"{EscapeCsv(p.Opis)}\";{p.IsActive}";
                sb.AppendLine(line);
            }

            var filename = $"pakiety_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var path = Path.Combine(FileSystem.AppDataDirectory, filename);
            await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);

            await DisplayAlert("Eksport", $"Zapisano: {path}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", $"Nie uda³o siê wyeksportowaæ: {ex.Message}", "OK");
        }
    }

    private static string EscapeCsv(string? s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Replace("\"", "\"\"");
    }

    private void ApplyFilter()
    {
        var q = (Search.Text ?? string.Empty).Trim().ToLowerInvariant();
        var list = _all.Where(x => string.IsNullOrEmpty(q) || (x.Nazwa?.ToLowerInvariant().Contains(q) == true || (x.Opis?.ToLowerInvariant().Contains(q) == true))).ToList();

        // sortowanie wg wyboru
        switch (PickerSort.SelectedIndex)
        {
            case 1:
                list = list.OrderBy(x => x.Nazwa).ToList();
                break;
            case 2:
                list = list.OrderBy(x => x.Cena).ToList();
                break;
            case 3:
                list = list.OrderByDescending(x => x.Cena).ToList();
                break;
            case 4:
                list = list.OrderByDescending(x => x.IsActive).ThenBy(x => x.Nazwa).ToList();
                break;
            default:
                break;
        }

        Lista.ItemsSource = list;
        LblCount.Text = list.Count.ToString();
    }

    // API response models
    private class PackageItem { public int Id { get; set; } public string Name { get; set; } = ""; public decimal Price { get; set; } public string? ImageUrl { get; set; } public string? DateFrom { get; set; } public string? DateTo { get; set; } public string? Description { get; set; } }
    private class PackageResponse { public List<PackageItem> Items { get; set; } = new(); public int Total { get; set; } }
}