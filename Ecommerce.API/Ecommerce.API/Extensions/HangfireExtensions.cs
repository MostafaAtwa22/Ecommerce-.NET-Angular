
namespace Ecommerce.API.Extensions
{
    public static class HangfireExtensions
    {
        public static WebApplication UseHangfireJobs(this WebApplication app)
        {
            app.UseHangfireDashboard();
            app.MapHangfireDashboard("/hangfire");

            // Setup recurring jobs
            using var scope = app.Services.CreateScope();
            var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

            recurringJobManager.AddOrUpdate(
                "clean-refresh-tokens",
                () => tokenService.CleanExpiredTokens(),
                Cron.Weekly
            );

            recurringJobManager.AddOrUpdate<IProductService>(
                "clean-expired-discounts",
                x => x.CleanExpiredDiscountsAsync(),
                Cron.Daily
            );

            return app;
        }
    }
}
