namespace Azrng.JSqlParser.Util.Validation;

/// <summary>
/// Represents a validation error found during SQL validation.
/// </summary>
public class ValidationError
{
    public string Message { get; set; } = "";
    public FeaturesAllowed? RequiredFeature { get; set; }

    public ValidationError() { }

    public ValidationError(string message)
    {
        Message = message;
    }

    public ValidationError(string message, FeaturesAllowed requiredFeature)
    {
        Message = message;
        RequiredFeature = requiredFeature;
    }

    public override string ToString() => Message;
}
