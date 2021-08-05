Feature: SmokeTests

Scenario: Function pulls the data from Payments DB and saves to Matched Learner DB
	Given A Submission Job Succeeded
	When A SubmissionJobSucceeded message is received
	Then the matched Learners are Imported

Scenario: Function pulls the data from Payments DB and saves to matched learner DB 2122
	Given A Submission Job Succeeded for CollectionPeriod 1 and AcademicYear 2122
	#And There is No Existing Data For CollectionPeriod 1 and AcademicYear 2122
	When A SubmissionJobSucceeded message is received for CollectionPeriod 1 and AcademicYear 2122
	Then the matched Learners are only Imported for CollectionPeriod 1 and AcademicYear 2122
#
Scenario: Existing data for current collection data is deleted before saving new Data to matched learner DB
	Given A Submission Job Succeeded for CollectionPeriod 1 and AcademicYear 2122
	And there is existing data For CollectionPeriod 1 and AcademicYear 2122
	When A SubmissionJobSucceeded message is received for CollectionPeriod 1 and AcademicYear 2122
	Then the existing matched Learners are deleted
	And the matched Learners are only Imported for CollectionPeriod 1 and AcademicYear 2122
#
#Scenario: Existing Date for previous collection then data is deleted before saving new Data to matched learner DB
#	Given A Submission Job Succeeded for CollectionPeriod 1 and AcademicYear 2122
#	And There is Existing Data For CollectionPeriod 14 and AcademicYear 2021
#	When A SubmissionJobSucceeded message is received
#	Then The Existing matched Learners for CollectionPeriod 14 and AcademicYear 2021 is NOT deleted 
#	And the matched Learners are only Imported for CollectionPeriod 1 and AcademicYear 2122
#	
#Scenario: Existing Date for previous Academic year latest collection then only new data is saved to matched learner DB
#	Given A Submission Job Succeeded for CollectionPeriod 1 and AcademicYear 2122
#	And There is Existing Data For CollectionPeriod 14 and AcademicYear 2021
#	When A SubmissionJobSucceeded message is received
#	Then The Existing matched Learners for CollectionPeriod 14 and AcademicYear 2021 is NOT deleted 
#	And the matched Learners are only Imported for CollectionPeriod 1 and AcademicYear 2122