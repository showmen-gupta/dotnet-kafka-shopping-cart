using ApacheKafkaBasics.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secret.json", true, true);
// Registering configuration section as a strongly typed class
builder.Services.Configure<KafkaConfigSecrets>(builder.Configuration.GetSection("KafkaConfig"));
// Configure services
KafkaConfiguration.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
// Configure the HTTP request pipeline
KafkaConfiguration.ConfigureMiddleware(app);

app.Run();