using ECommerceBackend.Data;
using ECommerceBackend.Hubs;
using ECommerceBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;
        private const int LowStockThreshold = 5;
        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task<List<Notification>> GetLatestAsync() =>
            await _context.Notifications.AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync() =>
            await _context.Notifications.CountAsync(n => !n.IsRead);

        public async Task MarkReadAsync(int id) =>
            await _context.Notifications.Where(n => n.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        public async Task MarkAllReadAsync() =>
            await _context.Notifications.Where(n => !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        public async Task NotifyIfLowStockAsync(int productId, string productName, int oldStock, int newStock)
        {
            if (!(oldStock > LowStockThreshold && newStock <= LowStockThreshold))
                return;

            var notification = new Notification
            {
                Type = "LowStock",
                ProductId = productId,
                Message = $"Low stock: {productName} - {newStock} remaining"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ReceiveNotification",
                new { notification.Id, notification.Type, notification.Message, notification.IsRead, notification.CreatedAt });
        }
    }
}