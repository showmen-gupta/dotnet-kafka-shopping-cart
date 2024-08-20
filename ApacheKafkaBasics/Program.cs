using ApacheKafkaBasics.Configuration;

var builder = WebApplication.CreateBuilder(args);
// Configure services
KafkaConfiguration.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
KafkaConfiguration.ConfigureMiddleware(app);

app.Run();