using System.ComponentModel;

public class Pakiet : INotifyPropertyChanged
{
    private bool _isActive;
    private bool _isSelected;
    private string _nazwa = "";

    public int Id { get; set; }

    public string Nazwa
    {
        get => _nazwa;
        set { if (_nazwa != value) { _nazwa = value; OnPropertyChanged(nameof(Nazwa)); } }
    }

    public decimal Cena { get; set; }
    public string? ZdjecieUrl { get; set; }
    public string? DataOd { get; set; }
    public string? DataDo { get; set; }
    public string? Opis { get; set; }

    public bool IsActive
    {
        get => _isActive;
        set { if (_isActive != value) { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
    }

    public string CenaStr => Cena > 0 ? $"{Cena:N2} z³" : "—";

    public string ZakresStr
    {
        get
        {
            if (!string.IsNullOrEmpty(DataOd) && !string.IsNullOrEmpty(DataDo)) return $"{DataOd} – {DataDo}";
            if (!string.IsNullOrEmpty(DataOd)) return $"od {DataOd}";
            return "—";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}