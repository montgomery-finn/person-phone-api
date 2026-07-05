using Microsoft.AspNetCore.Mvc;
using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Interfaces;

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

    public record CreatePersonRequest(string name, string cpf, DateTime birthDate);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePersonRequest request)
    {
        var (name, cpf, birthDate) = request;
        
        var person = new Person(name, cpf, birthDate);

        await _personRepository.AddAsync(person);
        
        return Ok(person);
    }
}