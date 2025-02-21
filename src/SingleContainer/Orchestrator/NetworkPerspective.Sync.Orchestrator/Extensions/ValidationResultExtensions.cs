﻿using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NetworkPerspective.Sync.Orchestrator.Extensions;

public static class ValidationResultExtensions
{
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}