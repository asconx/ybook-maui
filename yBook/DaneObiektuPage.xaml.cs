using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace yBook.Views.Ustawienia
{
    public partial class DaneObiektuPage : ContentPage
    {
        // ── Zakładki ──────────────────────────────────────────────────────────

        record TabDef(string Label, View Panel);

        List<TabDef> _tabs = new();
        int _activeTab = 0;
        readonly List<Label> _tabLabels = new();

        // Akcje check-in
        static readonly (string Label, bool DefaultOn)[] AkcjeZameldowanie =
        {
            ("Sprawdzenie rezerwacji",                  true),
            ("Weryfikacja tożsamości",                  false),
            ("Rejestracja danych",                      false),
            ("Rejestracja pojazdu",                     false),
            ("Informowanie o udogodnieniach i usługach",false),
            ("Przekazanie klucza",                      true),
            ("Przekazanie karty do pokoju",             false),
            ("Przekazanie opaski",                      false),
            ("Wsparcie z bagażem",                      false),
            ("Wyjaśnienie zasad obiektu",               true),
            ("Przekazanie ulotki",                      false),
            ("Przekazanie formularza",                  false),
            ("Karta pobytu",                            false),
            ("Voucher do restauracji",                  false),
            ("Informacja dla posiadaczy psów",          false),
        };

        static readonly (string Label, bool DefaultOn)[] AkcjeWymeldowanie =
        {
            ("Sprawdzenie stanu kwatery",  false),
            ("Rozliczenie rachunku",       false),
            ("Zwrot klucza",               true),
            ("Zwrot karty do pokoju",      false),
            ("Zwrot opaski",               false),
            ("Zapytanie o opinię",         false),
            ("Przekazanie ankiety",        false),
            ("Wysłanie ankiety",           false),
            ("Zorganizowanie transportu",  false),
        };

        readonly Dictionary<string, bool> _zameldowanieState = new();
        readonly Dictionary<string, bool> _wymeldowanieState = new();

        public DaneObiektuPage()
        {
            InitializeComponent();
            BudujTaby();
            BudujAkcjeCheckIn();
        }

        // ── Budowanie paska zakładek ──────────────────────────────────────────

        void BudujTaby()
        {
            // Use named fields generated from XAML
            var tabBar = TabBar;

            _tabs = new List<TabDef>
            {
                new("dane podstawowe",      TabDanePodstawowe),
                new("check-in i check-out", TabCheckIn),
                new("logo i zdjęcia",       TabLogo),
                new("opis miejsca",         TabOpis),
                new("przedpłata",           TabPrzedplata),
                new("metody płatności",     TabPlatnosci),
                new("strona internetowa i sm", TabStrona),
                new("grupy wiekowe",        TabGrupy),
                new("drukaki",              TabDrukaki),
                new("smart locks",          TabSmartLocks),
                new("mapa",                 TabMapa),
            };

            for (int i = 0; i < _tabs.Count; i++)
            {
                int idx = i;
                var lbl = new Label
                {
                    Text = _tabs[i].Label,
                    FontSize = 13,
                    Padding = new Thickness(14, 12),
                    TextColor = Color.FromArgb("#607D8B"),
                };

                var underline = new BoxView
                {
                    HeightRequest = 2,
                    BackgroundColor = Colors.Transparent,
                    Margin = new Thickness(14, 0),
                };

                var col = new VerticalStackLayout
                {
                    Spacing = 0,
                    Children = { lbl, underline }
                };

                col.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => SelectTab(idx))
                });

                _tabLabels.Add(lbl);
                tabBar.Add(col);
            }

            SelectTab(0);
        }

        void SelectTab(int idx)
        {
            var tabBar = TabBar;

            for (int i = 0; i < _tabs.Count; i++)
            {
                bool active = i == idx;
                _tabs[i].Panel.IsVisible = active;

                var lbl = _tabLabels[i];
                lbl.TextColor = active
                    ? Color.FromArgb("#1565C0")
                    : Color.FromArgb("#607D8B");
                lbl.FontAttributes = active ? FontAttributes.None : FontAttributes.None;

                // Underline
                var col = (VerticalStackLayout)tabBar.Children[i];
                var underline = (BoxView)col.Children[1];
                underline.BackgroundColor = active
                    ? Color.FromArgb("#1565C0")
                    : Colors.Transparent;
            }

            _activeTab = idx;
        }

        // ── Akcje check-in / check-out ────────────────────────────────────────

        void BudujAkcjeCheckIn()
        {
            var flexZ = FlexZameldowanie;
            var flexW = FlexWymeldowanie;

            foreach (var (label, on) in AkcjeZameldowanie)
            {
                _zameldowanieState[label] = on;
                flexZ?.Add(BudujChip(label, on, _zameldowanieState));
            }
            foreach (var (label, on) in AkcjeWymeldowanie)
            {
                _wymeldowanieState[label] = on;
                flexW?.Add(BudujChip(label, on, _wymeldowanieState));
            }
        }

        View BudujChip(string label, bool active, Dictionary<string, bool> state)
        {
            var frame = new Frame
            {
                CornerRadius = 20,
                Padding = new Thickness(12, 6),
                HasShadow = false,
                BackgroundColor = active ? Color.FromArgb("#FFCCBC") : Color.FromArgb("#F5F5F5"),
                BorderColor = active ? Color.FromArgb("#FF8A65") : Color.FromArgb("#E0E0E0"),
            };

            var row = new HorizontalStackLayout { Spacing = 4 };
            var check = new Label
            {
                Text = "✓",
                FontSize = 12,
                TextColor = Color.FromArgb("#E64A19"),
                IsVisible = active,
                VerticalOptions = LayoutOptions.Center
            };
            var lbl = new Label
            {
                Text = label,
                FontSize = 12,
                TextColor = active ? Color.FromArgb("#BF360C") : Color.FromArgb("#607D8B"),
                VerticalOptions = LayoutOptions.Center
            };

            row.Children.Add(check);
            row.Children.Add(lbl);
            frame.Content = row;

            frame.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    state[label] = !state[label];
                    bool nowActive = state[label];
                    frame.BackgroundColor = nowActive ? Color.FromArgb("#FFCCBC") : Color.FromArgb("#F5F5F5");
                    frame.BorderColor = nowActive ? Color.FromArgb("#FF8A65") : Color.FromArgb("#E0E0E0");
                    check.IsVisible = nowActive;
                    lbl.TextColor = nowActive ? Color.FromArgb("#BF360C") : Color.FromArgb("#607D8B");
                })
            });

            return frame;
        }

        // ── Zdarzenia ─────────────────────────────────────────────────────────

        async void OnZapisz(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                string orig = btn.Text;
                btn.Text = "✓ Zapisano";
                btn.BackgroundColor = Color.FromArgb("#43A047");
                await Task.Delay(1500);
                btn.Text = orig;
                btn.BackgroundColor = Color.FromArgb("#1E293B");
            }
        }

        async void OnWybierzLogo(object? sender, TappedEventArgs e)
        {
            await DisplayAlert("Logo", "Wybierz plik z logo obiektu.", "OK");
        }

        async void OnWybierzZdjecia(object? sender, TappedEventArgs e)
        {
            await DisplayAlert("Zdjęcia", "Wybierz zdjęcia obiektu.", "OK");
        }
    }
}
