using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Repositories
{
    public interface IMigrationStatusRepository
    {
        Task<MigrationStatusModel> GetMigrationStatus(long ukprn);
        Task<Guid> CreateNewMigrationStatus(long ukprn);
        Task UpdateMigrationStatus(Guid migrationRunId, MigrationStatus status);
    }
    public class MigrationStatusRepository : IMigrationStatusRepository
    {
        public Task<MigrationStatusModel> GetMigrationStatus(long ukprn)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> CreateNewMigrationStatus(long ukprn)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMigrationStatus(Guid migrationRunId, MigrationStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
