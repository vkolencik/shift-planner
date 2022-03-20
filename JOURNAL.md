# 2022-03-20
* Added domain project
* Started to write scheduler from scratch
  * Create wrapper interfaces around Google CP libs so that stuff can be unit tested
  * Convert input domain values to the language of Google CP solver (Employees/Shifts/Workplaces => CP boolean variables)

# 2022-03-13
* Read the [docs](https://developers.google.com/optimization/scheduling/employee_scheduling#requests) to see how different rules can be expressed in the solver.
* Added example with shift requests. 

# 2022-03-12
* Created [dotnet backend project](./backend).
* Experimented with [Google OR tools lib for constraint optimization](https://developers.google.com/optimization/cp) in order to find out whether it's a match for our problem. So far it seems to work great! The next task would be to find out how to describe the constraints of our domain in terms of CP concepts and try it out on a real-world example.
