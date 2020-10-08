Feature: SmokeTests
	In order to avoid silly mistakes
	Make sure the API responds with a 404 for no learner
	And a regular response when there is a learner


Scenario: No learner
	When we call the API with a learner that does not exist
	Then the result should be a 404

Scenario: Test Learner
	Given we have created a sample learner
	When we call the API with the sample learners details
	Then the result matches the sample learner