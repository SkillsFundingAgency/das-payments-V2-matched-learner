Feature: New Matched Learner Api SmokeTests

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

