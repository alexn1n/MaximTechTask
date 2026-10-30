using MaximTech.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.RespectRequiredConstructorParameters = true);
builder.Services.AddDriverServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ParallelLimitMiddleware>();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "MaximTech"));

app.UseHttpsRedirection();

app.MapDriverEndpoints();

app.Run();
