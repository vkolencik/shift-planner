namespace ShiftPlanner.Domain;

public record Shift(Employee Employee, DateOnly Day, Workplace Workplace);