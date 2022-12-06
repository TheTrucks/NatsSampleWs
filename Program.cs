using Microsoft.AspNetCore.WebSockets;
using NATS_WS.Services;

namespace NATS_WS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.Configure<NatsReaderOptions>(builder.Configuration.GetSection("NatsReaderOptions"));
            builder.Services.Configure<WSSenderOptions>(builder.Configuration.GetSection("WsWriterOptions"));
            builder.Services.AddSingleton<NatsReader>();
            builder.Services.AddSingleton<WSSender>();

            var app = builder.Build();

            app.UseRouting()
                .UseCors(x => x.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod());
            app.UseWebSockets();
            app.MapControllers();

            app.Run();
        }
    }
}