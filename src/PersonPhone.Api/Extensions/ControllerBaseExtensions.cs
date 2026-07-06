using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace PersonPhone.Api.Extensions;

public static class ControllerBaseExtensions
{
    public static async Task<IActionResult?> ValidateAsync<T>(
        this ControllerBase controller, IValidator<T> validator, T request)
    {
        var result = await validator.ValidateAsync(request);

        if (result.IsValid)
            return null;

        foreach (var error in result.Errors)
            controller.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

        return controller.ValidationProblem(controller.ModelState);
    }
}
