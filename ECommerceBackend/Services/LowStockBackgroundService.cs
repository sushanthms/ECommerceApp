using ECommerceBackend.Data;
using ECommerceBackend.Hubs;
using ECommerceBackend.Models;
using ECommerceBackend.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services
{
    public class LowStockBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotificationHub> _hub;

        public LowStockBackgroundService(IServiceScopeFactory scopeFactory, IHubContext<NotificationHub> hub)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var since = DateTime.UtcNow.AddHours(-24);

                    var alreadyNotified = await db.Notifications
                        .Where(n => n.Type == "LowStock" && n.CreatedAt > since && n.ProductId != null)
                        .Select(n => n.ProductId!.Value)
                        .ToListAsync(stoppingToken);

                    var lowStock = await db.Products
                        .Where(p => p.Stock <= 5 && !p.IsDeleted && !alreadyNotified.Contains(p.Id))
                        .Select(p => new { p.Id, p.Name, p.Stock })
                        .ToListAsync(stoppingToken);

                    var created = new List<Notification>();

                    foreach (var p in lowStock)
                    {
                        created.Add(new Notification
                        {
                            Type = "LowStock",
                            ProductId = p.Id,
                            Message = $"Low stock: {p.Name} - {p.Stock} remaining"
                        });
                    }

                    if (created.Count > 0)
                    {
                        db.Notifications.AddRange(created);
                        await db.SaveChangesAsync(stoppingToken);// now each message has an Id

                        foreach (var n in created)
                        {
                            await _hub.Clients.All.SendAsync("ReceiveNotification",// sends notification to all connected SignalR client
                                new { n.Id, n.Type, n.Message, n.IsRead, n.CreatedAt }, stoppingToken);
                        }
                    }
                }

                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    using var errorScope = _scopeFactory.CreateScope();
                    var appLogger = errorScope.ServiceProvider.GetRequiredService<IApplicationLogger>();
                    await appLogger.LogMessageAsync($"Low stock check failed: {ex.Message}", "Error");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}