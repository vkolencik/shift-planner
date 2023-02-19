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

            var hasPermission = permissionsChecker.CanModifyRequirement(new Day(2023, 4, 1), userId);

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

            var hasPermission = permissionsChecker.CanModifyRequirement(TestData.Day, "another-user-id");

            Assert.Equal(expectedResult, hasPermission);
        }

        [Fact]
        public void DisallowModificationAfterCutoffDate()
        {
            var userId = "user-id-1";
            var day = new Day(2023, 3, 1);

            _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns(userId);
            _userAccessorMock.Setup(_ => _.HasClaim(Claims.ModifyRequirementsOfOthers)).Returns(false);
            _clockMock.Setup(_ => _.Today).Returns(new Day(2023, 4, 14));

            var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object, _clockMock.Object);

            var hasPermission = permissionsChecker.CanModifyRequirement(day, "another-user-id");

            Assert.False(hasPermission);
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
