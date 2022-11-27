using ShiftPlanner.Api.Contracts;

namespace ShiftPlanner.Api.Services.Users;

public interface IUserService
{
    Task<IReadOnlyCollection<User>> GetAllUsers();
}
