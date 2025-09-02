using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.BackgroundServices;

public class PolicyExpirationChecker : BackgroundService
{
    private readonly ILogger<PolicyExpirationChecker> _logger;
    private readonly IServiceProvider _services;

    public PolicyExpirationChecker(ILogger<PolicyExpirationChecker> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("PolicyExpirationChecker is running.");

            await CheckForExpiredPoliciesAsync();

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task CheckForExpiredPoliciesAsync()
    {
        using (var scope = _services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var oneHourAgo = DateOnly.FromDateTime(DateTime.Today.AddHours(-1));
            var expiredPolicies = await dbContext.Policies
                .Where(p => p.EndDate == oneHourAgo)
                .ToListAsync();

            if (expiredPolicies.Any())
            {
                foreach (var policy in expiredPolicies)
                {
                    _logger.LogInformation($"Policy {policy.Id} for Car {policy.CarId} expired on {policy.EndDate}.");
                }
            }
        }
    }
}