namespace yBook.Models
{
    // ─── Dokumenty ────────────────────────────────────────────────────────────

    public enum TypDokumentu { Faktura, Paragon, Korekta, Nota }

    public class Dokument
    {
        public string       Id               { get; init; } = "";
        public string       NumerRezerwacji  { get; init; } = "";
        public string       Numer            { get; init; } = "";
        public TypDokumentu Typ              { get; init; }
        public string       Klient           { get; init; } = "";
        public decimal      KwotaBrutto      { get; init; }
        public decimal      KwotaWplaty      { get; init; }
        public DateTime     DataWystawienia  { get; init; }
        public DateTime?    DataSprzedazy    { get; init; }
        public DateTime?    DataPlatnosci    { get; init; }

        public string TypLabel           => Typ switch
        {
            TypDokumentu.Faktura => "Faktura VAT",
            TypDokumentu.Paragon => "Paragon",
            TypDokumentu.Korekta => "Korekta",
            TypDokumentu.Nota    => "Nota",
            _                    => "Dokument"
        };

        public string KwotaBruttoStr     => $"{KwotaBrutto:N2} zł";
        public string KwotaWplatyStr     => KwotaWplaty > 0 ? $"{KwotaWplaty:N2} zł" : "—";
        public string DataWystawieniaStr => DataWystawienia.ToString("dd.MM.yyyy");
        public string DataSprzedazyStr   => DataSprzedazy?.ToString("dd.MM.yyyy") ?? "—";
        public string DataPlatnosciStr   => DataPlatnosci?.ToString("dd.MM.yyyy") ?? "—";
    }

    // ─── Konta finansowe ──────────────────────────────────────────────────────

    public enum TypKonta { Bankowe, Gotowkowe, Karta, Inne }

    public class KontoFinansowe
    {
        public string   Id      { get; init; } = "";
        public string   Nazwa   { get; init; } = "";
        public TypKonta Typ     { get; init; }
        public string   Numer   { get; init; } = "";
        public decimal  Saldo   { get; init; }
        public string   Waluta  { get; init; } = "PLN";
        public bool     Aktywne { get; init; } = true;

        public string TypEmoji   => Typ switch
        {
            TypKonta.Bankowe   => "🏦",
            TypKonta.Gotowkowe => "💵",
            TypKonta.Karta     => "💳",
            _                  => "💼"
        };
        public string SaldoStr   => $"{Saldo:N2} {Waluta}";
        public Color  SaldoColor => Saldo >= 0
            ? Color.FromArgb("#43A047")
            : Color.FromArgb("#E53935");
    }

    // ─── Rejestr płatności ────────────────────────────────────────────────────

    public enum TypPlatnosci { Przelew, Gotowka, Karta, Online, Zwrot }

    public class Platnosc
    {
        public string       Id              { get; init; } = "";
        public string       Tytul           { get; init; } = "";
        public TypPlatnosci Typ             { get; init; }
        public string       Klient          { get; init; } = "";
        public string       KontoNazwa      { get; init; } = "";
        public string       NumerRezerwacji { get; init; } = "";
        public DateTime     Data            { get; init; }
        public decimal      Kwota           { get; init; }
        public bool         Przychod        { get; init; }

        public string TypEmoji   => Typ switch
        {
            TypPlatnosci.Przelew => "🏦",
            TypPlatnosci.Gotowka => "💵",
            TypPlatnosci.Karta   => "💳",
            TypPlatnosci.Online  => "🌐",
            TypPlatnosci.Zwrot   => "↩️",
            _                    => "💰"
        };
        public string KwotaStr    => $"{(Przychod ? "+" : "-")}{Kwota:N2} zł";
        public string PrzychodStr => Przychod  ? $"{Kwota:N2} zł" : "—";
        public string RozchodStr  => !Przychod ? $"{Kwota:N2} zł" : "—";
        public Color  KwotaColor  => Przychod
            ? Color.FromArgb("#43A047")
            : Color.FromArgb("#E53935");
        public string DataStr     => Data.ToString("dd.MM.yyyy");
    }

    // ─── Import MT940 ─────────────────────────────────────────────────────────

    public enum StatusImportu { Oczekuje, Przetwarzanie, Zaimportowany, Blad }

    public class ImportMT940Record
    {
        public string        Id          { get; init; } = "";
        public string        NazwaPliku  { get; init; } = "";
        public DateTime      DataImportu { get; init; }
        public StatusImportu Status      { get; init; }
        public int           LiczbaOper  { get; init; }
        public int           Dopasowano  { get; init; }
        public decimal       Suma        { get; init; }
        public string?       Blad        { get; init; }

        public string StatusLabel    => Status.ToString();
        public Color  StatusColor    => Status switch
        {
            StatusImportu.Zaimportowany => Color.FromArgb("#43A047"),
            StatusImportu.Przetwarzanie => Color.FromArgb("#1565C0"),
            StatusImportu.Oczekuje      => Color.FromArgb("#F9A825"),
            StatusImportu.Blad          => Color.FromArgb("#E53935"),
            _                           => Colors.Gray
        };
        public string SumaStr        => $"{Suma:N2} zł";
        public string DataStr        => DataImportu.ToString("dd.MM.yyyy HH:mm");
        public string DopasowanieStr => $"{Dopasowano}/{LiczbaOper}";
    }

    // ─── Mock data ────────────────────────────────────────────────────────────

    public static class MockFinanse
    {
        public static List<Dokument> Dokumenty() => new()
        {
            new() { Id="1", NumerRezerwacji="#1042", Numer="FV/2024/001",  Typ=TypDokumentu.Faktura, Klient="Jan Kowalski",               KwotaBrutto=480.00m,   KwotaWplaty=480.00m,  DataWystawienia=new(2024,3,1),  DataSprzedazy=new(2024,3,1),  DataPlatnosci=new(2024,3,3)  },
            new() { Id="2", NumerRezerwacji="#1043", Numer="FV/2024/002",  Typ=TypDokumentu.Faktura, Klient="Anna Nowak",                  KwotaBrutto=1050.00m,  KwotaWplaty=0m,       DataWystawienia=new(2024,3,5),  DataSprzedazy=new(2024,3,5),  DataPlatnosci=null           },
            new() { Id="3", NumerRezerwacji="#1044", Numer="PAR/2024/014", Typ=TypDokumentu.Paragon, Klient="Tomasz Wiśniewski",           KwotaBrutto=320.00m,   KwotaWplaty=320.00m,  DataWystawienia=new(2024,3,8),  DataSprzedazy=new(2024,3,8),  DataPlatnosci=new(2024,3,8)  },
            new() { Id="4", NumerRezerwacji="#1041", Numer="KOR/2024/003", Typ=TypDokumentu.Korekta, Klient="Maria Zielińska",             KwotaBrutto=-150.00m,  KwotaWplaty=0m,       DataWystawienia=new(2024,3,10), DataSprzedazy=null,           DataPlatnosci=null           },
            new() { Id="5", NumerRezerwacji="#1046", Numer="FV/2024/003",  Typ=TypDokumentu.Faktura, Klient="Hotel Konferencje Sp. z o.o.",KwotaBrutto=3200.00m,  KwotaWplaty=3200.00m, DataWystawienia=new(2024,3,15), DataSprzedazy=new(2024,3,15), DataPlatnosci=new(2024,3,17) },
            new() { Id="6", NumerRezerwacji="#1047", Numer="PAR/2024/015", Typ=TypDokumentu.Paragon, Klient="Karolina Marek",              KwotaBrutto=560.00m,   KwotaWplaty=560.00m,  DataWystawienia=new(2024,3,17), DataSprzedazy=new(2024,3,17), DataPlatnosci=new(2024,3,17) },
            new() { Id="7", NumerRezerwacji="#1048", Numer="NOTA/2024/001",Typ=TypDokumentu.Nota,    Klient="BNP Leasing S.A.",            KwotaBrutto=200.00m,   KwotaWplaty=0m,       DataWystawienia=new(2024,3,18), DataSprzedazy=null,           DataPlatnosci=null           },
        };

        public static List<KontoFinansowe> Konta() => new()
        {
            new() { Id="1", Nazwa="Konto główne PKO",    Typ=TypKonta.Bankowe,   Numer="PL 12 1020 0000 0000 1234 5678 9012", Saldo=48_320.50m },
            new() { Id="2", Nazwa="Kasa gotówkowa",      Typ=TypKonta.Gotowkowe, Numer="—",                                   Saldo=2_450.00m  },
            new() { Id="3", Nazwa="Karta firmowa mBank", Typ=TypKonta.Karta,     Numer="**** **** **** 4521",                 Saldo=-1_230.80m },
            new() { Id="4", Nazwa="Konto VAT",           Typ=TypKonta.Bankowe,   Numer="PL 12 1020 0000 0000 9876 5432 1000", Saldo=6_100.00m  },
        };

        public static List<Platnosc> Platnosci() => new()
        {
            new() { Id="1", Tytul="Rezerwacja #1042",     NumerRezerwacji="#1042", Typ=TypPlatnosci.Przelew, Klient="Jan Kowalski",      KontoNazwa="Konto główne PKO",   Data=new(2024,3,1),  Kwota=480.00m,  Przychod=true  },
            new() { Id="2", Tytul="Rezerwacja #1043",     NumerRezerwacji="#1043", Typ=TypPlatnosci.Karta,   Klient="Anna Nowak",        KontoNazwa="Karta firmowa mBank", Data=new(2024,3,5),  Kwota=1050.00m, Przychod=true  },
            new() { Id="3", Tytul="Zakup środków czyst.", NumerRezerwacji="—",     Typ=TypPlatnosci.Gotowka, Klient="Sklep Max",         KontoNazwa="Kasa gotówkowa",      Data=new(2024,3,6),  Kwota=145.00m,  Przychod=false },
            new() { Id="4", Tytul="Rezerwacja #1044",     NumerRezerwacji="#1044", Typ=TypPlatnosci.Online,  Klient="Tomasz Wiśniewski", KontoNazwa="Konto główne PKO",    Data=new(2024,3,8),  Kwota=320.00m,  Przychod=true  },
            new() { Id="5", Tytul="Zwrot rezerwacji",     NumerRezerwacji="#1041", Typ=TypPlatnosci.Zwrot,   Klient="Maria Zielińska",   KontoNazwa="Konto główne PKO",    Data=new(2024,3,10), Kwota=150.00m,  Przychod=false },
            new() { Id="6", Tytul="Rezerwacja #1046",     NumerRezerwacji="#1046", Typ=TypPlatnosci.Przelew, Klient="Hotel Konferencje", KontoNazwa="Konto główne PKO",    Data=new(2024,3,15), Kwota=3200.00m, Przychod=true  },
            new() { Id="7", Tytul="Opłata za prąd",       NumerRezerwacji="—",     Typ=TypPlatnosci.Przelew, Klient="PGE S.A.",          KontoNazwa="Konto główne PKO",    Data=new(2024,3,16), Kwota=890.00m,  Przychod=false },
            new() { Id="8", Tytul="Rezerwacja #1047",     NumerRezerwacji="#1047", Typ=TypPlatnosci.Gotowka, Klient="Karolina Marek",    KontoNazwa="Kasa gotówkowa",      Data=new(2024,3,17), Kwota=560.00m,  Przychod=true  },
        };

        public static List<ImportMT940Record> ImportyMT940() => new()
        {
            new() { Id="1", NazwaPliku="wyciag_PKO_2024_02.mt940",  DataImportu=new(2024,3,1,9,14,0),  Status=StatusImportu.Zaimportowany, LiczbaOper=24, Dopasowano=23, Suma=18_420.50m },
            new() { Id="2", NazwaPliku="wyciag_PKO_2024_03a.mt940", DataImportu=new(2024,3,15,14,3,0), Status=StatusImportu.Zaimportowany, LiczbaOper=11, Dopasowano=11, Suma=7_230.00m  },
            new() { Id="3", NazwaPliku="wyciag_mBank_2024_03.mt940",DataImportu=new(2024,3,18,10,0,0), Status=StatusImportu.Blad,          LiczbaOper=0,  Dopasowano=0,  Suma=0m,        Blad="Nieprawidłowy format pliku MT940." },
            new() { Id="4", NazwaPliku="wyciag_PKO_2024_03b.mt940", DataImportu=new(2024,3,19,8,55,0), Status=StatusImportu.Oczekuje,      LiczbaOper=8,  Dopasowano=0,  Suma=4_100.00m  },
        };
    }

    // ── DTO – modele odpowiedzi API (internal, dostępne w całym projekcie) ────

    internal class ApiPlatnosc
    {
        public object?   Id                { get; set; }
        public string?   Title             { get; set; }
        public string?   Description       { get; set; }
        public string?   Type              { get; set; }
        public string?   Direction         { get; set; }
        public string?   Client            { get; set; }
        public string?   ClientName        { get; set; }
        public string?   AccountName       { get; set; }
        public string?   ReservationNo     { get; set; }
        public string?   ReservationNumber { get; set; }
        public DateTime? Date              { get; set; }
        public DateTime? CreatedAt         { get; set; }
        public decimal?  Amount            { get; set; }
        public decimal?  Value             { get; set; }
        public bool?     Income            { get; set; }
    }

    internal class ApiDokument
    {
        public object?   Id                { get; set; }
        public string?   ReservationNo     { get; set; }
        public string?   ReservationNumber { get; set; }
        public string?   Number            { get; set; }
        public string?   DocumentNumber    { get; set; }
        public string?   Type              { get; set; }
        public string?   Client            { get; set; }
        public string?   ClientName        { get; set; }
        public decimal?  GrossAmount       { get; set; }
        public decimal?  Amount            { get; set; }
        public decimal?  PaidAmount        { get; set; }
        public DateTime? IssueDate         { get; set; }
        public DateTime? CreatedAt         { get; set; }
        public DateTime? SaleDate          { get; set; }
        public DateTime? PaymentDate       { get; set; }
    }

    internal class ApiKonto
    {
        public object?  Id             { get; set; }
        public string?  Name           { get; set; }
        public string?  Type           { get; set; }
        public string?  Number         { get; set; }
        public string?  AccountNumber  { get; set; }
        public decimal? Balance        { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string?  Currency       { get; set; }
        public bool?    Active         { get; set; }
    }

}
