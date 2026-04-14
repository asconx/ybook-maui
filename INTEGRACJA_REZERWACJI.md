# 📋 Integracja Rezerwacji - PrzyjazdWyjazdPage

## Architektura

Strona `PrzyjazdWyjazdPage` jest przygotowana do importowania rezerwacji z `RezerwacjeOnlinePage` w przyszłości.

### Przepływ danych:

```
RezerwacjeOnlinePage (strona z formularzem rezerwacji)
        ↓
RezerwacjeOnlinePage.StaticRezerwacje (static list)
        ↓
PrzyjazdWyjazdPage.SyncFromApi()
        ↓
RezerwacjeOnlinePage.StaticRezerwacje.ToList() ← pobranie rezerwacji
        ↓
GenerateRezerwacjeLabel() ← wyświetlenie na stronie
```

## Obecna integracja

```csharp
// W PrzyjazdWyjazdPage.xaml.cs - SyncFromApi()
var rezerwacje = RezerwacjeOnlinePage.StaticRezerwacje.ToList();
```

Rezerwacje są pobierane bezpośrednio ze statycznej kolekcji `StaticRezerwacje` z klasy `RezerwacjeOnlinePage`.

## Wymagane pola modelu RezerwacjaOnline

Aby rezerwacje wyświetlały się prawidłowo, model `RezerwacjaOnline` musi zawierać:

```csharp
public string TypPokoju { get; set; }           // Nazwa pokoju (musi się zgadzać z PokojeRepo.Lista)
public DateTime DataPrzyjazdu { get; set; }     // Data przyjazdu
public DateTime DataWyjazdu { get; set; }       // Data wyjazdu
public string Imie { get; set; }                // Imię gościa
public string Nazwisko { get; set; }            // Nazwisko gościa
public string PelneNazwisko { get; set; }       // Calculated: Imie + Nazwisko
```

## Jak rezerwacje są wyświetlane

W metodzie `GenerateRezerwacjeLabel()`:

```csharp
// 1. Pobierz rezerwacje dla danego pokoju w danym miesiącu/roku
var filteredRez = rezerwacje
    .Where(r => string.Equals(r.TypPokoju, pokojNazwa, StringComparison.OrdinalIgnoreCase) &&
                ((r.DataPrzyjazdu.Year == rok && r.DataPrzyjazdu.Month == miesiac) ||
                 (r.DataWyjazdu.Year == rok && r.DataWyjazdu.Month == miesiac)))
    .ToList();

// 2. Wyświetl każdą rezerwację
// Przykład: "📅 Imię Nazwisko (05.12 – 11.12)"
labels.Add($"📅 {rez.PelneNazwisko} ({rez.DataPrzyjazdu:dd.MM} – {rez.DataWyjazdu:dd.MM})");
```

## Integracja z API (Stay)

Oprócz rezerwacji ze strony, wyświetlane są też pobierane z API:

```csharp
var stays = await PokojeRepo.FetchStaysAsync(_token);
```

Pobyty są wyświetlane jako:
```
🏨 Pobyt (05.12 – 11.12)
```

## Listy pokojów muszą się zgadzać

```csharp
// PokojeRepo.cs
new() { Id = 68, Nazwa = "Mały pokój 1" },
new() { Id = 69, Nazwa = "Pokój dwuosobowy typu Standard 2" },
...

// RezerwacjaOnline.TypPokoju MUSI być dokładnie taka sama
"Mały pokój 1"                              ← w rezerwacji
"Pokój dwuosobowy typu Standard 2"          ← w rezerwacji
```

## Wyzwalacze synchronizacji

Rezerwacje są synchronizowane:

1. **Przy otwieraniu strony** - `OnAppearing()`
2. **Przy zmianie roku** - `OnPrevYear()`, `OnNextYear()`, `OnYearSliderChanged()`
3. **Przy zmianie miesiąca** - `OnMonthSelected()`

## Struktura kodu

```
PrzyjazdWyjazdPage.xaml.cs
├─ LoadAndSyncData()              ← Pobiera token i uruchamia sync
├─ OnAppearing()                  ← Wyzwala LoadAndSyncData
├─ SyncFromApi()                  ← Główna metoda synchronizacji
│  ├─ FetchArrivalDepartureAvailabilityAsync()  ← Pobiera dostępność z API
│  ├─ RezerwacjeOnlinePage.StaticRezerwacje     ← Pobiera rezerwacje ze strony
│  ├─ FetchStaysAsync()           ← Pobiera pobyty z API
│  └─ GenerateRezerwacjeLabel()   ← Generuje etykiety rezerwacji
└─ Obsługi kliknięć
   ├─ OnMonthSelected()
   ├─ OnPrevYear()
   ├─ OnNextYear()
   ├─ OnArrivalTapped()
   └─ OnDepartureTapped()
```

## Przyszłe rozszerzenia

### Możliwość 1: Dodanie przycisku "Dodaj rezerwację"
```xaml
<Button Text="+ Rezerwacja"
        Clicked="OnAddReservation"
        HorizontalOptions="Fill"/>
```

```csharp
async void OnAddReservation(object sender, EventArgs e)
{
    await Shell.Current.GoToAsync("RezerwacjeOnlinePage");
}
```

### Możliwość 2: Automatyczne odświeżanie po dodaniu rezerwacji
```csharp
protected override void OnAppearing()
{
    base.OnAppearing();
    LoadAndSyncData(); // Zawsze odświeża przy wejściu na stronę
}
```

### Możliwość 3: Synchronizacja w tle
```csharp
// W SyncFromApi()
// Dodaj timer do okresowego pobierania danych
Device.StartTimer(TimeSpan.FromSeconds(30), () =>
{
    _ = SyncFromApi();
    return true; // true = kontynuuj timer
});
```

## Testowanie

1. Otwórz `RezerwacjeOnlinePage`
2. Dodaj rezerwację:
   - Typ pokoju: "Mały pokój 1"
   - Data przyjazdu: 05.12.2024
   - Data wyjazdu: 11.12.2024
3. Otwórz `PrzyjazdWyjazdPage`
4. Ustaw rok: 2024, miesiąc: Grudzień
5. Powinna się wyświetlić:
   ```
   Mały pokój 1
   📅 Imię Nazwisko (05.12 – 11.12) • 🏨 Pobyt (...)
   ```

## API Endpoints

| Endpoint | Metoda | Cel |
|----------|--------|-----|
| `/entity/arrivalDepartureAvailability` | GET | Dostępność przyjazdów/wyjazdów |
| `/entity/stay` | GET | Pobyty z systemu rezerwacji |
| `/entity/reservation` | GET | Rezerwacje (nie używane, ale dostępne) |

## Notatki

- ✅ Kod jest czysty od logów debugowania
- ✅ Wszystkie testy są usunięte
- ✅ Rezerwacje są importowane z `RezerwacjeOnlinePage.StaticRezerwacje`
- ✅ Rezerwacje z API (`Stay`) są też wyświetlane
- ✅ Strona jest gotowa do rozszerzeń
