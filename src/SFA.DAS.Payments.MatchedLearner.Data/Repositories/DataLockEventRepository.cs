using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.MatchedLearner.Data.Repositories
{
    public interface IDataLockEventRepository
    {
        Task<List<DataLockEventModel>> GetDataLockEvents(long ukprn, short academicYear, byte collectionPeriod);
    }

    public class DataLockEventRepository : IDataLockEventRepository
    {
        private readonly IDataLockEventDataContext _dataContext;
        private readonly ILogger<DataLockEventRepository> _logger;

        public DataLockEventRepository(IDataLockEventDataContext dataContext, ILogger<DataLockEventRepository> logger)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<DataLockEventModel>> GetDataLockEvents(long ukprn, short academicYear, byte collectionPeriod)
        {
            return await _dataContext.DataLockEvent
                .Include(d => d.NonPayablePeriods)
                .ThenInclude(npp => npp.Failures)
                .Include(d => d.PayablePeriods)
                .Include(d => d.PriceEpisodes)
                .Where(d =>
                    d.Ukprn == ukprn &&
                    d.AcademicYear == academicYear &&
                    d.CollectionPeriod == collectionPeriod)
                .ToListAsync();
        }
    }
}