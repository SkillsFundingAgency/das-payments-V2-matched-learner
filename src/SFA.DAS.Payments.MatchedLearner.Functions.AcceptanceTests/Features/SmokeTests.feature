Feature: SmokeTests

Scenario: Test Learner
	Given A successful submission is completed
	When we receive Submission Succeeded Event 
	Then the matched Learners are Imported