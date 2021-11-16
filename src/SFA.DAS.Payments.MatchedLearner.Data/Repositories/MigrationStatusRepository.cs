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
    public interface IMigrationStatusRepository
    {
        Task BeginTransactionAsync(IsolationLevel isolationLevel);
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task CreateMigrationAttempt(MigrationRunAttemptModel model);
        Task<List<MigrationRunAttemptModel>> GetProviderMigrationAttempts(long ukprn);
        Task<MigrationRunAttemptModel> GetProviderMigrationStatusModel(Guid identifier); //todo remove
        Task UpdateStatus(Guid identifier, MigrationStatus newStatus); //todo remove
        Task UpdateStatus(long ukprn, MigrationStatus newStatus); //todo remove
        Task UpdateMigrationStatusModel(MigrationRunAttemptModel model); //todo remove
        Task UpdateMigrationRunAttempt(long ukprn, Guid migrationRunId, MigrationStatus status);
    }
    public class MigrationStatusRepository : IMigrationStatusRepository
    {
        private readonly MatchedLearnerDataContext _matchedLearnerDataContext;
        private IDbContextTransaction currentTransaction;

        public MigrationStatusRepository(MatchedLearnerDataContext matchedLearnerDataContext)
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
            _matchedLearnerDataContext.MigrationStatuses.Add(model);

            await _matchedLearnerDataContext.SaveChangesAsync();
        }

        public async Task<List<MigrationRunAttemptModel>> GetProviderMigrationAttempts(long ukprn)
        {
            return _matchedLearnerDataContext.MigrationStatuses
                .Where(x => x.Ukprn == ukprn).ToList();
        }

        public async Task<MigrationRunAttemptModel> GetProviderMigrationStatusModel(Guid identifier)
        {
            return await _matchedLearnerDataContext.MigrationStatuses
                .Where(x => x.Identifier == identifier)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateStatus(Guid identifier, MigrationStatus newStatus)
        {
            var model = await GetProviderMigrationStatusModel(identifier);
                
            model.Status = newStatus;

            await _matchedLearnerDataContext.SaveChangesAsync();
        }

        public async Task UpdateStatus(long ukprn, MigrationStatus newStatus)
        {
            var model = await GetProviderMigrationAttempts(ukprn);

            model.Status = newStatus;

            await _matchedLearnerDataContext.SaveChangesAsync();
        }

        public async Task UpdateMigrationStatusModel(MigrationRunAttemptModel model)
        {
            _matchedLearnerDataContext.MigrationStatuses.Update(model);

            await _matchedLearnerDataContext.SaveChangesAsync();
        }

        public Task UpdateMigrationRunAttempt(long ukprn, Guid migrationRunId, MigrationStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
