Feature: SmokeTests

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

Scenario: Test Learner Training
	Given we have created a sample learner with 1 Training Records with 1 Price Episode across 1 academic Year 
	When we call the V2 API with the sample learners details
	Then the result should contain 1 Training with 1 price episode and 1 Periods

Scenario: Test Learner single Training across single academic Year with multiple price episodes
	Given we have created a sample learner with 1 Training Records with 2 Price Episode across 1 academic Year 
	When we call the V2 API with the sample learners details
	Then the result should contain 1 Training with 2 price episode and 1 Periods

Scenario: Test Learner single Training across multiple academic Year with single price episode
	Given we have created a sample learner with 1 Training Records with 1 Price Episode across 2 academic Year 
	When we call the V2 API with the sample learners details
	Then the result should contain 1 Training with 1 price episode and 2 Periods

Scenario: Test Learner single Training across multiple academic Year with multiple price episode for each academic year
	Given we have created a sample learner with 1 Training Records with 2 Price Episode across 2 academic Year 
	When we call the V2 API with the sample learners details
	Then the result should contain 1 Training with 4 price episode and 1 Periods

Scenario: Test Learner multiple Training across multiple academic Year 
	Given we have created a sample learner with 2 Training Records with 1 Price Episode across 2 academic Year 
	When we call the V2 API with the sample learners details
	Then the result should contain 2 Training with 1 price episode and 1 Periods

