using MapMarkers.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
       .AddJsonOptions(opt =>
       {
           // camelCase JSON for the frontend
           opt.JsonSerializerOptions.PropertyNamingPolicy =
               System.Text.Json.JsonNamingPolicy.CamelCase;
       });

// Register the ADO.NET repository as a scoped service
builder.Services.AddScoped<MarkerRepository>();

// Serve static files from wwwroot (index.html, CSS, JS)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────
app.UseDefaultFiles();       // serves index.html for "/"
app.UseStaticFiles();        // serves wwwroot/

app.UseRouting();
app.MapControllers();

app.Run();
