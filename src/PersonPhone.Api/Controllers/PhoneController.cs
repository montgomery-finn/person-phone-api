using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PersonPhone.Api.Extensions;
using PersonPhone.Application.DTOs.Phone;
using PersonPhone.Application.Services.Phone;

namespace PersonPhone.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PhoneController : ControllerBase
{
    private readonly IPhoneService _phoneService;

    public PhoneController(IPhoneService phoneService)
    {
        _phoneService = phoneService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePhoneRequest request,
        IValidator<CreatePhoneRequest> validator)
    {
        if (await this.ValidateAsync(validator, request) is { } validationError)
            return validationError;

        var response = await _phoneService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? personId)
    {
        var phones = await _phoneService.GetAllAsync(personId);

        return Ok(phones);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _phoneService.GetByIdAsync(id);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePhoneRequest request,
        IValidator<UpdatePhoneRequest> validator)
    {
        if (await this.ValidateAsync(validator, request) is { } validationError)
            return validationError;

        var response = await _phoneService.UpdateAsync(id, request);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _phoneService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
