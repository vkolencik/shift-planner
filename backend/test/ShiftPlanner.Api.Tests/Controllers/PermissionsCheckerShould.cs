using Moq;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests.Controllers;

public abstract class PermissionsCheckerShould
{
    private Mock<IUserAccessor> _userAccessorMock = new Mock<IUserAccessor>();
    private Mock<IClock> _clockMock = new Mock<IClock>();

    public class ReadRequirements : PermissionsCheckerShould
    {
        [Fact]
        public void AffirmThatUserCanReadTheirOwnRequirements()
        {
            var userId = "user-1";

            _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns(userId);

            var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object, _clockMock.Object);

            var hasPermission = permissionsChecker.CanReadRequirementsOfUser(userId);

            Assert.True(hasPermission);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void AffirmThatUserCanReadRequirementsOfOtherUser_WhenUserHasSpecialPrivilege(bool hasReadOthersPermission, bool expectedResult)
        {
            _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns("logged-in-user-id");
            _userAccessorMock.Setup(_ => _.HasClaim(Claims.ReadRequirementsOfOthers)).Returns(hasReadOthersPermission);

            var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object, _clockMock.Object);

            var hasPermission = permissionsChecker.CanReadRequirementsOfUser("another-user-id");

            Assert.Equal(expectedResult, hasPermission);
        }
    }

    public class ModifyRequirements : PermissionsCheckerShould
    {
        [Fact]
        public void AffirmThatUserCanModifyTheirOwnRequirements()
        {
            var userId = "user-1";

            _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns(userId);
            _clockMock.Setup(_ => _.Today).Returns(new Day(2023, 2, 26));

            var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object, _clockMock.Object);

            var hasPermission = permissionsChecker.CanModifyRequirement(new Day(2023, 4, 1), userId, "hard-no");

            Assert.True(hasPermission);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void AffirmThatUserCanModifyRequirementsOfOtherUser_WhenUserHasSpecialPrivilege(bool hasModifyOthersPermission, bool expectedResult)
        {
            _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns("logged-in-user-id");
            _userAccessorMock.Setup(_ => _.HasClaim(Claims.ModifyRequirementsOfOthers)).Returns(hasModifyOthersPermission);

            var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object, _clockMock.Object);

            var hasPermission = permissionsChecker.CanModifyRequirement(TestData.Day, "another-user-id", "hard-no");

            Assert.Equal(expectedResult, hasPermission);
        }

        [Theory]
        [InlineData("Requirement in the past", 4, 14, 3, 1, false)]
        [InlineData("Requirement long in the past", 12, 1, 1, 1, false)]
        [InlineData("Requirement today", 3, 1, 3, 1, false)]
        [InlineData("Requirement before month is made active", 2, 24, 4, 1, false)]
        [InlineData("Requirement after month is made active", 2, 25, 4, 1, true)]
        [InlineData("Requirement right before month is made read-only", 3, 5, 4, 1, true)]
        [InlineData("Requirement right after month is made read-only", 3, 6, 4, 1, false)]
        [InlineData("Requirement for summer month 1 - start of period", 5, 25, 7, 1, true)]
        [InlineData("Requirement for summer month 2 - start of period", 5, 25, 8, 1, true)]
        [InlineData("Requirement right after month is made Requirement for summer month 1 - end of period", 6, 5, 7, 1, true)]
        [InlineData("Requirement right after month is made Requirement for summer month 2 - end of period", 6, 5, 8, 1, true)]
        [InlineData("Requirement for summer month 2 on summer month 1 - start of period", 6, 25, 8, 1, false)]
        [InlineData("Requirement for summer month 2 on summer month 1 - end of period", 7, 5, 8, 1, false)]
        [InlineData("Requirement far in the future", 3, 1, 6, 1, false)]
        [InlineData("Special case for 2023-10 before deadline", 10, 6, 11, 1, true)]
        [InlineData("Special case for 2023-10 after deadline", 10, 16, 11, 1, false)]
#pragma warning disable xUnit1026
        public void DisallowModificationAfterCutoffDate(string description, int currentMonth, int currentDay, int requirementMonth, int requirementDay, bool isPermitted)
#pragma warning restore xUnit1026
        {
            var userId = "user-id-1";
            var day = new Day(2023, requirementMonth, requirementDay);

            _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns(userId);
            _userAccessorMock.Setup(_ => _.HasClaim(Claims.ModifyRequirementsOfOthers)).Returns(false);
            _clockMock.Setup(_ => _.Today).Returns(new Day(2023, currentMonth, currentDay));

            var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object, _clockMock.Object);

            var hasPermission = permissionsChecker.CanModifyRequirement(day, userId, "hard-no");

            Assert.Equal(isPermitted, hasPermission);
        }

        [Theory]
        [InlineData(2023, 12, 31, true)]
        [InlineData(2024, 1, 1, false)]
        public void AllowOrForbidModificationAfterCutoffDateForLeaveRequirement(int requirementYear, int requirementMonth, int requirementDay, bool shouldBeAllowed)
        {
            var userId = "user-id-1";
            var requirementDate = new Day(requirementYear, requirementMonth, requirementDay);

            _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns(userId);
            _userAccessorMock.Setup(_ => _.HasClaim(Claims.ModifyRequirementsOfOthers)).Returns(false);
            _clockMock.Setup(_ => _.Today).Returns(new Day(2023, 1, 1));

            var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object, _clockMock.Object);

            var hasPermission = permissionsChecker.CanModifyRequirement(requirementDate, userId, "leave");

            Assert.Equal(shouldBeAllowed, hasPermission);
            
        }
    }

    public class ApproveRequirements : PermissionsCheckerShould
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void AffirmThatUserCanApproveRequirements_WhenUserHasSpecialPrivilege(bool hasApprovePermission, bool expectedResult)
        {
            _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns("logged-in-user-id");
            _userAccessorMock.Setup(_ => _.HasClaim(Claims.ApproveRequirementsOfOthers)).Returns(hasApprovePermission);

            var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object, _clockMock.Object);

            var hasPermission = permissionsChecker.CanApproveRequirements();

            Assert.Equal(expectedResult, hasPermission);
        }
    }
}
