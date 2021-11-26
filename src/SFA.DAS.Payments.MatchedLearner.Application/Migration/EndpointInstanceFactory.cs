using System;
using System.Threading.Tasks;
using NServiceBus;

namespace SFA.DAS.Payments.MatchedLearner.Application.Migration
{
    public interface IEndpointInstanceFactory
    {
        Task<IEndpointInstance> GetEndpointInstance();
    }

    public class EndpointInstanceFactory : IEndpointInstanceFactory
    {
        private readonly EndpointConfiguration _endpointConfiguration;
        private static IEndpointInstance _endpointInstance;

        public EndpointInstanceFactory(EndpointConfiguration endpointConfiguration)
        {
            this._endpointConfiguration = endpointConfiguration ?? throw new ArgumentNullException(nameof(endpointConfiguration));
        }

        public async Task<IEndpointInstance> GetEndpointInstance()
        {
            if (_endpointInstance != null)
                return _endpointInstance;

            _endpointInstance = await Endpoint.Start(_endpointConfiguration);
            
            return _endpointInstance;
        }
    }
}
