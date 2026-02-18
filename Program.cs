using Microsoft.EntityFrameworkCore;
using SalesForceSync.Data;
using SalesForceSync.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Salesforce services
builder.Services.AddHttpClient<SalesforceAuthService>();
builder.Services.AddHttpClient<SalesforceContactService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();