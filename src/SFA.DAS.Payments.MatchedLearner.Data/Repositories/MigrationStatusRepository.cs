using System;
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
        Task CreateMigrationStatusModel(MigrationStatusModel model);
        Task<MigrationStatusModel> GetProviderMigrationStatusModel(long ukprn);
        Task<MigrationStatusModel> GetProviderMigrationStatusModel(Guid identifier);
        Task UpdateStatus(Guid identifier, MigrationStatus newStatus);
        Task UpdateStatus(long ukprn, MigrationStatus newStatus);
        Task UpdateMigrationStatusModel(MigrationStatusModel model);
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

        public async Task CreateMigrationStatusModel(MigrationStatusModel model)
        {
            _matchedLearnerDataContext.MigrationStatuses.Add(model);

            await _matchedLearnerDataContext.SaveChangesAsync();
        }

        public async Task<MigrationStatusModel> GetProviderMigrationStatusModel(long ukprn)
        {
            return await _matchedLearnerDataContext.MigrationStatuses
                .Where(x => x.Ukprn == ukprn)
                .FirstOrDefaultAsync();
        }

        public async Task<MigrationStatusModel> GetProviderMigrationStatusModel(Guid identifier)
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
            var model = await GetProviderMigrationStatusModel(ukprn);

            model.Status = newStatus;

            await _matchedLearnerDataContext.SaveChangesAsync();
        }

        public async Task UpdateMigrationStatusModel(MigrationStatusModel model)
        {
            _matchedLearnerDataContext.MigrationStatuses.Update(model);

            await _matchedLearnerDataContext.SaveChangesAsync();
        }
    }
}
