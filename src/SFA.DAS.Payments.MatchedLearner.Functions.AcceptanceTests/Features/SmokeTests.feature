Feature: SmokeTests

Scenario: Function pulls the data from Payments DB and saves to matched learner DB
	Given A Submission Job Succeeded for CollectionPeriod <collection-period> and AcademicYear <academic-year>
	When A SubmissionJobSucceeded message is received for CollectionPeriod <collection-period> and AcademicYear <academic-year>
	Then the matched Learners are only Imported for CollectionPeriod <collection-period> and AcademicYear <academic-year>

Examples:
| academic-year | collection-period |
| 2122          | 1                 |
| 2021          | 1                 |
| 2021          | 14                |

Scenario: Existing data for current collection data is deleted before saving new Data to matched learner DB
	Given A Submission Job Succeeded for CollectionPeriod 1 and AcademicYear 2122
	And there is existing data For CollectionPeriod 1 and AcademicYear 2122
	When A SubmissionJobSucceeded message is received for CollectionPeriod 1 and AcademicYear 2122
	Then the existing matched Learners are deleted
	And the matched Learners are only Imported for CollectionPeriod 1 and AcademicYear 2122

Scenario: Existing Date for previous collection then data is deleted before saving new Data to matched learner DB
	Given A Submission Job Succeeded for CollectionPeriod 2 and AcademicYear 2122
	And there is existing data For CollectionPeriod 1 and AcademicYear 2122
	When A SubmissionJobSucceeded message is received for CollectionPeriod 2 and AcademicYear 2122
	Then the existing matched Learners are deleted
	And the matched Learners are only Imported for CollectionPeriod 2 and AcademicYear 2122
	
Scenario: Existing Date for previous Academic year latest collection then only new data is saved to matched learner DB
	Given A Submission Job Succeeded for CollectionPeriod 1 and AcademicYear 2122
	And there is existing data For CollectionPeriod 14 and AcademicYear 2021
	When A SubmissionJobSucceeded message is received for CollectionPeriod 1 and AcademicYear 2122
	Then the existing matched Learners are NOT deleted
	And the matched Learners are only Imported for CollectionPeriod 1 and AcademicYear 2122