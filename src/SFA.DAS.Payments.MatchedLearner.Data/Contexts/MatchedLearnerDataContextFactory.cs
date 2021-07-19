using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.Payments.MatchedLearner.Data.Contexts
{
    public interface IMatchedLearnerDataContextFactory
    {
        IMatchedLearnerDataContext Create(DbTransaction transaction = null);
    }

    public class MatchedLearnerDataContextFactory : IMatchedLearnerDataContextFactory
    {
        private readonly DbContextOptions _options;

        public MatchedLearnerDataContextFactory(DbContextOptions optionsBuilder)
        {
            _options = optionsBuilder;
        }

        public IMatchedLearnerDataContext Create(DbTransaction transaction = null)
        {
            var context = new MatchedLearnerDataContext(_options);
            if (transaction != null)
                context.Database.UseTransaction(transaction);
            return context;
        }
    }
}