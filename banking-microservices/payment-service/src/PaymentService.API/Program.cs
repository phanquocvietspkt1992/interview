using PaymentService.Application.Commands.InitiatePayment;
using PaymentService.Domain.Exceptions;
using PaymentService.Infrastructure;
using PaymentService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(InitiatePaymentCommand).Assembly));

var app = builder.Build();

// Initialize CosmosDB database + container on startup
using (var scope = app.Services.CreateScope())
{
    var cosmos = scope.ServiceProvider.GetRequiredService<CosmosDbContext>();
    await cosmos.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        (int statusCode, string message) = error switch
        {
            DomainException ex => (StatusCodes.Status422UnprocessableEntity, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.MapControllers();
app.Run();
