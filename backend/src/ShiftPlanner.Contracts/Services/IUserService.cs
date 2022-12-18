namespace ShiftPlanner.Contracts.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<User>> GetAllUsers();
}
