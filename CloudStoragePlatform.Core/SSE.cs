using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core
{
    public class SSE
    {
        // Now stores (HttpResponse, Guid userId)
        private readonly List<(HttpResponse Response, Guid UserId)> _clients = new();

        public void AddClient(HttpResponse response, Guid userId)
        {
            lock (_clients)
            {
                _clients.Add((response, userId));
            }
        }

        public void RemoveClient(HttpResponse response)
        {
            lock (_clients)
            {
                _clients.RemoveAll(c => c.Response == response);
            }
        }

        // Only send to clients matching userId
        public async Task SendEventAsync(string eventType, object data, Guid userId)
        {
            var json = JsonSerializer.Serialize(
                new
                {
                    eventType = eventType,
                    content = data
                },
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            Console.WriteLine(json);
            lock (_clients)
            {
                _clients.RemoveAll(client =>
                {
                    return client.Response.HttpContext.RequestAborted.IsCancellationRequested;
                });
            }

            var tasks = _clients
                .Where(client => client.UserId == userId)
                .Select(async client =>
                {
                    try
                    {
                        await client.Response.WriteAsync("data: " + json + "\n\n");
                        await client.Response.Body.FlushAsync();
                    }
                    catch
                    {
                        RemoveClient(client.Response);
                    }
                });
            await Task.WhenAll(tasks);
        }
    }
}
