using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjetoBanco.Api.Data;
using ProjetoBanco.Api.Messaging;
using Moq;

namespace ProjetoBanco.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove Oracle e substitui por InMemory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase("TestDb"));

            // Mock do RabbitMQ
            var mockRabbit = new Mock<IRabbitMqService>();
            mockRabbit.Setup(r => r.Publicar(It.IsAny<string>(), It.IsAny<string>()));

            var descriptorRabbit = services.SingleOrDefault(
                d => d.ServiceType == typeof(IRabbitMqService));
            if (descriptorRabbit != null) services.Remove(descriptorRabbit);

            services.AddSingleton(mockRabbit.Object);
        });
    }
}