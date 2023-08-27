using Microsoft.AspNetCore.Mvc;
using Moq;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;
using ValidationErrorOutputModel = ShiftPlanner.Api.Controllers.ValidationErrorOutputModel;

namespace ShiftPlanner.Api.Tests.Controllers;

public abstract class ScheduleControllerShould
{
    private readonly Mock<IStoredScheduleService> _storedScheduleServiceMock = new Mock<IStoredScheduleService>();
    private readonly Mock<IScheduleValidator> _scheduleValidatorMock = new Mock<IScheduleValidator>();
    private readonly Mock<IUserService> _userServiceMock = new Mock<IUserService>();

    public class Get : ScheduleControllerShould
    {
        [Fact]
        public async Task GetSchedule()
        {
            var workplace = new Workplace("ABC");
            var month = new ScheduledMonth(2027, 7);

            var scheduledShift = new ScheduledShift(new Day(month, 3), workplace, new User("user1", "Mr. User 1", "user1@krtek.me", User.MaxDate));
            var storedSchedule = new Schedule(new[] {scheduledShift});
            var expectedOutputShifts = new[] {new ShiftAssignmentOutputModel("2027-07-03", "ABC", new UserOutputModel("user1", "Mr. User 1", "2999-01"))};

            _storedScheduleServiceMock
                .Setup(_ => _.GetSchedule(month))
                .ReturnsAsync(storedSchedule);

            var controller = CreateController();

            var response = await controller.GetSchedule(2027, 7);

            var result = Assert.IsType<OkObjectResult>(response.Result);

            Assert.Equal(expectedOutputShifts, ((ScheduleOutputModel?) result.Value)?.Shifts);
        }


        [Fact]
        public async Task ReturnNoContent_WhenScheduleDoesNotExist()
        {
            var controller = CreateController();
            var response = await controller.GetSchedule(2027, 7);
            var result = Assert.IsType<NoContentResult>(response.Result);
        }
    }

    public class Validate : ScheduleControllerShould
    {
        [Fact]
        public async Task ValidateSchedule_GivenValidSchedule()
        {
            _scheduleValidatorMock.Setup(_ => _.ValidateSchedule(It.IsAny<Tenant>(), It.IsAny<ScheduledMonth>(), It.IsAny<Schedule>())).ReturnsAsync(Array.Empty<ScheduleValidationError>());
            var controller = CreateController();

            var response = await controller.ValidateSchedule("tenant1", 2027, 7, new ScheduleInputModel(Array.Empty<ShiftAssignmentInputModel>()));

            var result = Assert.IsType<OkObjectResult>(response.Result);

            Assert.Equal(new ScheduleValidationOutputModel(true, Array.Empty<ValidationErrorOutputModel>()), (ScheduleValidationOutputModel?) result.Value);
        }

        [Fact]
        public async Task RejectSchedule_GivenInvalidSchedule()
        {
            var user = new User("user-1", "User 1", "user1@krtek.me", User.MaxDate);
            var day = new Day(2023, 1, 5);
            var dayString = "2023-01-05";
            var workplace = new Workplace("ARO");
            var tenant = TestData.Tenant;

            IReadOnlyCollection<ValidationErrorOutputModel> errors = new[]
            {
                new ValidationErrorOutputModel(new[] {new ShiftOutputModel(dayString, workplace.Name)}, new ScheduleUserOutputModel(user.UserId, user.UserName), "Unknown")
            };

            var expectedSchedule = new Schedule(
                new[]
                {
                    new ScheduledShift(day, workplace, user),
                });

            var scheduleInputModel = new ScheduleInputModel(new[] {new ShiftAssignmentInputModel(dayString, workplace.Name, user.UserId)});

            var validatedSchedules = new List<Schedule>();

            _userServiceMock
                .Setup(_ => _.GetAllUsers())
                .ReturnsAsync(new[] {user});

            _scheduleValidatorMock
                .Setup(_ => _.ValidateSchedule(tenant, new ScheduledMonth(2027, 7), Capture.In(validatedSchedules)))
                .ReturnsAsync(new[] {new ScheduleValidationError(new[] {new Shift(day, workplace)}, user.UserName, ScheduleErrorType.Unknown)});

            var controller = CreateController();

            var response = await controller.ValidateSchedule(tenant.Name, 2027, 7, scheduleInputModel);

            var result = Assert.IsType<OkObjectResult>(response.Result);

            var validatedSchedule = Assert.Single(validatedSchedules);
            Assert.Equal(expectedSchedule.Shifts, validatedSchedule.Shifts);
            var outputModel = (ScheduleValidationOutputModel?) result.Value ?? throw new InvalidOperationException();
            Assert.False(outputModel.IsValid);
            var error = Assert.Single(outputModel.ValidationErrors);
            Assert.Equal("Unknown", error.Error);
            Assert.Equal(new ScheduleUserOutputModel(user.UserId, user.UserName), error.User);
            Assert.Equal(new ShiftOutputModel(dayString, workplace.Name), error.Shifts.Single());
        }
    }

    public class Save : ScheduleControllerShould
    {
        [Fact]
        public async Task SaveSchedule()
        {
            var user = new User("user-1", "User 1", "user1@krtek.me", User.MaxDate);
            var day = new Day(2023, 1, 5);
            var dayString = "2023-01-05";
            var workplace = new Workplace("ARO");
            var tenant = TestData.Tenant;


            var expectedSchedule = new Schedule(
                new[]
                {
                    new ScheduledShift(day, workplace, user),
                });

            var scheduleInputModel = new ScheduleInputModel(new[] {new ShiftAssignmentInputModel(dayString, workplace.Name, user.UserId)});

            var storedSchedules = new List<Schedule>();

            _userServiceMock
                .Setup(_ => _.GetAllUsers())
                .ReturnsAsync(new[] {user});

            _storedScheduleServiceMock
                .Setup(_ => _.StoreSchedule(tenant, new ScheduledMonth(2027, 7), Capture.In(storedSchedules)));

            var controller = CreateController();

            var response = await controller.SaveSchedule(tenant.Name, 2027, 7, scheduleInputModel);

            Assert.IsType<OkResult>(response);

            var storedSchedule = Assert.Single(storedSchedules);
            Assert.Equal(expectedSchedule.Shifts, storedSchedule.Shifts);
        }
    }

    private ScheduleController CreateController()
    {
        return new ScheduleController(
            _storedScheduleServiceMock.Object,
            _scheduleValidatorMock.Object,
            _userServiceMock.Object);
    }
}
