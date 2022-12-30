using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using ShiftPlanner.Services;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Api;

public class Startup
{
    private readonly string _myAllowSpecificOrigins = "_myAllowSpecificOrigins";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<StorageOptions>().Bind(Configuration.GetSection(StorageOptions.SectionName)).ValidateDataAnnotations();
        
        services.AddCors(
            options =>
            {
                options.AddPolicy(
                    name: _myAllowSpecificOrigins,
                    policy =>
                    {
                        policy
                            .WithOrigins(
                                "http://localhost:3000",
                                "https://krtek.me",
                                "https://www.krtek.me",
                                "https://*.d13dc8ck6htkvt.amplifyapp.com")
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .WithHeaders(
                                HeaderNames.Authorization,
                                HeaderNames.ContentType
                            )
                            .WithMethods("GET", "POST", "PUT", "DELETE");
                    });
            });

        // https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/01-authorization
        string domain = $"https://{Configuration["Auth0:Domain"]}/";
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                options =>
                {
                    options.Authority = domain;
                    options.Audience = Configuration["Auth0:Audience"];
                    // If the access token does not have a `sub` claim, `User.Identity.Name` will be `null`. Map it to a different claim by setting the NameClaimType below.
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

        services.AddAuthorization(
            options =>
            {
                options.AddPolicy(Claims.ReadRequirementsOfOthers, policy => policy.RequireClaim("permissions", Claims.ReadRequirementsOfOthers));
                options.AddPolicy(Claims.ApproveRequirementsOfOthers, policy => policy.RequireClaim("permissions", Claims.ApproveRequirementsOfOthers));
            });

        services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();


        services.AddControllers();

        services.AddServices();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors(_myAllowSpecificOrigins);

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllers();
            });
    }
}
