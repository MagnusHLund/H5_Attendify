namespace Attendify.Features.Settings.UpdateFacePictures;

public sealed class UpdateFacePicturesSummary : Summary<UpdateFacePicturesEndpoint>
{
    public UpdateFacePicturesSummary()
    {
        Summary = "Updates the authenticated student's face photos";
        Description =
            "Validates three new face photos, replaces the student's stored facial embeddings, and permanently removes the previous embeddings.";
        Response(204, "The face photos and embeddings were replaced.");
        Response(400, "One or more photos are invalid or do not contain exactly one face.");
        Response(401, "The request is not authenticated.");
        Response(403, "Only students can update face photos.");
        Response(404, "The authenticated student's facial profile was not found.");
    }
}
