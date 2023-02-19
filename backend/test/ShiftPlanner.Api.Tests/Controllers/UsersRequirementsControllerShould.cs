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
    private readonly Mock<IRequirementsService> _requirementsServiceMock;
    private readonly Mock<IPermissionsChecker> _permissionsCheckerMock;

    protected UsersRequirementsControllerShould()
    {
        _requirementsServiceMock = new Mock<IRequirementsService>();
        _permissionsCheckerMock = new Mock<IPermissionsChecker>();

        _permissionsCheckerMock.Setup(_ => _.CanReadRequirementsOfUser(It.IsAny<string>())).Returns(true);
        _permissionsCheckerMock.Setup(_ => _.CanApproveRequirements()).Returns(true);
        _permissionsCheckerMock.Setup(_ => _.CanModifyRequirement(It.IsAny<Day>(), It.IsAny<string>())).Returns(true);
    }

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
        public async Task ReturnForbidden_WhenUserHasNoPermissions()
        {
            var userId = "user-id-2";
            _permissionsCheckerMock.Setup(_ => _.CanReadRequirementsOfUser(userId)).Returns(false);
            var controller = CreateController();

            var response = await controller.Get("tenant-1", userId, 2022, 10);

            var result = Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(403, result.StatusCode);
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

            var controller = CreateController();

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
        public async Task ReturnForbidden_WhenUserHasNoPermissions()
        {
            var userId = "user-id-2";
            var day = new Day(2022, 10, 12);
            _permissionsCheckerMock.Setup(_ => _.CanModifyRequirement(day, userId)).Returns(false);
            var controller = CreateController();

            var response = await controller.Put("tenant-1", userId, day.Year, day.Month, day.DayOfMonth, new RequirementInputModel("hard-no"));

            var result = Assert.IsType<ObjectResult>(response);
            Assert.Equal(403, result.StatusCode);
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
        public async Task ReturnForbidden_WhenUserDoesntHavePermissions()
        {
            var userId = "user-id-2";
            var day = new Day(2022, 10, 12);
            _permissionsCheckerMock.Setup(_ => _.CanModifyRequirement(day, userId)).Returns(false);
            _permissionsCheckerMock.Setup(_ => _.CanApproveRequirements()).Returns(false);
            var controller = CreateController();

            var response = await controller.Delete("tenant-1", userId, day.Year, day.Month, day.DayOfMonth);

            var result = Assert.IsType<ObjectResult>(response);
            Assert.Equal(403, result.StatusCode);
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
            var controller = CreateController();

            var result = await controller.Approve(tenant.Name, _userId, day.Year, day.Month, day.DayOfMonth, new ApprovalInputModel(true));

            Assert.IsType<OkResult>(result);
            _requirementsServiceMock.VerifyAll();
        }

        [Theory]
        [InlineData(ApproveResult.NotFound, typeof(NotFoundResult))]
        [InlineData(ApproveResult.Failure, typeof(BadRequestResult))]
        public async Task ReturnErrorStatus_WhenApprovalNotsuccessful(ApproveResult approvalResult, Type expectedResultType)
        {
            var controller = CreateController();

            _requirementsServiceMock
                .Setup(_ => _.Approve(It.IsAny<Tenant>(), It.IsAny<string>(), It.IsAny<Day>()))
                .ReturnsAsync(approvalResult);

            var result = await controller.Approve(TestData.Tenant.Name, _userId, TestData.Day.Year, TestData.Day.Month, TestData.Day.DayOfMonth, new ApprovalInputModel(true));

            Assert.IsType(expectedResultType, result);
        }
    }

    private UsersRequirementsController CreateController()
        => new UsersRequirementsController(_requirementsServiceMock.Object, _permissionsCheckerMock.Object);
}
