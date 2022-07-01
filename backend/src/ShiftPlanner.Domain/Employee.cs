namespace ShiftPlanner.Domain;

public record Employee(string Name, CompetenceLevel CompetenceLevel, Capabilities Capabilities, uint MaxShiftsPerMonth = 6) { }
