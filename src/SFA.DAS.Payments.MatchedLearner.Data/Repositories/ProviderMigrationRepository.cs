using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Repositories
{
    public interface IProviderMigrationRepository
    {
        Task BeginTransactionAsync(IsolationLevel isolationLevel);
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task CreateMigrationAttempt(MigrationRunAttemptModel model);
        Task<List<MigrationRunAttemptModel>> GetProviderMigrationAttempts(long ukprn);
        Task UpdateMigrationRunAttemptStatus(long ukprn, Guid migrationRunId, MigrationStatus status);
    }
    public class ProviderMigrationRepository : IProviderMigrationRepository
    {
        private readonly MatchedLearnerDataContext _matchedLearnerDataContext;
        private IDbContextTransaction currentTransaction;

        public ProviderMigrationRepository(MatchedLearnerDataContext matchedLearnerDataContext)
        {
            _matchedLearnerDataContext = matchedLearnerDataContext;
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted)
        {
            currentTransaction = await _matchedLearnerDataContext.Database.BeginTransactionAsync(isolationLevel);
        }

        public async Task CommitTransactionAsync()
        {
            await currentTransaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await currentTransaction.RollbackAsync();
        }

        public async Task CreateMigrationAttempt(MigrationRunAttemptModel model)
        {
            _matchedLearnerDataContext.MigrationRunAttempts.Add(model);

            await _matchedLearnerDataContext.SaveChangesAsync();
        }

        public async Task<List<MigrationRunAttemptModel>> GetProviderMigrationAttempts(long ukprn)
        {
            return await _matchedLearnerDataContext.MigrationRunAttempts
                .Where(x => x.Ukprn == ukprn)
                .ToListAsync();
        }

        public async Task UpdateMigrationRunAttemptStatus(long ukprn, Guid migrationRunId, MigrationStatus status)
        {
            var model = await _matchedLearnerDataContext.MigrationRunAttempts
                .SingleAsync(x =>
                    x.Ukprn == ukprn &&
                    x.MigrationRunId == migrationRunId);

            model.Status = status;

            if(status == MigrationStatus.Completed)
                model.CompletionTime = DateTime.Now;

            await _matchedLearnerDataContext.SaveChangesAsync();
        }
    }
}
