using Beltzac.AIPlay.App.Api.Contract;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beltzac.AIPlay.App.Api.Hubs
{
    public class LipHub : Hub
    {
        public Task RegisterForProcessingStatusUpdate(string message)
        {
            Console.WriteLine($"From signalr -> {message}");
            return Task.CompletedTask;
        }

        public Task OnUpdateProcessingStatus(StatusUpdate message)
        {
            Console.WriteLine($"Message received -> {message.ProcessId} {message.PercentageProcessed} {message.Message}");
            return Clients.All.SendAsync("OnUpdateProcessingStatus", message);
        }
    }
}
