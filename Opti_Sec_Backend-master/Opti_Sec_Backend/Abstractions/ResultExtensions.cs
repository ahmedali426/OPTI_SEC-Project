using Microsoft.AspNetCore.Mvc;

namespace Opti_Sec_Backend.Abstractions;

public static class ResultExtensions
{
    public static ObjectResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert success result to a problem");
        // to use the ProblemDetails and To recieve the statusCode
        var problem = Results.Problem(statusCode: result.Error.StatusCode);
        // we do this line because we can not recieve the ProblemDetails Property directly 
        // we do as ProblemDetails because we can not use the Extensions 
        var problemDetails = problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails;

        problemDetails!.Extensions = new Dictionary<string, object?>
        {
            {
                "errors",
                new[]
                    {
                         new
                         {
                             result.Error.Code,
                             result.Error.Description
                         }
                    }

            }
        };

        return new ObjectResult(problemDetails);
    }
}
