using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests.Controllers;

public class RequirementsControllerShould
{
    [Fact]
    public async Task ReturnRequirementsForMonth()
    {
        var tenant = TestData.Tenant;
        var month = new ScheduledMonth(2020, 10);

        var controller = new RequirementsController();

        var response = await controller.Get(tenant.Name, month.Year, month.Month);
    }
}
