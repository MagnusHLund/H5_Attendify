namespace Attendify.Features.Settings.ExportPersonalData;

public sealed class ExportPersonalDataSummary : Summary<ExportPersonalDataEndpoint>
{
    public ExportPersonalDataSummary()
    {
        Summary = "Exports the authenticated user's personal data";
        Description =
            "Returns the user's account details, attendance events and access-code metadata as a downloadable JSON attachment. Facial embeddings and access-code hashes are not included.";
        Response<ExportPersonalDataResponse>(200, "The personal data export, sent as an attachment.");
        Response(401, "The request is not authenticated or the account has been deleted.");
        Response(403, "Only student sessions can use this endpoint.");
    }
}
