using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace SFA.DAS.Payments.MatchedLearner.Data.Contexts
{
    public interface IMatchedLearnerDataContextFactory
    {
        IMatchedLearnerContext Create(DbTransaction transaction = null);
    }

    public class MatchedLearnerDataContextFactory : IMatchedLearnerDataContextFactory
    {
        private readonly DbContextOptions _options;

        public MatchedLearnerDataContextFactory(DbContextOptionsBuilder optionsBuilder)
        {
            _options = optionsBuilder.Options;
        }

        public IMatchedLearnerContext Create(DbTransaction transaction = null)
        {
            var context = new MatchedLearnerContext(_options);
            if (transaction != null)
                context.Database.UseTransaction(transaction);
            return context;
        }
    }
}