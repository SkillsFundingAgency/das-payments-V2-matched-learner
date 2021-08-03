Feature: SmokeTests

Scenario: Function pulls the data from Payments DB and saves to Matched Learner DB
	Given A Submission Job Succeeded
	When A SubmissionJobSucceeded message is received
	Then the matched Learners are Imported