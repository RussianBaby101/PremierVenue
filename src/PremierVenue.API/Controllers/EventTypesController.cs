using Microsoft.AspNetCore.Mvc;
using PremierVenue.Core.DTOs;
using PremierVenue.Domain.Enums;

namespace PremierVenue.API.Controllers;

[ApiController]
[Route("api/event-types")]
// Provides the available event types that can be used for venue bookings
public class EventTypesController : ControllerBase
{
    // Returns all available event types
    [HttpGet]
    public ActionResult<IEnumerable<EventTypeDto>> GetEventTypes()
    {
        return Ok(Enum.GetValues<EventType>().Select(value => new EventTypeDto
        {
            Value = value,
            Name = value.ToString()
        }));
    }
}
