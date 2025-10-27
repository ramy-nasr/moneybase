using Infrastructure.Seed;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services
{
    public sealed class SeedDataHostedService(DataSeeder seeder) : IHostedService
    {
        private readonly DataSeeder _seeder = seeder;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _seeder.SeedAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
