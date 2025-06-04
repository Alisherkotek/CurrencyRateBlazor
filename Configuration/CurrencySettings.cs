namespace BlazorTestApp.Configuration;

public class CurrencySettings
{
    public string ApiUrl { get; set; } = string.Empty;
    public int UpdateIntervalMinutes { get; set; } = 5;
    public int DefaultTimeZoneOffset { get; set; } = 5;
    public string DefaultTimeZoneName { get; set; } = "Astana Time";
}