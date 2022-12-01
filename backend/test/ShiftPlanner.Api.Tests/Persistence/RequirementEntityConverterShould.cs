using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Api.Persistence;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests.Persistence;

public class RequirementEntityConverterShould
{
    [Theory]
    [InlineData("hard-no", null)]
    [InlineData("leave", true)]
    public void ConvertRequirementToEntity(string requirementType, bool? approved)
    {
        var model = new RequirementModel(new Tenant("tenant1"), "user-id-abc", new Day(2023, 2, 21), requirementType, approved);
        var converter = new RequirementEntityConverter();

        var entity = converter.ToEntity(model);

        Assert.Equal("v1:tenant1:user-id-abc:2023-02-21", entity.Key);
        Assert.Equal("2023-02-21", entity.Date);
        Assert.Equal(requirementType, entity.Requirement);
        Assert.Equal("tenant1", entity.Tenant);
        Assert.Equal("user-id-abc", entity.UserId);
        Assert.Equal(approved, entity.Approved);
        
        Assert.Equal("v1:tenant1:2023-02", entity.TenantMonthKey);
        Assert.Equal("v1:tenant1:user-id-abc:2023-02", entity.TenantUserMonthKey);
    }

    [Fact]
    public void ConvertEntitiesToDictionary()
    {
        var converter = new RequirementEntityConverter();

        var entities = new[]
        {
            new RequirementEntity
            {
                Date = "2022-11-19",
                Requirement = "hard-no"
            }
        };

        var dictionary = converter.RequirementEntitiesToMonthRequirements(entities);

        var expectedResult = new Dictionary<Day, string>
        {
            [new Day(2022, 11, 19)] = "hard-no"
        };

        Assert.Equal(expectedResult, dictionary);
    }

    [Theory]
    [InlineData("hard-no", null)]
    [InlineData("leave", true)]
    public void ConvertEntityToModel(string requirementType, bool? approved)
    {
        var entity = new RequirementEntity
            {
                Tenant = "tenant1",
                UserId = "user1",
                Date = "2022-11-19",
                Requirement = requirementType,
                Approved = approved
            };

        var expectedModel = new RequirementModel(new Tenant("tenant1"),"user1", new Day(2022, 11, 19), requirementType, approved);

        var converter = new RequirementEntityConverter();

        var requirements = converter.EntityToModel(entity);

        Assert.Equal(expectedModel, requirements);
    }

    [Fact]
    public void ComputePartitionKey()
    {
        var converter = new RequirementEntityConverter();

        var key = converter.GetPartitionKey(new Tenant("tenant1"), "user-id-abc", new Day(2023, 2, 21));

        Assert.Equal("v1:tenant1:user-id-abc:2023-02-21", key);
    }

    [Fact]
    public void ComputeUserMonthKey()
    {
        var converter = new RequirementEntityConverter();

        var key = converter.GetUserMonthKey(new Tenant("tenant1"), "user-id-abc", new ScheduledMonth(2023, 2));

        Assert.Equal("v1:tenant1:user-id-abc:2023-02", key);
    }

    [Fact]
    public void ComputeMonthKey()
    {
        var converter = new RequirementEntityConverter();

        var key = converter.GetMonthKey(new Tenant("tenant1"), new ScheduledMonth(2023, 2));

        Assert.Equal("v1:tenant1:2023-02", key);
    }
}
