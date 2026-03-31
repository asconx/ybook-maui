using System.Collections.ObjectModel;

namespace yBook.Views.Pakiety;

public partial class PakietyPage : ContentPage
{
    private ObservableCollection<PakietModel> _allPakiety = new();
    private ObservableCollection<PakietModel> _filteredPakiety = new();

    private bool _sortAsc = true;
    private int? _editingId = null;

    public PakietyPage()
    {
        InitializeComponent();

        PakietyList.ItemsSource = _filteredPakiety;

        Seed();
        RefreshList();
    }

    // 🔹 MOCK DATA
    private void Seed()
    {
        _allPakiety.Add(new PakietModel
        {
            Id = 1,
            Nazwa = "Weekend SPA",
            Cena = 599,
            LiczbaDni = 2,
            Uslugi = "Spa, Śniadanie, Masaż",
            Status = "Aktywny"
        });

        _allPakiety.Add(new PakietModel
        {
            Id = 2,
            Nazwa = "Rodzinny",
            Cena = 899,
            LiczbaDni = 3,
            Uslugi = "Śniadanie, Basen",
            Status = "Nieaktywny"
        });
    }

    // 🔹 REFRESH
    private void RefreshList()
    {
        var query = _allPakiety.AsEnumerable();

        // sort
        query = _sortAsc
            ? query.OrderBy(x => x.Cena)
            : query.OrderByDescending(x => x.Cena);

        _filteredPakiety.Clear();

        foreach (var item in query)
            _filteredPakiety.Add(item);

        LblCount.Text = _filteredPakiety.Count.ToString();
        LblSort.Text = _sortAsc ? "Cena ↑" : "Cena ↓";
    }

    // 🔹 SEARCH
    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue?.ToLower() ?? "";

        var filtered = _allPakiety
            .Where(x => x.Nazwa.ToLower().Contains(text))
            .ToList();

        _filteredPakiety.Clear();
        foreach (var item in filtered)
            _filteredPakiety.Add(item);
    }

    // 🔹 SORT
    private void OnSortClicked(object sender, EventArgs e)
    {
        _sortAsc = !_sortAsc;
        RefreshList();
    }

    // 🔹 ADD
    private void OnAddClicked(object sender, EventArgs e)
    {
        _editingId = null;

        EntName.Text = "";
        EntPrice.Text = "";
        EntDays.Text = "";
        EdtServices.Text = "";
        PckStatus.SelectedIndex = 0;

        Modal.IsVisible = true;
    }

    // 🔹 EDIT
    private void OnEditClicked(object sender, EventArgs e)
    {
        var label = sender as Label;
        var id = (int)label.BindingContext.GetType().GetProperty("Id").GetValue(label.BindingContext);

        var item = _allPakiety.First(x => x.Id == id);

        _editingId = id;

        EntName.Text = item.Nazwa;
        EntPrice.Text = item.Cena.ToString();
        EntDays.Text = item.LiczbaDni.ToString();
        EdtServices.Text = item.Uslugi;
        PckStatus.SelectedItem = item.Status;

        Modal.IsVisible = true;
    }

    // 🔹 DELETE
    private void OnDeleteClicked(object sender, EventArgs e)
    {
        var label = sender as Label;
        var id = (int)label.BindingContext.GetType().GetProperty("Id").GetValue(label.BindingContext);

        var item = _allPakiety.First(x => x.Id == id);
        _allPakiety.Remove(item);

        RefreshList();
    }

    // 🔹 SAVE
    private void OnSave(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntName.Text))
            return;

        if (!decimal.TryParse(EntPrice.Text, out var cena))
            return;

        if (!int.TryParse(EntDays.Text, out var dni))
            return;

        var status = PckStatus.SelectedItem?.ToString() ?? "Aktywny";

        if (_editingId == null)
        {
            var newId = _allPakiety.Any() ? _allPakiety.Max(x => x.Id) + 1 : 1;

            _allPakiety.Add(new PakietModel
            {
                Id = newId,
                Nazwa = EntName.Text,
                Cena = cena,
                LiczbaDni = dni,
                Uslugi = EdtServices.Text,
                Status = status
            });
        }
        else
        {
            var item = _allPakiety.First(x => x.Id == _editingId);

            item.Nazwa = EntName.Text;
            item.Cena = cena;
            item.LiczbaDni = dni;
            item.Uslugi = EdtServices.Text;
            item.Status = status;
        }

        Modal.IsVisible = false;
        RefreshList();
    }

    // 🔹 CANCEL
    private void OnCancel(object sender, EventArgs e)
    {
        Modal.IsVisible = false;
    }
}

// 📦 MODEL
public class PakietModel
{
    public int Id { get; set; }
    public string Nazwa { get; set; }
    public decimal Cena { get; set; }
    public int LiczbaDni { get; set; }
    public string Uslugi { get; set; }
    public string Status { get; set; }

    public string CenaStr => $"{Cena:0.00} PLN";
    public string UslugiPreview =>
        string.IsNullOrEmpty(Uslugi) ? "" :
        (Uslugi.Length > 25 ? Uslugi.Substring(0, 25) + "..." : Uslugi);
}