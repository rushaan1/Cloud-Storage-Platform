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
        private readonly List<HttpResponse> _clients = new(); 

        public void AddClient(HttpResponse response)
        {
            lock (_clients)
            {
                _clients.Add(response);
            }
        }

        public void RemoveClient(HttpResponse response)
        {
            lock (_clients)
            {
                _clients.Remove(response);
            }
        }

        public async Task SendEventAsync(string eventType, object data)
        {
            var json = JsonSerializer.Serialize(
                new 
                { eventType = eventType,
                    content = data
                }, 
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            Console.WriteLine(json);
            lock (_clients)
            {
                _clients.RemoveAll(client => {
                    if (!client.HttpContext.Response.Body.CanWrite) 
                    {
                        Console.WriteLine("don't work");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("do work");
                        return false;
                    }
                });
            }

            var tasks = _clients.Select(async client =>
            {
                try
                {
                    await client.WriteAsync("data: "+json+"\n\n");
                    await client.Body.FlushAsync();
                }
                catch
                {
                    RemoveClient(client);
                }
            });

            await Task.WhenAll(tasks);
        }
    }

}
