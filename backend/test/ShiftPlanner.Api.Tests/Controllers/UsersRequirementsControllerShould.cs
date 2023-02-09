using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

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
        public async Task ReturnForbidden_GivenTokenForDifferentUser()
        {
            var controller = CreateController("user-id-1");

            var response = await controller.Get("tenant-1", "user-id-2", 2022, 10);

            var result = Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task ReturnForbidden_GivenReadOnlyMonth()
        {
            var controller = CreateController("user-id-1");

            var response = await controller.Put("tenant-1", "user-id-2", 2022, 10, 1, new RequirementInputModel("hard-no"));

            Assert.IsType<ForbidResult>(response);
        }

        [Fact]
        public async Task ReturnOkReadingRequirementsOfDifferentUser_WhenCallerIsManager()
        {
            var tenant = TestData.Tenant;
            var month = new ScheduledMonth(2020, 10);
            var requirements = new Dictionary<Day, string>
            {
                [new Day(2022, 10, 12)] = "hard-no"
            };

            _requirementsServiceMock.Setup(_ => _.GetRequirements(tenant, _userId, month)).ReturnsAsync(requirements);

            var controller = CreateController("user-id-1", new[] {Claims.ReadRequirementsOfOthers});

            var response = await controller.Get(tenant.Name, _userId, month.Year, month.Month);

            Assert.IsType<OkObjectResult>(response.Result);
        }
    }

    public class Put : UsersRequirementsControllerShould
    {
        [Fact]
        public async Task CreateNewRequirement()
        {
            var requirement = TestData.Requirement with {UserId = _userId};

            var controller = CreateController();

            var response = await controller.Put(
                requirement.Tenant.Name,
                requirement.UserId,
                requirement.Date.Year,
                requirement.Date.Month,
                requirement.Date.DayOfMonth,
                new RequirementInputModel(requirement.RequirementType));

            Assert.IsType<OkResult>(response);
            _requirementsServiceMock.Verify(_ => _.SetRequirement(requirement));
        }

        [Fact]
        public async Task ReturnForbidden_GivenTokenForDifferentUser()
        {
            var controller = CreateController("user-id-1");

            var response = await controller.Put("tenant-1", "user-id-2", 2022, 10, 12, new RequirementInputModel("hard-no"));

            var result = Assert.IsType<ObjectResult>(response);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task CreateNewRequirement_GivenTokenForManager()
        {
            var controller = CreateController("user-id-1", new[] {Claims.ModifyRequirementsOfOthers});

            var response = await controller.Put("tenant-1", "user-id-2", 2022, 10, 12, new RequirementInputModel("hard-no"));

            Assert.IsType<OkResult>(response);
            _requirementsServiceMock.Verify(_ => _.SetRequirement(It.IsAny<Requirement>()));
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
        public async Task ReturnForbidden_GivenTokenForDifferentUser()
        {
            var controller = CreateController("user-id-1");

            var response = await controller.Delete("tenant-1", "user-id-2", 2022, 10, 12);

            var result = Assert.IsType<ObjectResult>(response);
            Assert.Equal(403, result.StatusCode);
        }

        [Theory]
        [InlineData(Claims.ApproveRequirementsOfOthers)]
        [InlineData(Claims.ModifyRequirementsOfOthers)]
        public async Task DeleteOtherUserRequirement_WhenDeletedByManager(string claim)
        {
            var tenant = TestData.Tenant;
            var day = new Day(2022, 10, 18);
            _requirementsServiceMock.Setup(_ => _.DeleteRequirement(tenant, "userId2", day))
                .ReturnsAsync(DeleteResult.Deleted);
            var controller = CreateController(_userId, new[] {claim});

            var response = await controller.Delete(tenant.Name, "userId2", day.Year, day.Month, day.DayOfMonth);

            _requirementsServiceMock.Verify(_ => _.DeleteRequirement(tenant, "userId2", day));

            Assert.IsType<OkResult>(response);
        }
    }

    public class Approval : UsersRequirementsControllerShould
    {
        [Fact]
        public async Task ApproveRequirement()
        {
            var tenant = TestData.Tenant;
            var day = new Day(2022, 10, 18);
            _requirementsServiceMock
                .Setup(_ => _.Approve(tenant, _userId, day))
                .ReturnsAsync(ApproveResult.Success)
                .Verifiable();
            var controller = CreateController(_userId, new[] {Claims.ApproveRequirementsOfOthers});

            var result = await controller.Approve(tenant.Name, _userId, day.Year, day.Month, day.DayOfMonth, new ApprovalInputModel(true));

            Assert.IsType<OkResult>(result);
            _requirementsServiceMock.VerifyAll();
        }

        [Theory]
        [InlineData(ApproveResult.NotFound, typeof(NotFoundResult))]
        [InlineData(ApproveResult.Failure, typeof(BadRequestResult))]
        public async Task ReturnErrorStatus_WhenApprovalNotsuccessful(ApproveResult approvalResult, Type expectedResultType)
        {
            var controller = CreateController(_userId, new[] {Claims.ApproveRequirementsOfOthers});

            _requirementsServiceMock
                .Setup(_ => _.Approve(It.IsAny<Tenant>(), It.IsAny<string>(), It.IsAny<Day>()))
                .ReturnsAsync(approvalResult);

            var result = await controller.Approve(TestData.Tenant.Name, _userId, TestData.Day.Year, TestData.Day.Month, TestData.Day.DayOfMonth, new ApprovalInputModel(true));

            Assert.IsType(expectedResultType, result);
        }
    }

    private UsersRequirementsController CreateController() => CreateController(_userId);

    private UsersRequirementsController CreateController(string userId) => CreateController(userId, Array.Empty<string>());

    private UsersRequirementsController CreateController(string userId, string[] permissions)
    {
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name, userId)
                }.Concat(permissions.Select(p => new Claim("permissions", p))),
                "mock"));

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
