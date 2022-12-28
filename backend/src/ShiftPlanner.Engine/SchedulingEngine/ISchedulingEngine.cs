using ShiftPlanner.Domain;
using ShiftPlanner.Rules;

namespace ShiftPlanner.Engine.SchedulingEngine;

internal interface ISchedulingEngine : IScheduleConstraintReceiver
{
    PlannedSchedule? Solve();
}
