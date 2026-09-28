namespace Attendify.Common.Domain.FacialRecognition;

public static class FacialErrors
{
    public static readonly Error NotFound = Error.NotFound(
            "FacialProfile.NotFound",
            "Facial profile is not found");

    public static readonly Error InvalidBase64 = Error.Validation(
            "FacialEncoding.Validation",
            "Facial encoding is not valid"
            );

}