using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Create([FromBody] CreatePersonRequest request)
    {
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
}