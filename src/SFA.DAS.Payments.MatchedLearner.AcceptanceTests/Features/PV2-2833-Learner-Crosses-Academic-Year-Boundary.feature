Feature: PV2-2833-Learner-Crosses-Academic-Year-Boundary

Scenario: EI03 - Break in learning
	Given the provider submitted a learner in Academic Year 2122 and Collection Period 12
	And the learner then had a break in learning
	And the provider then submitted without the learner in Academic Year 2324 and Collection Period 1
	When the Api is called in AY 2324 and Collection Period 1
	Then the header should have AY 2324 and Collection Period 1
	And the latest price episode should have Academic Year 2122 and Collection Period 12

Scenario: EI005A - Submission before and after rollover
	Given the provider submitted a learner in Academic Year 2122 and Collection Period 13
	And the provider submitted a learner in Academic Year 2223 and Collection Period 1
	When the Api is called in AY 2223 and Collection Period 1
	Then the header should have AY 2223 and Collection Period 1
	And the latest price episode should have Academic Year 2223 and Collection Period 1

Scenario: EI005B - Learner submission on last collection period of previous year and R02 current year
	Given the provider submitted a learner in Academic Year 2122 and Collection Period 14
	And the provider submitted a learner in Academic Year 2223 and Collection Period 2
	When the Api is called in AY 2223 and Collection Period 2
	Then the header should have AY 2223 and Collection Period 2
	And the latest price episode should have Academic Year 2223 and Collection Period 2
	And the price episode from Academic Year 2122 and Collection Period 14 submission is also returned

Scenario: EI005C - Learner submitted previous year R12, R13 followed by submission without learner in current year
	Given the provider submitted a learner in Academic Year 2122 and Collection Period 12
	And the provider submitted a learner in Academic Year 2122 and Collection Period 13
	And the provider then submitted without the learner in Academic Year 2223 and Collection Period 1
	When the Api is called in AY 2223 and Collection Period 1
	Then the header should have AY 2223 and Collection Period 1
	And the latest price episode should have Academic Year 2122 and Collection Period 13

Scenario: EI005d - Learner submission in previous year R12 and R13, api called in current year R01
	Given the provider submitted a learner in Academic Year 2122 and Collection Period 12
	And the provider submitted a learner in Academic Year 2122 and Collection Period 13
	When the Api is called in AY 2223 and Collection Period 1
	Then the header should have AY 2122 and Collection Period 13
	And the latest price episode should have Academic Year 2122 and Collection Period 13
