using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureKeyVaultExample.Data;
using AzureKeyVaultExample.Models;
using AzureKeyVaultExample.Routes;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure KeyVault access.
builder.Host.ConfigureAppConfiguration((context, config) =>
{
    IConfiguration configuration = config.Build();

    string kvURL = configuration["KeyVaultConfig:KVUrl"];
    string tenantId = configuration["KeyVaultConfig:TenantId"];
    string clientId = configuration["KeyVaultConfig:ClientId"];
    string clientSecret = configuration["KeyVaultConfig:ClientSecretId"];

    var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

    var client = new SecretClient(new Uri(kvURL), credential);
    config.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database SqlServer
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration.GetConnectionString("SqlServerConnectionStrings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(op =>
    {
        op.RoutePrefix = string.Empty;
        op.SwaggerEndpoint("swagger/v1/swagger.json", "Minimal API");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapMethods(ProductGetAll.Template, ProductGetAll.Methods, ProductGetAll.Handle)
    .Produces<Product>(StatusCodes.Status200OK);

app.MapMethods(ProductPost.Template, ProductPost.Methods, ProductPost.Handle)
    .Produces<Guid>(StatusCodes.Status201Created);

app.Run();
