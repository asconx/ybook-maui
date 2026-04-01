using yBook.Models;

namespace yBook.Views.Kalendarz
{
    public partial class KalendarzPage : ContentPage
    {
        const double DayW   = 40;
        const double RowH   = 44;
        const double BookH  = 26;

        int      _rok     = DateTime.Today.Year;
        int      _miesiac = DateTime.Today.Month;
        DateTime _start;
        int      _dni;

        static readonly string[] MonSkrot =
            { "sty","lut","mar","kwi","maj","cze","lip","sie","wrz","paź","lis","gru" };
        static readonly string[] DaySkrot =
            { "Nd","Pn","Wt","Śr","Cz","Pt","So" };
        static readonly string[] MonPelna =
            { "Styczeń","Luty","Marzec","Kwiecień","Maj","Czerwiec",
              "Lipiec","Sierpień","Wrzesień","Październik","Listopad","Grudzień" };

        List<KalendarzPokoj> _pokoje = [];
        bool _syncH, _syncV;

        public KalendarzPage()
        {
            InitializeComponent();
            Header.HamburgerClicked += (_, _) => Drawer.Open();
            _pokoje = MockDane();
            Render();
        }

        // ── Mock ───────────────────────────────────────────────────────────────
        static List<KalendarzPokoj> MockDane()
        {
            var rng  = new Random(7);
            var rok  = DateTime.Today.Year;
            var m    = DateTime.Today.Month;
            string[] nazwy = {
                "Mały pokój 1","Pokój dwuos. 2","Pokój cztero. 3","Pokój Dwuos. 4",
                "Pokój cztero. 5","Pokój Dwuos. 6","Pokój Dwuos. 7","Pokój Dwuos. 8",
                "Pokój Dwuos. 9","Pokój czter. 10","Pokój Dwuo. 11"
            };
            string[] opisy = {
                "booking2: CLOSED – Not available",
                "booking2: CLOSED – Maintenance",
                "booking2: CLOSED"
            };
            return nazwy.Select((n, i) => new KalendarzPokoj
            {
                Id    = i + 1,
                Nazwa = n,
                Rezerwacje = Enumerable.Range(0, rng.Next(1, 3)).Select(j =>
                {
                    int od = rng.Next(1, 22);
                    int dl = rng.Next(3, 9);
                    return new KalendarzRezerwacja
                    {
                        Id     = 6500 + i * 10 + j,
                        DataOd = new DateTime(rok, m, od),
                        DataDo = new DateTime(rok, m, Math.Min(od + dl, 28)),
                        Opis   = opisy[rng.Next(opisy.Length)],
                    };
                }).ToList()
            }).ToList();
        }

        // ── Render ─────────────────────────────────────────────────────────────
        void Render()
        {
            LblRok.Text          = _rok.ToString();
            LblMiesiacNazwa.Text = MonPelna[_miesiac - 1];
            _start = new DateTime(_rok, _miesiac, 1);
            _dni   = DateTime.DaysInMonth(_rok, _miesiac);

            BuildMonthBar();
            BuildDayHeaderAndGrid();
        }

        // ── Pasek miesięcy ─────────────────────────────────────────────────────
        void BuildMonthBar()
        {
            MonthBar.Children.Clear();
            for (int i = 1; i <= 12; i++)
            {
                int  cap    = i;
                bool active = i == _miesiac;
                var b = new Border
                {
                    StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                    BackgroundColor = active ? Color.FromArgb("#3AACA2") : Colors.Transparent,
                    Stroke          = Colors.Transparent,
                    Padding         = new Thickness(10, 5),
                    VerticalOptions = LayoutOptions.Center,
                    Content         = new Label
                    {
                        Text           = MonSkrot[i - 1],
                        FontSize       = 12,
                        FontAttributes = active ? FontAttributes.Bold : FontAttributes.None,
                        TextColor      = active ? Colors.White : Color.FromArgb("#555"),
                        VerticalOptions = LayoutOptions.Center,
                    }
                };
                b.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => { _miesiac = cap; Render(); })
                });
                MonthBar.Children.Add(b);
            }
        }

        // ── Nagłówek + siatka (budowane razem, ten sam DayW) ──────────────────
        void BuildDayHeaderAndGrid()
        {
            DayHeaderPanel.Children.Clear();
            RoomNamesPanel.Children.Clear();
            CalendarGrid.Children.Clear();

            double totalW = _dni * DayW;
            double totalH = _pokoje.Count * RowH;
            CalendarGrid.WidthRequest  = totalW;
            CalendarGrid.HeightRequest = totalH;

            // ── NAGŁÓWEK DNI ─────────────────────────────────────────────────
            // Każda komórka ma DOKŁADNIE DayW — bez separatorów między nimi
            for (int d = 0; d < _dni; d++)
            {
                var date  = _start.AddDays(d);
                bool wknd = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                bool dzis = date.Date == DateTime.Today;

                // Komórka nagłówka — ten sam kolor co tło kolumny w siatce
                var cell = new AbsoluteLayout
                {
                    WidthRequest    = DayW,
                    HeightRequest   = 38,
                    BackgroundColor = wknd ? Color.FromArgb("#EAEFF4") : Colors.White,
                };

                // Prawa linia oddzielająca kolumny
                var vline = new BoxView
                {
                    BackgroundColor = Color.FromArgb("#DEDEDE"),
                    WidthRequest    = 1,
                    HeightRequest   = 38,
                };
                AbsoluteLayout.SetLayoutBounds(vline, new Rect(DayW - 1, 0, 1, 38));
                cell.Children.Add(vline);

                // Skrót dnia tygodnia
                var lblDzien = new Label
                {
                    Text              = DaySkrot[(int)date.DayOfWeek],
                    FontSize          = 9,
                    TextColor         = wknd ? Color.FromArgb("#AAA") : Color.FromArgb("#999"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions   = LayoutOptions.Start,
                    Margin            = new Thickness(0, 4, 0, 0),
                    WidthRequest      = DayW,
                };
                AbsoluteLayout.SetLayoutBounds(lblDzien, new Rect(0, 2, DayW, 14));
                cell.Children.Add(lblDzien);

                // Numer dnia
                if (dzis)
                {
                    var circle = new Border
                    {
                        StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                        BackgroundColor = Color.FromArgb("#3AACA2"),
                        Stroke          = Colors.Transparent,
                        WidthRequest    = 22, HeightRequest = 15,
                        HorizontalOptions = LayoutOptions.Center,
                        Content = new Label
                        {
                            Text              = date.Day.ToString("D2"),
                            FontSize          = 9,
                            FontAttributes    = FontAttributes.Bold,
                            TextColor         = Colors.White,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions   = LayoutOptions.Center,
                        }
                    };
                    AbsoluteLayout.SetLayoutBounds(circle, new Rect((DayW - 22) / 2, 18, 22, 15));
                    cell.Children.Add(circle);
                }
                else
                {
                    var lblNum = new Label
                    {
                        Text              = date.Day.ToString("D2"),
                        FontSize          = 9,
                        TextColor         = wknd ? Color.FromArgb("#AAA") : Color.FromArgb("#444"),
                        HorizontalOptions = LayoutOptions.Center,
                        WidthRequest      = DayW,
                    };
                    AbsoluteLayout.SetLayoutBounds(lblNum, new Rect(0, 19, DayW, 14));
                    cell.Children.Add(lblNum);
                }

                // Pozycjonuj w AbsoluteLayout
                AbsoluteLayout.SetLayoutBounds(cell, new Rect(d * DayW, 0, DayW, 38));
                DayHeaderPanel.WidthRequest = _dni * DayW;
                DayHeaderPanel.Children.Add(cell);
            }

            // ── SIATKA POKOJÓW ───────────────────────────────────────────────
            for (int r = 0; r < _pokoje.Count; r++)
            {
                var    pokoj = _pokoje[r];
                double y     = r * RowH;
                int    rc    = r;

                // Nazwa pokoju
                var nameRow = new Grid
                {
                    HeightRequest   = RowH,
                    BackgroundColor = Colors.White,
                };
                nameRow.Children.Add(new Label
                {
                    Text            = $"{pokoj.Id} {pokoj.Nazwa}",
                    FontSize        = 12,
                    TextColor       = Color.FromArgb("#3AACA2"),
                    VerticalOptions = LayoutOptions.Center,
                    Margin          = new Thickness(10, 0, 4, 0),
                    LineBreakMode   = LineBreakMode.TailTruncation,
                });
                nameRow.Children.Add(new BoxView
                {
                    HeightRequest   = 1,
                    BackgroundColor = Color.FromArgb("#EBEBEB"),
                    VerticalOptions = LayoutOptions.End,
                });
                nameRow.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => OtworzPopup(_pokoje[rc]))
                });
                RoomNamesPanel.Children.Add(nameRow);

                // Tło komórek — każda kolumna ma DOKŁADNIE DayW
                for (int d = 0; d < _dni; d++)
                {
                    var date  = _start.AddDays(d);
                    bool wknd = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

                    // Tło kolumny
                    var bg = new BoxView
                    {
                        BackgroundColor = wknd ? Color.FromArgb("#EAEFF4") : Colors.White,
                    };
                    AbsoluteLayout.SetLayoutBounds(bg, new Rect(d * DayW, y, DayW, RowH));
                    CalendarGrid.Children.Add(bg);

                    // Prawa linia kolumny
                    var vl = new BoxView { BackgroundColor = Color.FromArgb("#DEDEDE") };
                    AbsoluteLayout.SetLayoutBounds(vl, new Rect((d + 1) * DayW - 1, y, 1, RowH));
                    CalendarGrid.Children.Add(vl);
                }

                // Dolna linia wiersza
                var hl = new BoxView { BackgroundColor = Color.FromArgb("#EBEBEB") };
                AbsoluteLayout.SetLayoutBounds(hl, new Rect(0, y + RowH - 1, totalW, 1));
                CalendarGrid.Children.Add(hl);

                // Rezerwacje
                foreach (var rez in pokoj.Rezerwacje)
                {
                    int sd = (int)(rez.DataOd - _start).TotalDays;
                    int ed = (int)(rez.DataDo - _start).TotalDays;
                    if (ed < 0 || sd >= _dni) continue;
                    sd = Math.Max(sd, 0);
                    ed = Math.Min(ed, _dni - 1);

                    double bx = sd * DayW + 2;
                    double bw = (ed - sd + 1) * DayW - 4;
                    double by = y + (RowH - BookH) / 2.0;

                    var blok = new Border
                    {
                        BackgroundColor = Color.FromArgb("#F5C842"),
                        StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 5 },
                        StrokeThickness = 0,
                        Padding         = new Thickness(7, 0),
                    };
                    var row = new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(),
                            new ColumnDefinition { Width = 14 },
                        }
                    };
                    row.Children.Add(new Label
                    {
                        Text            = $"#{rez.Id}: {rez.Opis}",
                        FontSize        = 9,
                        TextColor       = Color.FromArgb("#5D3A00"),
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode   = LineBreakMode.TailTruncation,
                    });
                    var arrowLbl = new Label
                    {
                        Text              = "›",
                        FontSize          = 13,
                        TextColor         = Color.FromArgb("#5D3A00"),
                        VerticalOptions   = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                    };
                    Grid.SetColumn(arrowLbl, 1);
                    row.Children.Add(arrowLbl);
                    blok.Content = row;

                    AbsoluteLayout.SetLayoutBounds(blok, new Rect(bx, by, bw, BookH));
                    CalendarGrid.Children.Add(blok);
                }
            }
        }

        // ── Nawigacja ──────────────────────────────────────────────────────────
        void OnPrevYear(object? s, TappedEventArgs e) { _rok--; Render(); }
        void OnNextYear(object? s, TappedEventArgs e) { _rok++; Render(); }

        // ── Sync scroll ───────────────────────────────────────────────────────
        async void OnGridHScrolled(object? s, ScrolledEventArgs e)
        {
            if (_syncH) return;
            _syncH = true;
            await DayHeaderScroll.ScrollToAsync(e.ScrollX, 0, false);
            _syncH = false;
        }
        async void OnDayHeaderScrolled(object? s, ScrolledEventArgs e)
        {
            if (_syncH) return;
            _syncH = true;
            await GridHScroll.ScrollToAsync(e.ScrollX, 0, false);
            _syncH = false;
        }
        async void OnRoomNamesScrolled(object? s, ScrolledEventArgs e)
        {
            if (_syncV) return;
            _syncV = true;
            await GridVScroll.ScrollToAsync(0, e.ScrollY, false);
            _syncV = false;
        }
        async void OnGridVScrolled(object? s, ScrolledEventArgs e)
        {
            if (_syncV) return;
            _syncV = true;
            await RoomNamesScroll.ScrollToAsync(0, e.ScrollY, false);
            _syncV = false;
        }

        // ── Popup ─────────────────────────────────────────────────────────────
        void OtworzPopup(KalendarzPokoj pokoj)
        {
            PopupTitle.Text = $"Kwatera #{pokoj.Id}";
            PopupRezerwacjeList.Children.Clear();

            foreach (var rez in pokoj.Rezerwacje)
            {
                var card = new Border
                {
                    BackgroundColor = Color.FromArgb("#F8F9FA"),
                    StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                    StrokeThickness = 0,
                    Padding         = new Thickness(12, 10),
                };
                var g = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(),
                        new ColumnDefinition { Width = 34 }
                    }
                };
                var info = new VerticalStackLayout { Spacing = 3 };
                info.Children.Add(new HorizontalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label
                        {
                            Text = rez.NumerRezerwacji, FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#1A1A1A")
                        },
                        new Border
                        {
                            BackgroundColor = Color.FromArgb("#F5C842"),
                            StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                            StrokeThickness = 0,
                            Padding         = new Thickness(5, 1),
                            VerticalOptions = LayoutOptions.Center,
                            Content         = new Label { Text = "●", FontSize = 7, TextColor = Color.FromArgb("#5D3A00") }
                        }
                    }
                });
                info.Children.Add(new Label { Text = rez.OkresStr, FontSize = 11, TextColor = Color.FromArgb("#555") });
                info.Children.Add(new Label { Text = rez.Opis, FontSize = 10, TextColor = Color.FromArgb("#888") });
                g.Add(info, 0, 0);

                var btn = new Border
                {
                    StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                    BackgroundColor = Colors.Transparent,
                    Stroke          = Color.FromArgb("#3AACA2"),
                    StrokeThickness = 1.5,
                    WidthRequest    = 30, HeightRequest = 30,
                    VerticalOptions   = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Content = new Label
                    {
                        Text = "›", FontSize = 15,
                        TextColor = Color.FromArgb("#3AACA2"),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions   = LayoutOptions.Center,
                    }
                };
                g.Add(btn, 1, 0);
                card.Content = g;
                PopupRezerwacjeList.Children.Add(card);
            }

            if (!pokoj.Rezerwacje.Any())
                PopupRezerwacjeList.Children.Add(
                    new Label { Text = "Brak rezerwacji", FontSize = 12, TextColor = Color.FromArgb("#999") });

            PopupOverlay.IsVisible = true;
        }

        void OnClosePopup(object? s, TappedEventArgs e) => PopupOverlay.IsVisible = false;
    }
}
