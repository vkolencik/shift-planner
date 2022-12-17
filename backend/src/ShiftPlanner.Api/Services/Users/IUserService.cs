using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Contracts;

namespace ShiftPlanner.Api.Services.Users;

public interface IUserService
{
    Task<IReadOnlyCollection<User>> GetAllUsers();
}
