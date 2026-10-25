using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PricingService.Api.Commands;

namespace PricingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PricingController : ControllerBase
{
    private readonly IMediator mediator;

    public PricingController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CalculatePriceCommand command)
    {
        try
        {
            return new JsonResult(await mediator.Send(command));
        }
        catch (ValidationException exception)
        {
            return BadRequest(exception.Errors);
        }
    }
}
