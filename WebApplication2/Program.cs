
using WebApplication2.Services;
using static WebApplication2.TestServices;

namespace WebApplication2
{
    [TestAttribute("ProgramT")]
    public class Program
    {
        [TestAttribute("MainT")]
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddTransient<ITransientService, TransientService>();
            builder.Services.AddScoped<IScopedService, ScopedService>();
            builder.Services.AddSingleton<ISingletonService, SingletonService>();

            builder.Services.AddHttpClient();
            builder.Services.AddHostedService<ElectricityPriceFetchingService>();

            builder.Services.AddHttpClient("DatabaseServiceClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7122/");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}