namespace Opti_Sec_Backend.Errors;

public static class GateErrors
{
    public static readonly Error NotFound =
           new("Gate.NotFound", "Gate is Not Found", StatusCodes.Status400BadRequest);

    public static readonly Error DuplicateName =
          new("Gate.DuplicateName", "the Name Of the Gate is Exist", StatusCodes.Status400BadRequest);
}
