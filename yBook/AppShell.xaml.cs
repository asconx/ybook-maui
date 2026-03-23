using yBook.Views.Finanse;

namespace yBook
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // ── Rejestracja tras finansowych ──────────────────────────────────
            Routing.RegisterRoute("Dokumenty",        typeof(DokumentyPage));
            Routing.RegisterRoute("KontaFinansowe",   typeof(KontaFinansowePage));
            Routing.RegisterRoute("RejestrPlatnosci", typeof(RejestrPlatnosciPage));
            Routing.RegisterRoute("ImportMT940",      typeof(ImportMT940Page));
            Routing.RegisterRoute("UslugiOplaty",     typeof(yBook.Views.Ceny.UslugiOplaty));

            // ── Tu dodasz pozostałe trasy w kolejnych etapach ─────────────────
            // Routing.RegisterRoute("Recepcja",     typeof(RecepcjaPage));
            // Routing.RegisterRoute("Rezerwacje",   typeof(RezerwacjePage));
        }
    }
}
