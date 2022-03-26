using Google.OrTools.Sat;

namespace ShiftPlanner.Engine;

public class SolutionPrinter : CpSolverSolutionCallback
{
    public SolutionPrinter(int[] allNurses, int[] allDays, int[] allShifts,
        Dictionary<Tuple<int, int, int>, IntVar> shifts, int limit)
    {
        solutionCount_ = 0;
        allNurses_ = allNurses;
        allDays_ = allDays;
        allShifts_ = allShifts;
        shifts_ = shifts;
        solutionLimit_ = limit;
    }

    public override void OnSolutionCallback()
    {
        Console.WriteLine($"Solution #{solutionCount_}:");
        foreach (int d in allDays_)
        {
            Console.WriteLine($"Day {d}");
            foreach (int n in allNurses_)
            {
                bool isWorking = false;
                foreach (int s in allShifts_)
                {
                    var key = Tuple.Create(n, d, s);
                    if (Value(shifts_[key]) == 1L)
                    {
                        isWorking = true;
                        Console.WriteLine($"  Nurse {n} work shift {s}");
                    }
                }
                if (!isWorking)
                {
                    Console.WriteLine($"  Nurse {d} does not work");
                }
            }
        }
        solutionCount_++;
        if (solutionCount_ >= solutionLimit_)
        {
            Console.WriteLine($"Stop search after {solutionLimit_} solutions");
            StopSearch();
        }
    }

    public int SolutionCount()
    {
        return solutionCount_;
    }

    private int solutionCount_;
    private int[] allNurses_;
    private int[] allDays_;
    private int[] allShifts_;
    private Dictionary<Tuple<int, int, int>, IntVar> shifts_;
    private int solutionLimit_;
}
