using Moq;
using ShiftPlanner.Api.Controllers;

namespace ShiftPlanner.Api.Tests.Controllers;

public class PermissionsCheckerShould
{
    private Mock<IUserAccessor> _userAccessorMock = new Mock<IUserAccessor>(); 
    
    [Fact]
    public void AffirmThatUserCanReadTheirOwnRequirements()
    {
        var userId = "user-1";
        
        _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns(userId);
        
        var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object);

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
        
        var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object);

        var hasPermission = permissionsChecker.CanReadRequirementsOfUser("another-user-id");
        
        Assert.Equal(expectedResult, hasPermission);
    }

    [Fact]
    public void AffirmThatUserCanModifyTheirOwnRequirements()
    {
        var userId = "user-1";
        
        _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns(userId);
        
        var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object);

        var hasPermission = permissionsChecker.CanModifyRequirementsOfUser(userId);
        
        Assert.True(hasPermission);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void AffirmThatUserCanModifyRequirementsOfOtherUser_WhenUserHasSpecialPrivilege(bool hasModifyOthersPermission, bool expectedResult)
    {
        _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns("logged-in-user-id");
        _userAccessorMock.Setup(_ => _.HasClaim(Claims.ModifyRequirementsOfOthers)).Returns(hasModifyOthersPermission);
        
        var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object);

        var hasPermission = permissionsChecker.CanModifyRequirementsOfUser("another-user-id");
        
        Assert.Equal(expectedResult, hasPermission);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void AffirmThatUserCanApproveRequirements_WhenUserHasSpecialPrivilege(bool hasApprovePermission, bool expectedResult)
    {
        _userAccessorMock.Setup(_ => _.GetLoggedInUserId()).Returns("logged-in-user-id");
        _userAccessorMock.Setup(_ => _.HasClaim(Claims.ApproveRequirementsOfOthers)).Returns(hasApprovePermission);
        
        var permissionsChecker = new PermissionsChecker(_userAccessorMock.Object);

        var hasPermission = permissionsChecker.CanApproveRequirements();
        
        Assert.Equal(expectedResult, hasPermission);
    }
}
