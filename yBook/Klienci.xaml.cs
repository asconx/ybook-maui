using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;
using yBook.Services;

namespace yBook.Views.Klienci
{
    public partial class KlienciPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<Client> _allClients = new List<Client>();
        private int _currentPage = 1;
        private const int PageSize = 50;

        public KlienciPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            RefreshClientList();
        }

        void OnAddClientClicked(object sender, EventArgs e)
        {
            ListaSection.IsVisible = false;
            FormSection.IsVisible = true;
        }

        void OnMoreDataChecked(object sender, CheckedChangedEventArgs e)
        {
            MoreDataSection.IsVisible = e.Value;

            MoreDataLabel.Text = e.Value
                ? "Pokaż mniej"
                : "Więcej danych klienta";
        }

        void OnMoreDataLabelTapped(object sender, EventArgs e)
        {
            CbWiecejDanych.IsChecked = !CbWiecejDanych.IsChecked;
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Walidacja
                if (string.IsNullOrWhiteSpace(NazwaEntry.Text))
                {
                    await DisplayAlert("Błąd", "Imię i nazwisko jest wymagane", "OK");
                    return;
                }

                var client = new Client
                {
                    Name = NazwaEntry.Text,
                    Email = EmailEntry.Text,
                    Phone = TelefonEntry.Text,
                    Nip = NipEntry.Text,
                    Notes = UwagiEntry.Text,
                    Discount = int.TryParse(RabatEntry.Text, out var r) ? r : 0,
                    Type = RbStalyKlient.IsChecked ? 1 : 0,
                    CompanyName = FirmaEntry.Text,
                    Pesel = PeselEntry.Text,
                    Street = UlicaEntry.Text,
                    HouseNumber = DomEntry.Text,
                    ApartmentNumber = LokalEntry.Text,
                    PostalCode = KodEntry.Text,
                    City = MiastoEntry.Text
                };

                // Dodaj klienta przez API
                await _apiService.AddClientAsync(client);

                await DisplayAlert("Sukces", "Klient został dodany pomyślnie!", "OK");

                ClearForm();

                FormSection.IsVisible = false;
                ListaSection.IsVisible = true;

                RefreshClientList();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Nie udało się dodać klienta: {ex.Message}", "OK");
            }
        }

        void OnCancelClicked(object sender, EventArgs e)
        {
            FormSection.IsVisible = false;
            ListaSection.IsVisible = true;
        }

        async void RefreshClientList()
        {
            try
            {
                _allClients = await _apiService.GetClientsAsync();
                _currentPage = 1;
                DisplayPage(_currentPage);
                UpdatePaginationUI();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Nie udało się pobrać klientów: {ex.Message}", "OK");
            }
        }

        void DisplayPage(int pageNumber)
        {
            var skip = (pageNumber - 1) * PageSize;
            var pageData = _allClients.Skip(skip).Take(PageSize).ToList();
            ClientsList.ItemsSource = pageData;
        }

        void UpdatePaginationUI()
        {
            int totalPages = (int)Math.Ceiling((double)_allClients.Count / PageSize);
            PaginationLabel.Text = $"Strona {_currentPage} z {totalPages}";

            PrevButton.IsEnabled = _currentPage > 1;
            NextButton.IsEnabled = _currentPage < totalPages;
        }

        void OnPrevPageClicked(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                DisplayPage(_currentPage);
                UpdatePaginationUI();
            }
        }

        void OnNextPageClicked(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_allClients.Count / PageSize);
            if (_currentPage < totalPages)
            {
                _currentPage++;
                DisplayPage(_currentPage);
                UpdatePaginationUI();
            }
        }

        void ClearForm()
        {
            NazwaEntry.Text = "";
            EmailEntry.Text = "";
            TelefonEntry.Text = "";
            RabatEntry.Text = "0";
            NipEntry.Text = "";
            UwagiEntry.Text = "";
            FirmaEntry.Text = "";
            PeselEntry.Text = "";
            UlicaEntry.Text = "";
            DomEntry.Text = "";
            LokalEntry.Text = "";
            KodEntry.Text = "";
            MiastoEntry.Text = "";

            RbStalyKlient.IsChecked = true;
            CbWiecejDanych.IsChecked = false;
            MoreDataSection.IsVisible = false;
            MoreDataLabel.Text = "Więcej danych klienta";
        }
    }

    // MODEL
    public class Client
    {
        public int Id { get; set; }

        [JsonPropertyName("organization_id")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("date_modified")]
        public string DateModified { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("additional_info")]
        public string Notes { get; set; }

        [JsonPropertyName("pesel")]
        public string Pesel { get; set; }

        [JsonPropertyName("id_number")]
        public string IdNumber { get; set; }

        [JsonPropertyName("nip")]
        public string Nip { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("has_newsletter_consent")]
        public int HasNewsletterConsent { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("house_number")]
        public string HouseNumber { get; set; }

        [JsonPropertyName("apartment_number")]
        public string ApartmentNumber { get; set; }

        [JsonPropertyName("discount")]
        public int Discount { get; set; }

        public string TypeText => Type == 0 ? "Niechciany" : "Stały";
    }

    // WRAPPER (na wypadek że API zwraca wrapper)
    public class ClientWrapper
    {
        [JsonPropertyName("items")]
        public List<Client> Items { get; set; }

        [JsonPropertyName("data")]
        public List<Client> Data { get; set; }

        public List<Client> GetClients() => Items ?? Data ?? new List<Client>();
    }

    // API SERVICE
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private const string BaseUrl = "https://api.ybook.pl/entity/client";

        public ApiService()
        {
            _httpClient = new HttpClient();
            _authService = IPlatformApplication.Current.Services.GetService<IAuthService>();
        }

        public async Task<List<Client>> GetClientsAsync()
        {
            try
            {
                // Pobierz token z AuthService
                var token = await _authService.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var httpResponse = await _httpClient.GetAsync(BaseUrl);
                var content = await httpResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"========== API DEBUG ==========");
                System.Diagnostics.Debug.WriteLine($"URL: {BaseUrl}");
                System.Diagnostics.Debug.WriteLine($"Status: {httpResponse.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response Content: {content}");
                System.Diagnostics.Debug.WriteLine($"========== END DEBUG ==========");

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error: {httpResponse.StatusCode}");
                }

                // Deserialize wrapper response
                var wrapperResponse = await httpResponse.Content.ReadFromJsonAsync<ClientWrapper>();

                System.Diagnostics.Debug.WriteLine($"Parsed {wrapperResponse?.Items?.Count ?? 0} clients from API");
                return wrapperResponse?.Items ?? new List<Client>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetClientsAsync Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Client> AddClientAsync(Client client)
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var content = JsonContent.Create(client);
            var response = await _httpClient.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Client>();
        }
    }
}