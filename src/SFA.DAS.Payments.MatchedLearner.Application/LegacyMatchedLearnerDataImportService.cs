using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Data;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface ILegacyMatchedLearnerDataImportService
    {
        Task Import(ImportMatchedLearnerData importMatchedLearnerData, List<DataLockEventModel> dataLockEvents);
    }

    public class LegacyMatchedLearnerDataImportService : ILegacyMatchedLearnerDataImportService
    {
        private readonly ILegacyMatchedLearnerRepository _legacyMatchedLearnerRepository;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly ILogger<LegacyMatchedLearnerDataImportService> _logger;

        public LegacyMatchedLearnerDataImportService(ILegacyMatchedLearnerRepository legacyMatchedLearnerRepository, IPaymentsRepository paymentsRepository, ILogger<LegacyMatchedLearnerDataImportService> logger)
        {
            _legacyMatchedLearnerRepository = legacyMatchedLearnerRepository ?? throw new ArgumentNullException(nameof(legacyMatchedLearnerRepository));
            _paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Import(ImportMatchedLearnerData importMatchedLearnerData, List<DataLockEventModel> dataLockEvents)
        {
            var collectionPeriods = new List<byte> { importMatchedLearnerData.CollectionPeriod };

            if (importMatchedLearnerData.CollectionPeriod != 1)
            {
                collectionPeriods.Add((byte)(importMatchedLearnerData.CollectionPeriod - 1));
            }

            try
            {
                await _legacyMatchedLearnerRepository.BeginTransactionAsync();

                await _legacyMatchedLearnerRepository.RemovePreviousSubmissionsData(importMatchedLearnerData.Ukprn, importMatchedLearnerData.AcademicYear, collectionPeriods);

                var apprenticeshipIds = dataLockEvents
                    .SelectMany(dle => dle.PayablePeriods)
                    .Select(dlepp => dlepp.ApprenticeshipId ?? 0)
                    .Union(dataLockEvents.SelectMany(dle => dle.NonPayablePeriods).SelectMany(dlenpp => dlenpp.Failures)
                        .Select(dlenppf => dlenppf.ApprenticeshipId ?? 0))
                    .ToList();

                var apprenticeships = await _paymentsRepository.GetApprenticeships(apprenticeshipIds);

                await _legacyMatchedLearnerRepository.RemoveApprenticeships(apprenticeshipIds);

                await _legacyMatchedLearnerRepository.StoreApprenticeships(apprenticeships);

                try
                {
                    await _legacyMatchedLearnerRepository.SaveDataLockEvents(dataLockEvents.Clone());
                }
                catch (Exception e)
                {
                    if (!e.IsUniqueKeyConstraintException() && !e.IsDeadLockException()) throw;

                    _logger.LogInformation("Batch contained a duplicate DataLock.  Will store each individually and discard duplicate.");

                    await _legacyMatchedLearnerRepository.SaveDataLocksIndividually(dataLockEvents);
                }

                await _legacyMatchedLearnerRepository.CommitTransactionAsync();
            }
            catch(Exception exception)
            {
               await _legacyMatchedLearnerRepository.RollbackTransactionAsync();

               _logger.LogError(exception,$"Error Importing Training Data. JobId: {importMatchedLearnerData.JobId}, AcademicYear: {importMatchedLearnerData.AcademicYear}, CollectionPeriod: {importMatchedLearnerData.CollectionPeriod} ");

               throw;
            }
        }
    }
}