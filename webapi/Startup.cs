using StxDemo;
using Microsoft.OpenApi.Models;
using STX.Sdk.Api.Services;
using System.Text.Json.Serialization.Metadata;

namespace STX.Sdk.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add STX Services to service collection (to DI container)
            // Picks API-key or email/password based on the environment. See shared/StxAuth.cs.
            services.AddStx();

            //Add simple services
            services.AddSingleton<SimpleActiveOrdersChannelWrapper>();
            services.AddSingleton<SimpleActiveSettlementsChannelWrapper>();
            services.AddSingleton<SimpleActiveTradesChannelWrapper>();
            services.AddSingleton<SimpleMarketChannelWrapper>();
            services.AddSingleton<SimplePortfolioChannelWrapper>();
            services.AddSingleton<SimplePositionsChannelWrapper>();
            services.AddSingleton<SimpleUserInfoChannelWrapper>();

            // Resolve the API key's user id before any user-scoped channel is asked for.
            services.AddHostedService<StxIdentityWarmup>();

            services.AddTransient<STXWorker>();

            // Prices, amounts and quantities go out as strings ("0.5600", "2.00"), not cents.
            services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
                {
                    Modifiers = { AmountStringsJsonModifier.Apply }
                });

            // Configure swagger.
            services.AddSwaggerGen(doc =>
            {
                doc.SwaggerDoc("v1", new OpenApiInfo { Title = "STX Test Api", Version = "v1", Description = "STX Sdk Wrapper testing Api" });

                doc.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                doc.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] { }
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(endpoint =>
            {
                endpoint.SwaggerEndpoint("/swagger/v1/swagger.json", "STX Sdk Wrapper Test Api");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
