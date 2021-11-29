using System.Threading.Tasks;
using NServiceBus;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public interface IProviderLevelMigrationRequestSendWrapper
    {
        Task Send(ProviderLevelMigrationRequest request);
    }

    public class ProviderLevelMigrationRequestSendWrapper : IProviderLevelMigrationRequestSendWrapper
    {
        private readonly IEndpointInstanceFactory _endpointInstanceFactory;
        private readonly string _providerLevelMatchedLearnerMigration;
        private IEndpointInstance _endpointInstance;

        public ProviderLevelMigrationRequestSendWrapper(IEndpointInstanceFactory endpointInstanceFactory,
            string providerLevelMatchedLearnerMigration)
        {
            _endpointInstanceFactory = endpointInstanceFactory;
            _providerLevelMatchedLearnerMigration = providerLevelMatchedLearnerMigration;
        }

        private async Task<IEndpointInstance> GetEndpointInstance()
        {
            return _endpointInstance ??= await _endpointInstanceFactory.GetEndpointInstance();
        }

        public async Task Send(ProviderLevelMigrationRequest request)
        {
            var options = new SendOptions();
            options.SetDestination(_providerLevelMatchedLearnerMigration);
            var instance = await GetEndpointInstance();
            await instance.SendLocal(request);
        }
    }
}