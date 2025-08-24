using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        protected readonly Claim adminClaim = new Claim("esAdmin", "1");

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

        protected async Task<string> CrearUsuario(string nombreDB, WebApplicationFactory<Program> factory)
            => await CrearUsuario(nombreDB, factory, [], "ejemplo@hotmail.com");

        protected async Task<string> CrearUsuario(string nombreDB, WebApplicationFactory<Program> factory, IEnumerable<Claim> claims)
            => await CrearUsuario(nombreDB, factory, claims, "ejemplo@hotmail.com");

        protected async Task<string> CrearUsuario(string nombreDB, WebApplicationFactory<Program> factory, IEnumerable<Claim> claims, string email)
        {
            var urlRegistro = "api/v1/usuarios/registro";
            string token = string.Empty;

            token = await ObtenerToken(email, urlRegistro, factory);

            if (claims.Any())
            {
                var context = ConstruirContext(nombreDB);
                var usuario = await context.Users.Where(x => x.Email == email).FirstAsync();
                Assert.IsNotNull(usuario);
                var userClaims = claims.Select(x  => new Microsoft.AspNetCore.Identity.IdentityUserClaim<string>
                {
                    UserId = usuario.Id,
                    ClaimType = x.Type,
                    ClaimValue = x.Value
                });

                context.UserClaims.AddRange(userClaims);
                await context.SaveChangesAsync();
                var urlLogin = "api/v1/usuarios/login";
                token = await ObtenerToken(email, urlLogin, factory);
            }

            return token;
        }

        private async Task<string> ObtenerToken(string email, string url, WebApplicationFactory<Program> factory)
        {
            var password = "aA123456!";
            var credenciales = new
            {
                Email = email,
                Password = password
            };
            var cliente = factory.CreateClient();
            var respuesta = await cliente.PostAsJsonAsync(url, credenciales);
            respuesta.EnsureSuccessStatusCode();

            var contenido = await respuesta.Content.ReadAsStringAsync();
            var respuestaAutenticacion = JsonSerializer.Deserialize<RespuestaAutenticacionDTO>(contenido, jsonSerializerOptions)!;

            Assert.IsNotNull(respuestaAutenticacion.Token);
            return respuestaAutenticacion.Token;
        }
    }
}
