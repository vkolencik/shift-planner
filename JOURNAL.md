#  2022-04-24
* Added percentual preferences for workplaces to model
* Split integration test to several test cases

# 2022-04-18
* Added employee qualifications

# 2022-04-16
* Refactored rules to a separate class, covered them by tests
* Created directory structure in the solver engine project
* Added rule for alternate days

# 2022-03-31
* Separated scheduler and solver

# 2022-03-27
* Added basic scheduling rules:
  * Every workplace has an employee assigned for every day
  * No employee is asigned to two workplaces at once
  * Employees can't have consecutive shifts

# 2022-03-26
* Finished basic scheduller, including the ability to add generic constraints on sums of variables.
  * Generic shift scheduler component
  * Thin wrapper around Google CP library's model and solver so that the scheduler is is testable

# 2022-03-20
* Added domain project
* Started to write scheduler from scratch
  * Create wrapper interfaces around Google CP libs so that stuff can be unit tested
  * Convert input domain values to the language of Google CP solver (Employees/Shifts/Workplaces => CP boolean variables)
  * Convert computation results to output model (CP boolean variables => map of Day > Workplace > Employee)

# 2022-03-13
* Read the [docs](https://developers.google.com/optimization/scheduling/employee_scheduling#requests) to see how different rules can be expressed in the solver.
* Added example with shift requests. 

# 2022-03-12
* Created [dotnet backend project](./backend).
* Experimented with [Google OR tools lib for constraint optimization](https://developers.google.com/optimization/cp) in order to find out whether it's a match for our problem. So far it seems to work great! The next task would be to find out how to describe the constraints of our domain in terms of CP concepts and try it out on a real-world example.
