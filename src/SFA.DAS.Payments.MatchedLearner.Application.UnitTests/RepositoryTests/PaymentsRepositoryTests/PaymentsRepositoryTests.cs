using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.Payments.MatchedLearner.Data.Contexts;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Events;

namespace SFA.DAS.Payments.MatchedLearner.Application.UnitTests.RepositoryTests.PaymentsRepositoryTests
{
    [TestFixture]
    public class PaymentsRepositoryTests
    {
        private string _connectionString = "Server=.;Database=SFA.DAS.Payments.Database;User Id=SFActor; Password=SFActor;";
        private PaymentsDataContext _context;
        private PaymentsRepository _sut;

        [Test]
        public async Task Test()
        {
            var submissionJobEvent = new SubmissionJobSucceeded
            {
                Ukprn = 97918,
                CollectionPeriod = 1,
                AcademicYear = 2122,
                JobId = 123
            };

            _context = new PaymentsDataContext(new DbContextOptionsBuilder()
                .UseSqlServer(_connectionString)
                .Options);

            _sut = new PaymentsRepository(_context);

            //await _sut.GetApprenticeships(new List<long> {1, 2});

            var result = await _sut.GetDataLockEvents(submissionJobEvent);
        }

    }
}
