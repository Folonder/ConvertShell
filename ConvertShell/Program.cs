using ConvertShell.Infrastructure;
using ConvertShell.Middleware;
using ConvertShell.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddScoped<MetaData, MetaData>();
builder.Services.AddScoped<ConvertioClient, ConvertioClient>();
builder.Services.AddScoped<ConvertioContentBuilder, ConvertioContentBuilder>();
builder.Services.AddScoped<IConvertService, ConvertService>();
builder.Services.AddScoped<IConverter, ConvertioConverter>();
builder.Services.Configure<ConvertioConverterOptions>(builder.Configuration.GetSection(ConvertioConverterOptions.Key));


builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
