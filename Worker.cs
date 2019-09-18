using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace WorkerServiceDemo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _httpClient = _httpClientFactory.CreateClient();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await _httpClient.GetAsync("http://api.icndb.com/jokes/random", stoppingToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsAsync<JToken>(stoppingToken);
                    _logger.LogInformation($"{content["value"]["joke"]}");
                }
                else
                {
                    _logger.LogError($"The website is down. Status code {response.StatusCode}");
                }

                var millisecondsDelay = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
                await Task.Delay(millisecondsDelay, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            // no need to dispose the http client here since its lifetime is tracked by HttpClientFactory
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#httpclient-and-lifetime-management

            return base.StopAsync(cancellationToken);
        }
    }
}