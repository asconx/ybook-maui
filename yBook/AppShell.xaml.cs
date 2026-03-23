using yBook.Views.Finanse;
using yBook.Views.ICalendar;

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
            Routing.RegisterRoute("ICalendar",        typeof(ICalendarPage));

            // ── Tu dodasz pozostałe trasy w kolejnych etapach ─────────────────
            // Routing.RegisterRoute("Recepcja",     typeof(RecepcjaPage));
            // Routing.RegisterRoute("Rezerwacje",   typeof(RezerwacjePage));
        }
    }
}
