using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Sockets;
using WebApp.Data;

namespace E2ETests.ApproachC.Testcontainers.Infrastructure;

public sealed class WebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly int _port;

    public string BaseUrl => $"http://127.0.0.1:{_port}";

    public WebAppFactory(string connectionString)
    {
        _connectionString = connectionString;
        _port = GetFreePort();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls($"http://127.0.0.1:{_port}");

        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(s => s.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();
            toRemove.ForEach(d => services.Remove(d));

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_connectionString,
                    sql => sql.EnableRetryOnFailure(3)));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // WebApplicationFactory defaults to in-memory TestServer.
        // Playwright needs real Kestrel so the browser can reach it.
        builder.ConfigureWebHost(b => b.UseKestrel());
        var host = builder.Build();
        host.Start();
        return host;
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
