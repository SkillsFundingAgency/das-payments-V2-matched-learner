Feature: MigrationTests

Scenario: Function pulls the datalock events from Payments DB and saves Trainings to matched learner DB Ignoring Duplicates
	Given A Successful Submission Job for 5 Learners in CollectionPeriod 1 and AcademicYear 2122
	And Duplicate Matched Learners Trainings Already Exists for 1 Learners CollectionPeriod 1 and AcademicYear 2122
	When Migration is Run for Provider
	Then Matched Learners Trainings are only Imported for 4 Learners