namespace yBook.Views.Ceny;

public partial class UslugiOplaty : ContentPage
{
	// =========================================
	//                VARIABLES
	// =========================================
	private List<Usluga> _all = new();
	private string? _typ = null;
	
	// =========================================
	//                   START
	// =========================================
	public UslugiOplaty()
	{
		InitializeComponent();
	}

	private bool _initialized = false;
	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (!_initialized)
		{
			_all.AddRange(MockData());
			_initialized = true;
		}

		ApplyFilter();
	}

	// =========================================
	//                    EVENTS
	// =========================================
	void OnSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

	async void OnTypTapped(object sender, EventArgs e)
    {
        var options = new[] { "Wszystkie", "Opłata dzienna", "Jednorazowa" };
        var res = await DisplayActionSheet("Typ", "Anuluj", null, options);

        if (res == "Anuluj") return;

        _typ = res == "Wszystkie" ? null : res;
        LblTyp.Text = res;

        ApplyFilter();
    }

	async void OnDodajClicked(object sender, EventArgs e)
	{
		var page = new UslugiOplatyDodawanie(new Usluga(), modif: false, () => { ApplyFilter(); });
		await Shell.Current.Navigation.PushAsync(page);
	}
	async void OnModifClicked(object sender, TappedEventArgs e)
	{
		var item = e.Parameter as Usluga;

		if (item == null) return;

		var page = new UslugiOplatyDodawanie(
			item,
			modif: true,
			() => { ApplyFilter(); }
		);

		await Shell.Current.Navigation.PushAsync(page);
	}
	public void Dodaj(Usluga usluga)
    {
        _all.Add(usluga);
    }
	void OnDeleteClicked(object sender, TappedEventArgs e)
	{
		var item = e.Parameter as Usluga;

		if (item == null) return;

		_all.Remove(item);
		ApplyFilter();
	}

	// =========================================
	//             PRIVATE METHODS
	// =========================================
	private void ApplyFilter()
    {
        var q = Search.Text?.ToLower() ?? "";

        var result = _all.Where(x =>
            (_typ == null || x.Rodzaj == _typ) &&
            (string.IsNullOrEmpty(q) || x.Name.ToLower().Contains(q))
        ).ToList();

        Lista.ItemsSource = result;
        LblCount.Text = result.Count.ToString();
    }
    private List<Usluga> MockData()
    {
        return new List<Usluga>
        {
            new Usluga
            {
                Name="Dostawka",
                Rodzaj="Opłata dzienna",
                Typ="Dodatkowa usługa",
                Ceny=new(){"100 zł"}
            },
            new Usluga
            {
                Name="Opłata miejscowa",
                Rodzaj="Opłata dzienna",
                Typ="Obowiązkowa opłata",
                Ceny=new(){"3.22 zł dorosły","3.22 zł dziecko"},
                DataOd="2024-01-01",
                DataDo="2024-12-31"
            }
        };
    }
}

public class Usluga
{
    public string Name { get; set; }
    public string Rodzaj { get; set; }
    public string Typ { get; set; }
    public List<string> Ceny { get; set; } = new();
    public string DataOd { get; set; }
    public string DataDo { get; set; }
    public string Opis { get; set; }
}