[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/das-payments-V2-matched-learner?repoName=SkillsFundingAgency%2Fdas-payments-V2-matched-learnerk&branchName=master)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2292&repoName=SkillsFundingAgency%2Fdas-payments-V2-matched-learner&branchName=master)

# Payments V2 Matched Learner API

This repository contains an API used to obtain data lock information about a learner, matching a given UKPRN and ULN. This information is used to make decisions about the status of a learner, for example whether to authorise payments to their employer.

Licensed under the [MIT license](LICENSE)

### Requirements

In order to run this solution locally you will need the following:

* [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
* [.NET SDK >= 6](https://www.microsoft.com/net/download/)
* [SQL Server](https://www.microsoft.com/en-gb/sql-server/sql-server-downloads)
* [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (previously known as Azure Storage Emulator)

### Environment Setup

Publish the database locally using the database project `SFA.DAS.MatchedLearner.Database`

Create a file called `local.settings.json` in the following projects:

- `SFA.DAS.Payments.MatchedLearner.Api`
- `SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure`

It should contain the following content in both files:

```
{
  "AzureAd": {
    "ApiBaseUrl": "https://localhost:5001",
    "ClientId": "",
    "ClientSecret": "",
    "IdentifierUri": "https://citizenazuresfabisgov.onmicrosoft.com/das-at-mlrnapi-as-ar",
    "Tenant": "citizenazuresfabisgov.onmicrosoft.com"
  },

  "MatchedLearner": {
    "TimeToWait": "00:02:00",
    "TimeToPause": "00:00:02",
    "TimeToWaitUnexpected": "00:00:30",
    
    "MatchedLearnerStorageAccountConnectionString": "UseDevelopmentStorage=true",

    "PaymentsServiceBusConnectionString": "<INSERT SERVICE BUS CONNECTION STRING FOR YOUR OWN PV2 DCOL-DAS-LAB INSTANCE>",

    "PaymentsConnectionString": "<INSERT LOCAL CONNECTION STRING FOR PAYMENTS V2 DATABASE>",
    "MatchedLearnerConnectionString": "<INSERT LOCAL CONNECTION STRING FOR MATCHED LEARNER DATABASE>",
    "ConnectionNeedsAccessToken": "False",
    "MatchedLearnerImportQueue": "sfa-das-payments-matchedlearner-import",
    "MatchedLearnerQueue": "sfa-das-payments-matchedlearner"
  }
}
```

If they do not already exist, create the queues `sfa-das-payments-matchedlearner-import` and `sfa-das-payments-matchedlearner` in your service bus instance in the `DCOL-DAS-LAB` resource group.

Running

- Start Azurite (or Azure Storage Emulator)
- Set `SFA.DAS.MatchedLearner.Api` as the startup project