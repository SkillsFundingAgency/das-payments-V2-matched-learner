Scenario: No learner
	When we call the API with a learner that does not exist
	Then the result should be a 404

Scenario: Test Learner
	Given we have created a sample learner
	When we call the API with the sample learners details
	Then the result matches the sample learner