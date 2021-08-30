using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExercicioAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ExercicioAPI
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
      string securityKey = "my_second_API-Now_about_BOOKS!";
      var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

      services.AddControllers();
      services.AddDbContext<AppDbContext>(options =>
      {
        options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
      });
      services.AddSwaggerGen(conf =>
      {
        conf.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Favorite books API", Version = "v1" });
      });
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = "BookAPI",
          ValidAudience = "common_user",
          IssuerSigningKey = symmetricKey,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero
        };
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

      app.UseRouting();

      app.UseAuthentication();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });

      app.UseSwagger(conf =>
      {
        conf.RouteTemplate = "favBooks/{documentName}/swagger.json";
      });

      app.UseSwaggerUI(config => //Gera os Views HTML do Swagger
      {
        config.SwaggerEndpoint("/favBooks/v1/swagger.json", "v1 docs");
      });
    }
  }
}
