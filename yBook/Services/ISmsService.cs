using System.Collections.Generic;
using System.Threading.Tasks;

namespace yBook.Services
{
    public class StatusDto
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
    }

    public interface ISmsService
    {
        Task<List<StatusDto>> GetStatusesAsync();
        Task<int> GetActiveCountAsync(IEnumerable<int> statusIds, bool checkedIn, bool notCheckedIn);
        Task<bool> SendToActiveAsync(string message, IEnumerable<int> statusIds, bool checkedIn, bool notCheckedIn);
    }
}
