using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml.Linq;
using yBook.Models;

namespace yBook.ViewModels;

public class StatusyViewModel : INotifyPropertyChanged
{
    // ── ObservableCollection ──────────────────────────────────────────────

    public ObservableCollection<Status> Statusy { get; } = new();

    public int LiczbaStatusow => Statusy.Count;

    // ── Pola formularza ───────────────────────────────────────────────────

    private string _nowaNazwa = string.Empty;
    public string NowaNazwa
    {
        get => _nowaNazwa;
        set { _nowaNazwa = value; Notify(); }
    }

    private string _nowyKolor = "#3498db";
    public string NowyKolor
    {
        get => _nowyKolor;
        set { _nowyKolor = value; Notify(); }
    }

    private string _nowyOpis = string.Empty;
    public string NowyOpis
    {
        get => _nowyOpis;
        set { _nowyOpis = value; Notify(); }
    }

    // ── Stan formularza ───────────────────────────────────────────────────

    private bool _formularzWidoczny;
    public bool FormularzWidoczny
    {
        get => _formularzWidoczny;
        set { _formularzWidoczny = value; Notify(); Notify(nameof(PrzyciskDodajWidoczny)); }
    }

    public bool PrzyciskDodajWidoczny => !FormularzWidoczny;

    private bool _czyEdycja;
    public string TytulFormularza => _czyEdycja ? "Edytuj status" : "Nowy status";

    // ── Komendy ───────────────────────────────────────────────────────────

    public ICommand PokazFormularzCommand { get; }
    public ICommand ZapiszCommand { get; }
    public ICommand AnulujCommand { get; }
    public ICommand EdytujCommand { get; }
    public ICommand UsunCommand { get; }

    // ── Prywatne ──────────────────────────────────────────────────────────

    private Status? _edytowany;
    private int _nextId = 1;

    // ── Konstruktor ───────────────────────────────────────────────────────

    public StatusyViewModel()
    {
        PokazFormularzCommand = new Command(PokazFormularz);
        ZapiszCommand = new Command(Zapisz);
        AnulujCommand = new Command(Anuluj);
        EdytujCommand = new Command<Status>(Edytuj);
        UsunCommand = new Command<Status>(Usun);

        // Odśwież LiczbaStatusow przy każdej zmianie kolekcji
        Statusy.CollectionChanged += (_, _) => Notify(nameof(LiczbaStatusow));

        // Dane przykładowe
        Statusy.Add(new Status { Id = _nextId++, Nazwa = "Aktywny", Kolor = "#2ecc71", Opis = "Widoczny dla klientów" });
        Statusy.Add(new Status { Id = _nextId++, Nazwa = "Nieaktywny", Kolor = "#e74c3c", Opis = "Ukryty" });
    }

    // ── Logika ────────────────────────────────────────────────────────────

    private void PokazFormularz()
    {
        WyczyscFormularz();
        _czyEdycja = false;
        Notify(nameof(TytulFormularza));
        FormularzWidoczny = true;
    }

    private void Zapisz()
    {
        if (string.IsNullOrWhiteSpace(NowaNazwa)) return;

        if (_czyEdycja && _edytowany is not null)
        {
            // Podmień obiekt w kolekcji → CollectionView się odświeży
            int idx = Statusy.IndexOf(_edytowany);
            if (idx >= 0)
                Statusy[idx] = new Status
                {
                    Id = _edytowany.Id,
                    Nazwa = NowaNazwa,
                    Kolor = NowyKolor,
                    Opis = NowyOpis,
                    Powiadomienia = _edytowany.Powiadomienia
                };
        }
        else
        {
            Statusy.Add(new Status
            {
                Id = _nextId++,
                Nazwa = NowaNazwa,
                Kolor = NowyKolor,
                Opis = NowyOpis
            });
        }

        FormularzWidoczny = false;
        WyczyscFormularz();
    }

    private void Anuluj()
    {
        FormularzWidoczny = false;
        WyczyscFormularz();
    }

    private void Edytuj(Status? s)
    {
        if (s is null) return;

        _edytowany = s;
        NowaNazwa = s.Nazwa;
        NowyKolor = s.Kolor;
        NowyOpis = s.Opis;
        _czyEdycja = true;
        Notify(nameof(TytulFormularza));
        FormularzWidoczny = true;
    }

    private void Usun(Status? s)
    {
        if (s is not null) Statusy.Remove(s);
    }

    private void WyczyscFormularz()
    {
        NowaNazwa = string.Empty;
        NowyKolor = "#3498db";
        NowyOpis = string.Empty;
        _edytowany = null;
    }

    // ── INotifyPropertyChanged ────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}