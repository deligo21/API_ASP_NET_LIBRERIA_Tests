using AutoMapper;
using BibliotecaAPI.Datos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BibliotecaAPITest.Utilidades
{
    public class BasePruebas
    {
        protected readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        protected ApplicationDbContext ConstruirContext(string nombreBD)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreBD)
                .Options;
            var dbCcontext = new ApplicationDbContext(opciones);
            return dbCcontext;
        }

        protected IMapper ConfigurarAutoMapper()
        {
            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile(new BibliotecaAPI.Utilidades.AutoMapperProfiles());

            var configuration = new MapperConfiguration(configExpression, loggerFactory: null);

            return configuration.CreateMapper();
        }

        protected WebApplicationFactory<Program> ConstruirWebApplicationFactory(string nombreBD, bool ignorarSeguridad = true)
        {
            var factory = new WebApplicationFactory<Program>();
            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    ServiceDescriptor descriptorDBContext = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>))!;
                    if (descriptorDBContext is not null)
                    {
                        services.Remove(descriptorDBContext);
                    }

                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nombreBD));

                    if (ignorarSeguridad)
                    {
                        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(options =>
                        {
                            options.Filters.Add(new UsuarioFalsoFiltro());
                        });
                    }
                });
            });
            return factory;
        }
    }
}
