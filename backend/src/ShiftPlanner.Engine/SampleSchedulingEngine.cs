using Google.OrTools.Sat;

namespace ShiftPlanner.Engine;
public static class SampleSchedulingEngine
{
    private class VarArraySolutionPrinter : CpSolverSolutionCallback
    {
        public VarArraySolutionPrinter(IntVar[] variables)
        {
            variables_ = variables;
        }

        public override void OnSolutionCallback()
        {
            {
                Console.WriteLine(String.Format("Solution #{0}: time = {1:F2} s", solution_count_, WallTime()));
                foreach (IntVar v in variables_)
                {
                    Console.WriteLine(String.Format("  {0} = {1}", v.ShortString(), Value(v)));
                }
                solution_count_++;
            }
        }

        public int SolutionCount()
        {
            return solution_count_;
        }

        private int solution_count_;
        private IntVar[] variables_;
    }

    public static void Test()
    {
        CpModel model = new CpModel();
        
        IntVar x = model.NewIntVar(0, 10, "x");
        IntVar y = model.NewIntVar(0, 20, "y");
        IntVar z = model.NewIntVar(0, 30, "z");
        
        model.Add(x != y);
        model.Add(y != z);
        model.Add(x != z);

        model.Add(x + y + z == 13);
        
        model.Maximize(z);
        
        CpSolver solver = new CpSolver();
        solver.StringParameters = "enumerate_all_solutions:true";
        
        VarArraySolutionPrinter cb = new VarArraySolutionPrinter(new IntVar[] { x, y, z });

        CpSolverStatus status = solver.Solve(model, cb);
        
        Console.WriteLine(status);
        Console.WriteLine($"{solver.Value(x)}, {solver.Value(y)}, {solver.Value(z)}");
        
        Console.WriteLine("Statistics");
        Console.WriteLine($"  conflicts : {solver.NumConflicts()}");
        Console.WriteLine($"  branches  : {solver.NumBranches()}");
        Console.WriteLine($"  wall time : {solver.WallTime()} s");
        Console.WriteLine($"  number of solutions found: {cb.SolutionCount()}");
    }

    public static void CalculateSchedule()
    {
        const int numNurses = 5;
        const int numDays = 7;
        const int numShifts = 3;

        int[] allNurses = Enumerable.Range(0, numNurses).ToArray();
        int[] allDays = Enumerable.Range(0, numDays).ToArray();
        int[] allShifts = Enumerable.Range(0, numShifts).ToArray();

        int[,,] shiftRequests = new int[,,] {
            {
                { 0, 0, 1 },
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 1 },
                { 0, 1, 0 },
                { 0, 0, 1 },
            },
            {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 1, 0 },
                { 0, 1, 0 },
                { 1, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 1 },
            },
            {
                { 0, 1, 0 },
                { 0, 1, 0 },
                { 0, 0, 0 },
                { 1, 0, 0 },
                { 0, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 0 },
            },
            {
                { 0, 0, 1 },
                { 0, 0, 0 },
                { 1, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 0 },
                { 1, 0, 0 },
                { 0, 0, 0 },
            },
            {
                { 0, 0, 0 },
                { 0, 0, 1 },
                { 0, 1, 0 },
                { 0, 0, 0 },
                { 1, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 0 },
            },
        };

        // Creates the model.
        CpModel model = new CpModel();

        // Creates shift variables.
        // shifts[(n, d, s)]: nurse 'n' works shift 's' on day 'd'.
        Dictionary<Tuple<int, int, int>, IntVar> shifts = new Dictionary<Tuple<int, int, int>, IntVar>();
        foreach (int n in allNurses)
        {
            foreach (int d in allDays)
            {
                foreach (int s in allShifts)
                {
                    shifts.Add(Tuple.Create(n, d, s), model.NewBoolVar($"shifts_n{n}d{d}s{s}"));
                }
            }
        }

        // Each shift is assigned to exactly one nurse in the schedule period.
        foreach (int d in allDays)
        {
            foreach (int s in allShifts)
            {
                IntVar[] x = new IntVar[numNurses];
                foreach (int n in allNurses)
                {
                    var key = Tuple.Create(n, d, s);
                    x[n] = shifts[key];
                }
                model.Add(LinearExpr.Sum(x) == 1);
            }
        }

        // Each nurse works at most one shift per day.
        foreach (int n in allNurses)
        {
            foreach (int d in allDays)
            {
                IntVar[] x = new IntVar[numShifts];
                foreach (int s in allShifts)
                {
                    var key = Tuple.Create(n, d, s);
                    x[s] = shifts[key];
                }
                model.Add(LinearExpr.Sum(x) <= 1);
            }
        }

        // Try to distribute the shifts evenly, so that each nurse works
        // minShiftsPerNurse shifts. If this is not possible, because the total
        // number of shifts is not divisible by the number of nurses, some nurses will
        // be assigned one more shift.
        int minShiftsPerNurse = (numShifts * numDays) / numNurses;
        int maxShiftsPerNurse;
        if ((numShifts * numDays) % numNurses == 0)
        {
            maxShiftsPerNurse = minShiftsPerNurse;
        }
        else
        {
            maxShiftsPerNurse = minShiftsPerNurse + 1;
        }
        foreach (int n in allNurses)
        {
            IntVar[] numShiftsWorked = new IntVar[numDays * numShifts];
            foreach (int d in allDays)
            {
                foreach (int s in allShifts)
                {
                    var key = Tuple.Create(n, d, s);
                    numShiftsWorked[d * numShifts + s] = shifts[key];
                }
            }
            model.AddLinearConstraint(LinearExpr.Sum(numShiftsWorked), minShiftsPerNurse, maxShiftsPerNurse);
        }

        IntVar[] flatShifts = new IntVar[numNurses * numDays * numShifts];
        int[] flatShiftRequests = new int[numNurses * numDays * numShifts];
        foreach (int n in allNurses)
        {
            foreach (int d in allDays)
            {
                foreach (int s in allShifts)
                {
                    var key = Tuple.Create(n, d, s);
                    flatShifts[n * numDays * numShifts + d * numShifts + s] = shifts[key];
                    flatShiftRequests[n * numDays * numShifts + d * numShifts + s] = shiftRequests[n, d, s];
                }
            }
        }
        model.Maximize(LinearExpr.ScalProd(flatShifts, flatShiftRequests));

        // Solve
        CpSolver solver = new CpSolver();
        CpSolverStatus status = solver.Solve(model);
        Console.WriteLine($"Solve status: {status}");

        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            Console.WriteLine("Solution:");
            foreach (int d in allDays)
            {
                Console.WriteLine($"Day {d}");
                foreach (int n in allNurses)
                {
                    bool isWorking = false;
                    foreach (int s in allShifts)
                    {
                        var key = Tuple.Create(n, d, s);
                        if (solver.Value(shifts[key]) == 1L)
                        {
                            if (shiftRequests[n, d, s] == 1)
                            {
                                Console.WriteLine($"  Nurse {n} work shift {s} (requested).");
                            }
                            else
                            {
                                Console.WriteLine($"  Nurse {n} work shift {s} (not requested).");
                            }
                        }
                    }
                }
            }
            Console.WriteLine(
                $"Number of shift requests met = {solver.ObjectiveValue} (out of {numNurses * minShiftsPerNurse}).");
        }
        else
        {
            Console.WriteLine("No solution found.");
        }

        Console.WriteLine("Statistics");
        Console.WriteLine($"  conflicts: {solver.NumConflicts()}");
        Console.WriteLine($"  branches : {solver.NumBranches()}");
        Console.WriteLine($"  wall time: {solver.WallTime()}s");
    }
}
