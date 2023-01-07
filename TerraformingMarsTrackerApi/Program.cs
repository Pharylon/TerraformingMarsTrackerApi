using TerraformingMarsTrackerApi;
using TerraformingMarsTrackerApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
#if DEBUG
//builder.WebHost.UseUrls("http://0.0.0.0:33602");
#endif
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<GameStore>();
builder.Services.Configure<CosmosConfig>(builder.Configuration.GetSection("CosmosConfig"));
builder.Services.AddSingleton<CosmosDbClient>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebSockets();
}

#if !DEBUG
app.UseHttpsRedirection();
#endif


app.UseAuthorization();

app.MapControllers();
app.MapHub<TfmHub>("/tfmHub");

app.Run();
