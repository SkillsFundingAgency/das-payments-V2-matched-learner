﻿namespace SFA.DAS.Payments.MatchedLearner.Api.Configuration
{
    public class MatchedLearnerApiConfigurationKeys
    {
        public const string MatchedLearnerApiKey = "SFA.DAS.MatchedLearnerApi";

        public static readonly string MatchedLearnerConfigKey = "MatchedLearner";
        public static readonly string AzureADConfigKey = "AzureAd";

        //NOTE: default config.AddAzureTableStorage() uses PreFixConfigurationKeys = true which means all the configurations are prefixed by $"{MatchedLearnerApiKey}:<key>"
        //public static readonly string MatchedLearnerConfigKey = $"{MatchedLearnerApiKey}:MatchedLearner";
        //public static readonly string AzureADConfigKey = $"{MatchedLearnerApiKey}:AzureAd";
    }
}
