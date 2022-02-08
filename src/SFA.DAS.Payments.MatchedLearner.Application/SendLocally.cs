using NServiceBus;

namespace SFA.DAS.Payments.MatchedLearner.Functions.Migration
{
    public static class SendLocally
    {
        public static SendOptions Options
        {
            get
            {
                var options = new SendOptions();
                options.RouteToThisEndpoint();
                return options;
            }
        }
    }
}