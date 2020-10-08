﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchedLearnerApi.Configuration;
using Microsoft.AspNetCore.Hosting;
using SFA.DAS.Configuration.AzureTableStorage;

namespace MatchedLearnerApi.Extensions
{
    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureDasAppConfiguration(this IWebHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration(c =>
                c.AddAzureTableStorage(MatchedLearnerApiConfigurationKeys.MatchedLearnerApi));
        }
    }
}