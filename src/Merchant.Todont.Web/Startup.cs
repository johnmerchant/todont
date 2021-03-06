using System.IO;
using System.Text;
using HotChocolate;
using HotChocolate.Execution.Options;
using Merchant.Todont.Api;
using Merchant.Todont.Domain;
using Merchant.Todont.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Merchant.Todont.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging()
                .AddDomain()
                .AddInfrastructure(Configuration)
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(File.ReadAllText(Configuration["Jwt:Key"])))
                    };
                });
            
            services
                .AddRouting()
                .AddAuthorization()
                .AddGraphQLServer()
                .ConfigureSchema(builder => builder.AddApi().Create().MakeExecutable(new RequestExecutorOptions
                {
                    IncludeExceptionDetails = true 
                }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app
                .UseRouting()
                .UseEndpoints(endpoints => endpoints.MapGraphQL());
        }
    }
}