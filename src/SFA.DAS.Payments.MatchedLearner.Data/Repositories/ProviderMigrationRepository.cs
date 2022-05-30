using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Repositories
{
    public interface IProviderMigrationRepository
    {
        Task CreateMigrationAttempt(MigrationRunAttemptModel model);
        Task<List<MigrationRunAttemptModel>> GetProviderMigrationAttempts(long ukprn);
        Task UpdateMigrationRunAttemptStatus(MigrationRunAttemptModel migrationRun, MigrationStatus status);
    }
    public class ProviderMigrationRepository : IProviderMigrationRepository
    {
        private readonly MatchedLearnerDataContext _matchedLearnerDataContext;

        public ProviderMigrationRepository(MatchedLearnerDataContext matchedLearnerDataContext)
        {
            _matchedLearnerDataContext = matchedLearnerDataContext;
        }

        public async Task CreateMigrationAttempt(MigrationRunAttemptModel model)
        {
            await _matchedLearnerDataContext.MigrationRunAttempts.AddAsync(model);

            await _matchedLearnerDataContext.SaveChangesAsync();
        }

        public async Task<List<MigrationRunAttemptModel>> GetProviderMigrationAttempts(long ukprn)
        {
            return await _matchedLearnerDataContext.MigrationRunAttempts
                .Where(x => x.Ukprn == ukprn)
                .ToListAsync();
        }

        public async Task UpdateMigrationRunAttemptStatus(MigrationRunAttemptModel migrationRun, MigrationStatus status)
        {
            migrationRun.Status = status;

            if(status == MigrationStatus.Completed)
                migrationRun.CompletionTime = DateTime.Now;

            _matchedLearnerDataContext.Update(migrationRun);

            await _matchedLearnerDataContext.SaveChangesAsync();
        }
    }
}
