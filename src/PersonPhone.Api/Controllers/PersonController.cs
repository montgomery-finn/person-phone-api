using Microsoft.AspNetCore.Mvc;
using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Interfaces;
using PersonPhone.Application.DTOs.Person;

namespace PersonPhone.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PersonController : ControllerBase
{
    private readonly IPersonRepository _personRepository;

    public PersonController(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePersonRequest request)
    {
        var (name, cpf, birthDate) = request;
        
        var person = new Person(name, cpf, birthDate);

        await _personRepository.AddAsync(person);

        return CreatedAtAction(nameof(GetById), new { id = person.Id}, person);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var people = await _personRepository.GetAllAsync();

        return Ok(people);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var person = await _personRepository.GetByIdAsync(id);

        if (person is null)
            return NotFound();

        return Ok(person);
    }
}