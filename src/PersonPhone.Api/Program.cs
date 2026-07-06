using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PersonPhone.Application.Services.Person;
using PersonPhone.Application.Validators.Person;
using PersonPhone.Domain.Interfaces;
using PersonPhone.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Registra TODOS os validators do assembly automaticamente
builder.Services.AddValidatorsFromAssemblyContaining<CreatePersonRequestDtoValidator>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IPersonRepository, InMemoryPersonRepository>();
builder.Services.AddScoped<IPersonService, PersonService>();

builder.Services.AddProblemDetails();

var app = builder.Build();

// Traduz exceções não tratadas (ex: regras de domínio violadas) em ProblemDetails,
// no mesmo formato que o FluentValidation já usa para erros de validação.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var statusCode = exception is ArgumentException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == StatusCodes.Status400BadRequest
                ? "One or more validation errors occurred."
                : "An unexpected error occurred.",
            Detail = exception?.Message
        });
    });
});

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
