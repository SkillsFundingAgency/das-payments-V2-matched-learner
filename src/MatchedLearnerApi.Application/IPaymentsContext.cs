using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace MatchedLearnerApi.Application
{
    public interface IPaymentsContext
    {
        DbSet<DatalockEvent> DatalockEvents { get; set; }
    }
}