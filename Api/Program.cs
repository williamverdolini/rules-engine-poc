using Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IRulesEngineStore, RulesEngineLocalJsonStore>();
builder.Services.AddSingleton<InMemoryProvider>();
builder.Services.AddSingleton<RulesResolver>();
builder.Services.AddSingleton<RulesEngineWrapper>();
builder.Services.AddSingleton<BusinessRulesResolver>();

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
