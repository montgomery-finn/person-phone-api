using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PersonPhone.Api.Extensions;
using PersonPhone.Application.DTOs.Person;
using PersonPhone.Application.Services.Person;

namespace PersonPhone.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PersonController : ControllerBase
{
    private readonly IPersonService _personService;

    public PersonController(IPersonService personService)
    {
        _personService = personService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePersonRequest request,
        IValidator<CreatePersonRequest> validator)
    {
        if (await this.ValidateAsync(validator, request) is { } validationError) // testa se o resultado da task é diferente de nulo e atribui à variável
            return validationError;

        var response = await _personService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var people = await _personService.GetAllAsync();

        return Ok(people);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _personService.GetByIdAsync(id);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePersonRequest request,
        IValidator<UpdatePersonRequest> validator)
    {
        if (await this.ValidateAsync(validator, request) is { } validationError)
            return validationError;

        var response = await _personService.UpdateAsync(id, request);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _personService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}