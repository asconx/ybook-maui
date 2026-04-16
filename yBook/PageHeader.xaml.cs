namespace yBook.Controls
{
    public partial class PageHeader : ContentView
    {
        // ── Bindable Properties ───────────────────────────────────────────────

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(PageHeader),
                defaultValue: "yBook",
                propertyChanged: (b, _, n) => ((PageHeader)b).LblTitle.Text = (string)n);

        public static readonly BindableProperty IconProperty =
            BindableProperty.Create(nameof(Icon), typeof(string), typeof(PageHeader),
                defaultValue: "🏨",
                propertyChanged: (b, _, n) => ((PageHeader)b).LblIcon.Text = (string)n);

        /// <summary>
        /// True  → lewa ikona to strzałka wstecz (podstrony)
        /// False → lewa ikona to hamburger (MainPage)
        /// </summary>
        public static readonly BindableProperty ShowBackProperty =
            BindableProperty.Create(nameof(ShowBack), typeof(bool), typeof(PageHeader),
                defaultValue: false,
                propertyChanged: (b, _, n) => ((PageHeader)b).UpdateLeftButton((bool)n));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool ShowBack
        {
            get => (bool)GetValue(ShowBackProperty);
            set => SetValue(ShowBackProperty, value);
        }

        // ── Zdarzenia ─────────────────────────────────────────────────────────

        /// <summary>Wywoływane gdy ShowBack=False i użytkownik kliknie hamburger.</summary>
        public event EventHandler? HamburgerClicked;

        public PageHeader()
        {
            InitializeComponent();
        }

        void UpdateLeftButton(bool showBack)
        {
            // Strzałka wstecz ← vs hamburger ☰
            LeftBtnIcon.Glyph = showBack ? "\u2190" : "\u2630";
        }

        async void OnLeftBtnClicked(object? sender, EventArgs e)
        {
            if (ShowBack)
            {
                // Wróć do MainPage zamiast ".."
                await Shell.Current.GoToAsync("///MainPage");
            }
            else
            {
                HamburgerClicked?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
