namespace ShiftPlanner.Domain;

public record Schedule(IDictionary<DateOnly, IDictionary<Workplace, Employee>> Assignments);
