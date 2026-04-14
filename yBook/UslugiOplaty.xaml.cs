using System.Collections.ObjectModel;
using yBook.Models;

namespace yBook.Views.Ceny;

public partial class UslugiOplaty : ContentPage
{
	// =========================================
	//                VARIABLES
	// =========================================
	List<Usluga> _all = new();
    string? _typ = null;

	// =========================================
	//                   START
	// =========================================
	public UslugiOplaty()
	{
		InitializeComponent();
	}
	protected override void OnAppearing()
    {
        base.OnAppearing();

        _all.AddRange(MockData());
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
		_all.Add(new Usluga()
		{
			Name = "Cassian",
			Rodzaj = "Bestia",
			Typ = "DND",
			Ceny = new() { "67 zł" },
			DataOd = "1-01-01",
			DataDo = "9999-12-31",
			Opis = "Lassian"
		});

		ApplyFilter();
	}
	void OnDeleteClicked(object sender, EventArgs e)
	{
		if (sender is BindableObject bo &&
			bo.BindingContext is Usluga item)
		{
			_all.Remove(item);
			ApplyFilter();
		}
	}
	// =========================================
	//             PRIVATE ACTIONS
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