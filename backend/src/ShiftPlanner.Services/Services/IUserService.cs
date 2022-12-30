using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<User>> GetAllUsers();
}
