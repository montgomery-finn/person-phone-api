using FluentValidation;
using FluentValidation.AspNetCore;
using PersonPhone.Application.Validators.Person;
using PersonPhone.Domain.Interfaces;
using PersonPhone.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Registra TODOS os validators do assembly automaticamente
builder.Services.AddValidatorsFromAssemblyContaining<CreatePersonRequestDtoValidator>();

// Habilita a auto-validação no pipeline (roda antes do Controller)
builder.Services.AddFluentValidationAutoValidation();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IPersonRepository, InMemoryPersonRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "PersonPhone API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
