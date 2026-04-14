using yBook.Models;

namespace yBook.Services
{
    public interface IFinanseService
    {
        Task<List<Platnosc>>        GetPlatnosciAsync(DateTime? od = null, DateTime? do_ = null, string? kontoId = null);
        Task<List<Dokument>>        GetDokumentyAsync(DateTime? od = null, DateTime? do_ = null, string? typ = null, string? klient = null);
        Task<List<KontoFinansowe>>  GetKontaAsync();
        Task<KontoFinansowe>        CreateKontoAsync(KontoFinansowe konto);
        Task<bool>                  DeleteKontoAsync(string id);
        Task<Platnosc>              CreatePlatnoscAsync(Platnosc platnosc);
        Task<Dokument>              CreateDokumentAsync(Dokument dokument);
        Task<bool>                  ImportMT940Async(Stream fileStream, string fileName);
    }
}
