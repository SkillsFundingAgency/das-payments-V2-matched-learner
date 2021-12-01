@Ignore_on_Build_server
Feature: Legacy Api SmokeTests

Scenario: Legacy - No learner
	When we call the API with a learner that does not exist in Legacy Schema
	Then the result should be a 404

Scenario: Legacy - Test Learner
	Given we have created 1 sample learners in Legacy Schema
	When we call the API with the sample learners details in Legacy Schema
	Then the result matches the sample learner
	
Scenario: Legacy - Test Learner performance Test
	Given we have created 5000 sample learners in Legacy Schema
	When we call the API 5000 times with the sample learners details in Legacy Schema
	Then the result should not be any exceptions