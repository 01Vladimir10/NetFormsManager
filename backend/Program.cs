using FluentValidation;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi;
using NetMailGun.Api;
using NetMailGun.Api.Validators;
using NetMailGun.Configuration;
using NetMailGun.Core.Services;
using NetMailGun.Endpoints;
using NetMailGun.Infrastructure.Email;
using NetMailGun.Infrastructure.Templates;
using NetMailGun.Infrastructure.TokenValidators;
using NetMailGun.Middleware;
using NetMailGun.Utils;
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

builder.Services.AddCors();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, context, _) =>
    {
        var provider = context.ApplicationServices.GetService<IAuthenticationOpenApiConfigurationProvider>();
        if (provider is null) return Task.CompletedTask;
        doc.Components ??= new OpenApiComponents();
        doc.Components.SecuritySchemes = provider.GetSecuritySchemes();
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.ConfigureCors(builder.Configuration);
app.MapOpenApi().AllowAnonymous();
app.MapScalarApiReference(x => x.PersistentAuthentication = true).AllowAnonymous();
app.UseHttpLogging();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<AuthMiddleware>();

app.MapFormEndpoints();
app.MapEmailTemplateEndpoints();
app.MapFormPublicEndpoints();
app.MapProvidersEndpoints();
app.MapSubscriberEndpoints();

await app.RunAsync();