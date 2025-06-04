using BlazorTestApp.Configuration;

namespace BlazorTestApp.Services;

public class CurrencyUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CurrencyUpdateService> _logger;
    private readonly CurrencySettings _settings;
    private readonly TimeSpan _updateInterval;

    public CurrencyUpdateService(
        IServiceProvider serviceProvider,
        ILogger<CurrencyUpdateService> logger,
        CurrencySettings settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings;
        _updateInterval = TimeSpan.FromMinutes(settings.UpdateIntervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Currency Update Service started with interval: {Interval} minutes",
            _settings.UpdateIntervalMinutes);

        await UpdateCurrencies();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_updateInterval, stoppingToken);
                await UpdateCurrencies();
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in currency update loop, will retry in {Minutes} minutes",
                    _settings.UpdateIntervalMinutes);
            }
        }

        _logger.LogInformation("Currency Update Service stopped");
    }

    private async Task UpdateCurrencies()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyService>();

            _logger.LogInformation(
                "Running scheduled currency update at {Time} ({TimeZone})",
                DateTime.UtcNow.AddHours(_settings.DefaultTimeZoneOffset).ToString("yyyy-MM-dd HH:mm:ss"),
                _settings.DefaultTimeZoneName);

            var success = await currencyService.UpdateCurrencyRatesAsync();

            if (success)
            {
                _logger.LogInformation("Scheduled currency update completed successfully");
            }
            else
            {
                _logger.LogWarning("Scheduled currency update failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in UpdateCurrencies");
        }
    }
}