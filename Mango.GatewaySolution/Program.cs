using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppAuthentication();
//if (builder.Environment.EnvironmentName.ToString().ToLower().Equals("production"))
//{
//    builder.Configuration.AddJsonFile("ocelot.Production.json", optional: false, reloadOnChange: true);
//}
//else
//{
//    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
//}
//builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddOcelot();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseOcelot().GetAwaiter().GetResult();
app.Run();


/*using Mango.GatewaySolution.Extensions;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppAuthentication();
//builder.Services.AddAuthentication("Bearer")
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.Authority = "https://localhost:7002";
//        options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
//    });
builder.Services.AddOcelot();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseOcelot().GetAwaiter().GetResult();
app.UseAuthorization();
app.MapControllers();   
app.Run();*/
