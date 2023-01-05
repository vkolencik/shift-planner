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
    private readonly Mock<IScheduleValidator> _scheduleValidator = new Mock<IScheduleValidator>();

    public class Get : ScheduleControllerShould
    {
        [Fact]
        public async Task GetSchedule()
        {
            var workplace = new Workplace("ABC");
            var month = new ScheduledMonth(2027, 7);

            var scheduledShift = new ScheduledShift(new Day(month, 3), workplace, new User("user1", "Mr. User 1", "user1@krtek.me"));
            var storedSchedule = new Schedule(new[] {scheduledShift});
            var expectedOutputShifts = new[] {new ShiftAssignmentOutputModel("2027-07-03", "ABC", new UserOutputModel("user1", "Mr. User 1"))};

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
            var controller = CreateController();
            var response = await controller.ValidateSchedule(2027, 7, new ScheduleInputModel(Array.Empty<ShiftAssignmentInputModel>()));

            var result = Assert.IsType<OkObjectResult>(response.Result);

            Assert.Equal(new ScheduleValidationOutputModel(true, null), (ScheduleValidationOutputModel?) result.Value);
        }

        [Fact]
        public async Task RejectSchedule_GivenInvalidSchedule()
        {
            var user = new User("user-1", "User 1", "user1@krtek.me");
            var day = new Day(2023, 1, 5);
            var workplace = new Workplace("ARO");

            IReadOnlyCollection<ValidationErrorOutputModel> errors = new[]
            {
                new ValidationErrorOutputModel(new[] {new ShiftOutputModel("2023-01-05", workplace.Name)}, new UserOutputModel(user.UserId, user.UserName), "coxohydrates-not-carbonized")
            };

            var expectedSchedule = new Schedule(
                new[]
                {
                    new ScheduledShift(day, workplace, user),
                });
            
            var scheduleInputModel = new ScheduleInputModel(new[] {new ShiftAssignmentInputModel("2023-01-05", workplace.Name, user.UserId)});

            var validatedSchedules = new List<Schedule>();

            _scheduleValidator
                .Setup(_ => _.ValidateSchedule(Capture.In(validatedSchedules)))
                .ReturnsAsync(new[] {new ScheduleValidationError(new[] {new Shift(day, workplace)}, user, ScheduleErrorType.Unknown)});

            var controller = CreateController();
            var response = await controller.ValidateSchedule(2027, 7, scheduleInputModel);

            var result = Assert.IsType<OkObjectResult>(response.Result);

            Assert.Single(validatedSchedules);
            Assert.Equal(expectedSchedule, validatedSchedules.Single());
            Assert.Equal(new ScheduleValidationOutputModel(false, errors), (ScheduleValidationOutputModel?) result.Value);
        }
    }

    private ScheduleController CreateController()
    {
        return new ScheduleController(_storedScheduleServiceMock.Object, _scheduleValidator.Object);
    }
}
