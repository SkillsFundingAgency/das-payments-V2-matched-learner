Feature: SmokeTests

Scenario: No learner
	When we call the API with a learner that does not exist
	Then the result should be a 404

Scenario: Test Learner
	Given we have created a sample learner
	When we call the API with the sample learners details
	Then the result matches the sample learner
	
Scenario: Test Learner performance Test
	Given we have created 5000 sample learners
	When we call the API 5000 times with the sample learners details
	Then the result should not be any exceptions

Scenario: Test Learner Training
	Given we have created a sample learner Training
	When we call the V2 API with the sample learners details
	Then the result matches the sample learner Training
