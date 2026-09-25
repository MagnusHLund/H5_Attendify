namespace Attendify.Features.Settings;

public sealed class SettingsGroup : Group
{
    public SettingsGroup()
    {
        Configure("settings", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}
