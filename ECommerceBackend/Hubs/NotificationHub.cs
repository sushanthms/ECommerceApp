using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ECommerceBackend.Hubs
{
    [Authorize(Roles = "Admin")]
    public class NotificationHub : Hub
    {
    }
}