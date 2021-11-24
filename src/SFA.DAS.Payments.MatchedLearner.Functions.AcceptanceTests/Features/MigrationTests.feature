Feature: MigrationTests

Scenario: Provider migrated successfully
    Given a learner has Datalock events in PV2 Format
    When Migration is Run
    Then learner Datalock events are migrated into the new format