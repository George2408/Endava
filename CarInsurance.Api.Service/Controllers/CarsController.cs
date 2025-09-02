using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        }

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{carId:long}/claims")]
    public async Task<ActionResult> RegisterClaim(long carId, [FromBody] ClaimDto claimDto)
    {
        try
        {
            var claim = await _service.RegisterClaimAsync(carId, claimDto);
            return CreatedAtAction(nameof(RegisterClaim), new { id = claim.Id }, claim);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{carId:long}/history")]
    public async Task<ActionResult<List<HistoryEventDto>>> GetCarHistory(long carId)
    {
        try
        {
            var history = await _service.GetCarHistoryAsync(carId);
            return Ok(history);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
