using ShiftPlanner.Domain;
using ShiftScheduler.Rules;

namespace ShiftPlanner.Engine.SchedulingEngine;

internal interface ISchedulingEngine : IScheduleConstraintReceiver
{
    PlannedSchedule? Solve();
}
