Feature: PV2-2833-Learner-Crosses-Academic-Year-Boundary

Scenario: EI03 - Break in learning
	Given the provider submitted a learner in Academic Year 2122 and Collection Period 12
	And the learner then had a break in learning
	And the provider then submitted again in Academic Year 2324 and Collection Period 1
	When the Api is called in AY 2324 and Collection Period 1
	Then the header should have AY 2324 and Collection Period 1
	And the latest price episode Academic Year should be 2122
	And the latest price episode CollectionPeriod should be 12
	
