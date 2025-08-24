using BibliotecaAPI.DTOs;
using BibliotecaAPITest.Utilidades;
using System.Security.Claims;
using System.Text.Json;

namespace BibliotecaAPITest.PruebasDeIntegracion.Controllers.V1
{
    public class AutoresControllerPruebas: BasePruebas
    {
        private static readonly string url = "/api/v1/autores";
        private string nombreBD = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Get_Devuelve404_CuandoAutorNoExiste()
        {
            // Preparación
            var factory = ConstruirWebApplicationFactory(nombreBD);
            var cliente = factory.CreateClient();

            // Prueba

            var respuesta = await cliente.GetAsync($"{url}/1");

            // Verificación
            var status = respuesta.StatusCode;
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, status);
        }

        [TestMethod]
        public async Task Get_DevuelveAutor_CuandoAutorExiste()
        {
            // Preparación
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new BibliotecaAPI.Entidades.Autor()
            {
                Nombres = "Rodrigo",
                Apellidos = "Rojas"
            });
            context.Autores.Add(new BibliotecaAPI.Entidades.Autor()
            {
                Nombres = "Luciana",
                Apellidos = "Vargas"
            });
            await context.SaveChangesAsync();
            var factory = ConstruirWebApplicationFactory(nombreBD);
            var cliente = factory.CreateClient();

            // Prueba

            var respuesta = await cliente.GetAsync($"{url}/1");

            // Verificación
            respuesta.EnsureSuccessStatusCode();

            var autor = JsonSerializer.Deserialize<AutorConLibrosDTO>(
                await respuesta.Content.ReadAsStringAsync(),
                jsonSerializerOptions
            )!;

            Assert.AreEqual(expected: 1, autor.Id);
        }

        [TestMethod]
        public async Task Post_Devuelve401_CuandoUsuarioNoEstaAutenticado()
        {
            // Preparación
            var factory = ConstruirWebApplicationFactory(nombreBD, false);
            var cliente = factory.CreateClient();
            var autorCreacionDTO = new AutorCreacionDTO()
            {
                Nombres = "Rodrigo",
                Apellidos = "Rojas",
                Identificacion = "123"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            // Verificación
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Devuelve403_CuandoUsuarioNoEsAdmin()
        {
            // Preparación
            var factory = ConstruirWebApplicationFactory(nombreBD, false);
            var token = await CrearUsuario(nombreBD, factory);

            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var autorCreacionDTO = new AutorCreacionDTO()
            {
                Nombres = "Rodrigo",
                Apellidos = "Rojas",
                Identificacion = "123"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            // Verificación
            Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Devuelve201_CuandoUsuarioEsAdmin()
        {
            // Preparación
            var factory = ConstruirWebApplicationFactory(nombreBD, false);

            var claims = new List<Claim>
            {
                adminClaim
            };

            var token = await CrearUsuario(nombreBD, factory, claims);

            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var autorCreacionDTO = new AutorCreacionDTO()
            {
                Nombres = "Rodrigo",
                Apellidos = "Rojas",
                Identificacion = "123"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            // Verificación
            respuesta.EnsureSuccessStatusCode();
            Assert.AreEqual(System.Net.HttpStatusCode.Created, respuesta.StatusCode);
        }
    }
}
