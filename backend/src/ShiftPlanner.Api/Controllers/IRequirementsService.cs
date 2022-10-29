namespace ShiftPlanner.Api.Controllers;

public interface IRequirementsService
{
    Task<Dictionary<string, string>> GetRequirements(string userId, int year, int month);
}
