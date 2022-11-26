using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests.Controllers;

public abstract class UsersRequirementsControllerShould
{
    private readonly string _userId = "user-id-1";
    private readonly Mock<IRequirementsService> _requirementsServiceMock = new Mock<IRequirementsService>();

    public class Get : UsersRequirementsControllerShould
    {
        [Fact]
        public async Task ReturnListOfRequirementsForAGivenMonth()
        {
            var tenant = TestData.Tenant;
            var month = new ScheduledMonth(2020, 10);
            var requirements = new Dictionary<Day, string>
            {
                [new Day(2022, 10, 12)] = "hard-no"
            };

            _requirementsServiceMock.Setup(_ => _.GetRequirements(tenant, _userId, month)).ReturnsAsync(requirements);
            var controller = CreateController();

            var response = await controller.Get(tenant.Name, _userId, month.Year, month.Month);
            var result = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(
                new Dictionary<string, string>()
                {
                    ["2022-10-12"] = "hard-no"
                }, result.Value);
        }

        [Fact]
        public async Task ReturnUnauthorized_GivenTokenForDifferentUser()
        {
            var controller = CreateController("user-id-1");

            var response = await controller.Get("tenant-1", "user-id-2", 2022, 10);

            Assert.IsType<UnauthorizedObjectResult>(response.Result);
        }
    }

    public class Put : UsersRequirementsControllerShould
    {
        [Fact]
        public async Task CreateNewRequirement()
        {
            var tenant = TestData.Tenant;
            var day = new Day(2022, 10, 18);
            var controller = CreateController();
            var requirement = "hard-no";

            var response = await controller.Put(tenant.Name, _userId, day.Year, day.Month, day.DayOfMonth, new RequirementInputModel(requirement));
            Assert.IsType<OkResult>(response);

            _requirementsServiceMock.Verify(_ => _.SetRequirement(tenant, _userId, day, requirement));
        }

        [Fact]
        public async Task ReturnUnauthorized_GivenTokenForDifferentUser()
        {
            var controller = CreateController("user-id-1");

            var response = await controller.Put("tenant-1", "user-id-2", 2022, 10, 12, new RequirementInputModel("hard-no"));

            Assert.IsType<UnauthorizedObjectResult>(response);
        }
    }

    public class Delete : UsersRequirementsControllerShould
    {
        [Fact]
        public async Task DeleteRequirement()
        {
            var tenant = TestData.Tenant;
            var day = new Day(2022, 10, 18);
            _requirementsServiceMock.Setup(_ => _.DeleteRequirement(tenant, _userId, day))
                .ReturnsAsync(DeleteResult.Deleted);
            var controller = CreateController();

            var response = await controller.Delete(tenant.Name, _userId, day.Year, day.Month, day.DayOfMonth);

            _requirementsServiceMock.Verify(_ => _.DeleteRequirement(tenant, _userId, day));

            Assert.IsType<OkResult>(response);
        }

        [Fact]
        public async Task ReturnNotFound_GivenNonexistentRequirement()
        {
            var tenant = TestData.Tenant;
            var day = new Day(2022, 10, 18);
            _requirementsServiceMock.Setup(_ => _.DeleteRequirement(tenant, _userId, day))
                .ReturnsAsync(DeleteResult.NotFound);
            var controller = CreateController();

            var response = await controller.Delete(tenant.Name, _userId, day.Year, day.Month, day.DayOfMonth);

            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task ReturnUnauthorized_GivenTokenForDifferentUser()
        {
            var controller = CreateController("user-id-1");

            var response = await controller.Delete("tenant-1", "user-id-2", 2022, 10, 12);

            Assert.IsType<UnauthorizedObjectResult>(response);
        }
    }

    private UsersRequirementsController CreateController() => CreateController(_userId);

    private UsersRequirementsController CreateController(string userId)
    {
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name, userId),
                }, "mock"));

        var controller = new UsersRequirementsController(_requirementsServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() {User = user}
            }
        };

        return controller;
    }
}
