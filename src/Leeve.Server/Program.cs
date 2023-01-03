//=========================================//
// default db port      : 20050            //
// default db name      : leeve-db         //
// default grpc port    : 2701             //
// config variable      : LEEVE_SERVICE     //
//=========================================//

using System.Runtime.InteropServices;
using Leeve.Server.Common;
using Leeve.Server.Evaluations;
using Leeve.Server.Questionnaires;
using Leeve.Server.Users;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options =>
{
    var credentials = Credentials.Get();

    options.Listen(IPAddress.Any, credentials?.Server ?? 2701, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddHostedService<StartupService>();
builder.Services.AddSingleton<TeacherNotifier>();
builder.Services.AddSingleton<TeacherImageNotifier>();
builder.Services.AddSingleton<QuestionnaireNotifier>();
builder.Services.AddSingleton<EvaluationNotifier>();
builder.Services.AddSingleton<EvaluationProcessNotifier>();
builder.Services.AddSingleton<EvaluationSubmitNotifier>();
builder.Services.AddSingleton<ILogger>(_ =>
{
    var basePath = string.Empty;

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        basePath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.leeve");

    var logPath = Path.Combine(basePath, "logs");
    return new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File(Path.Combine(logPath, "leeve-logs.txt"), rollingInterval: RollingInterval.Day)
        .CreateLogger();
});

if (builder.Environment.IsDevelopment())
    builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AdminService>();
app.MapGrpcService<TeacherNotifier>();
app.MapGrpcService<TeacherService>();
app.MapGrpcService<TeacherImageService>();
app.MapGrpcService<TeacherImageNotifier>();
app.MapGrpcService<QuestionnaireService>();
app.MapGrpcService<QuestionnaireNotifier>();
app.MapGrpcService<EvaluationService>();
app.MapGrpcService<EvaluationNotifier>();
app.MapGrpcService<EvaluationProcessNotifier>();
app.MapGrpcService<EvaluationSubmitNotifier>();

if (builder.Environment.IsDevelopment())
    app.MapGrpcReflectionService();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

await app.RunAsync();