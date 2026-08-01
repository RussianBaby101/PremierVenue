using Microsoft.AspNetCore.Mvc;
using PremierVenue.API.Controllers;
using PremierVenue.Core.DTOs;
using PremierVenue.Domain.Enums;
using Xunit;

namespace PremierVenue.Tests;

public class ApiControllerTests
{
    [Fact]
    public void EventTypesController_ReturnsAllSupportedEventTypes()
    {
        var controller = new EventTypesController();

        var result = controller.GetEventTypes().Result as OkObjectResult;
        var eventTypes = Assert.IsAssignableFrom<IEnumerable<EventTypeDto>>(result?.Value);

        Assert.Equal(Enum.GetValues<EventType>().Length, eventTypes.Count());
        Assert.Contains(eventTypes, eventType => eventType.Value == EventType.Wedding);
        Assert.Contains(eventTypes, eventType => eventType.Value == EventType.Conference);
    }
}
