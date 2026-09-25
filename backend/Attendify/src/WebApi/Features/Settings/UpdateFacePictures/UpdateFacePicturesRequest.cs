namespace Attendify.Features.Settings.UpdateFacePictures;

public sealed record UpdateFacePicturesRequest
{
    public string StraightPhoto { get; init; } = string.Empty;
    public string LeftPhoto { get; init; } = string.Empty;
    public string RightPhoto { get; init; } = string.Empty;
}
