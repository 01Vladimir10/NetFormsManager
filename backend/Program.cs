using FluentValidation;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi;
using NetFormsManager.Api;
using NetFormsManager.Api.Validators;
using NetFormsManager.Configuration;
using NetFormsManager.Core.Services;
using NetFormsManager.Endpoints;
using NetFormsManager.Infrastructure.Email;
using NetFormsManager.Infrastructure.Templates;
using NetFormsManager.Infrastructure.TokenValidators;
using NetFormsManager.Middleware;
using NetFormsManager.Utils;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddScoped<AuthMiddleware>();
builder.Services.AddSingleton<ExceptionMiddleware>();
builder.Services.AddSingleton<ITemplateRendererService, MustacheTemplateService>();
builder.Services.AddTransient<IEmailService, EmptyEmailService>();
builder.Services.AddLogging(x =>
{
    x.AddConfiguration(builder.Configuration.GetSection("Logging"));
    x.AddConsole();
    x.SetMinimumLevel(LogLevel.Trace);
    x.Configure(options => options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId
                                                             | ActivityTrackingOptions.TraceState |
                                                             ActivityTrackingOptions.TraceFlags);
});
builder.Services.AddEmailService(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddSubscriptions(builder.Configuration);

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.Duration | HttpLoggingFields.RequestMethod |
                            HttpLoggingFields.RequestPath | HttpLoggingFields.RequestQuery;
    options.CombineLogs = true;
});
// Register HttpClientFactory for token validators
builder.Services.AddHttpClient();

// Register token validators
builder.Services.AddTokenValidationFactory(registry =>
{
    registry.RegisterValidator<GoogleEnterpriseReCaptchaBotValidator>("EnterpriseRecaptcha");
    registry.RegisterValidator<GoogleReCaptchaBotValidator>("Recaptcha");
    registry.RegisterValidator<HCaptchaBotValidator>("HCaptcha");
    registry.RegisterValidator<CloudflareTurnstileBotValidator>("CloudflareTurnstile");
});

// Register FluentValidation validators
ValidatorOptions.Global.PropertyNameResolver = CamelCasePropertyNameResolver.ResolvePropertyName;
ValidatorOptions.Global.DisplayNameResolver = CamelCasePropertyNameResolver.ResolvePropertyName;
builder.Services.AddScoped<IValidator<FormRequestDto>, FormRequestDtoValidator>();
builder.Services.AddScoped<IValidator<EmailTemplateRequestDto>, EmailTemplateRequestDtoValidator>();


builder.Services.AddManagedOpenApi();
builder.Services.AddManagedCors(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi().AllowAnonymous();
app.MapScalarApiReference(x => x.PersistentAuthentication = true).AllowAnonymous();
app.UseHttpLogging();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<AuthMiddleware>();

// do not enable cors for these endpoints.
app.MapFormPublicEndpoints();
app.UseCors();
app.MapFormEndpoints();
app.MapEmailTemplateEndpoints();
app.MapProvidersEndpoints();
app.MapSubscriberEndpoints();

await app.RunAsync();