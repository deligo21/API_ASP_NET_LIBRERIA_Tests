using BibliotecaAPI.Controllers.v1;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.v1;
using BibliotecaAPITest.Utilidades;
using BibliotecaAPITest.Utilidades.Dobles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITest.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas: BasePruebas
    {
        IAlmacenadorArchivos almacenadorArchivos = null!;
        ILogger<AutoresController> logger = null!;
        IOutputCacheStore outputCacheStore = null!;
        IServicioAutores servicioAutores = null!;
        private string nombreBD = Guid.NewGuid().ToString();
        private AutoresController controller = null!;

        [TestInitialize]
        public void Setup()
        {
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            almacenadorArchivos = Substitute.For<IAlmacenadorArchivos>();
            logger = Substitute.For<ILogger<AutoresController>>();
            outputCacheStore = Substitute.For<IOutputCacheStore>();
            servicioAutores = Substitute.For<IServicioAutores>();

            controller = new AutoresController(
                context, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutores);
        }

        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {
            // Preparación

            // Prueba
            var respuesta = await controller.Get(1);

            // Verificación
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Get_RetornaAutor_CuandoAutorConIdExiste()
        {
            // Preparación

            var context = ConstruirContext(nombreBD);

            context.Autores.Add(new BibliotecaAPI.Entidades.Autor { Nombres = "Rodrigo", Apellidos = "Rojas" });

            await context.SaveChangesAsync();
            // Prueba
            var respuesta = await controller.Get(1);

            // Verificación
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
        }
        
        [TestMethod]
        public async Task Get_RetornaAutorConLibros_CuandoAutorTieneLibros()
        {
            // Preparación
            var context = ConstruirContext(nombreBD);

            var libro1 = new BibliotecaAPI.Entidades.Libro { Titulo = "Libro 1" };
            var libro2 = new BibliotecaAPI.Entidades.Libro { Titulo = "Libro 2" };

            var autor = new BibliotecaAPI.Entidades.Autor
            {
                Nombres = "Rodrigo",
                Apellidos = "Rojas",
                Libros = new List<BibliotecaAPI.Entidades.AutorLibro>
                {
                    new BibliotecaAPI.Entidades.AutorLibro { Libro = libro1 },
                    new BibliotecaAPI.Entidades.AutorLibro { Libro = libro2 }
                }
            };

            context.Add(autor);

            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Get(1);

            // Verificación
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
            Assert.AreEqual(expected: 2, actual: resultado!.Libros.Count);
        }

        [TestMethod]
        public async Task Get_DebeLlamarGetDelServicioAutores()
        {
            // Preparación
            var paginacionDTO = new BibliotecaAPI.DTOs.PaginacionDTO
            {
                RecordsPorPagina = 10,
                Pagina = 1
            };

            // Prueba
            await controller.Get(paginacionDTO);

            // Verificación
            await servicioAutores.Received(1).Get(paginacionDTO);
        }

        [TestMethod]
        public async Task Post_DebeCrearAutor_CuandoEnviamosAutor()
        {
            // Preparación
            var context = ConstruirContext(nombreBD);

            var nuevoAutor = new BibliotecaAPI.DTOs.AutorCreacionDTO
            {
                Nombres = "Rodrigo",
                Apellidos = "Rojas"
            };

            // Prueba
            var respuesta = await controller.Post(nuevoAutor);

            // Verificación
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            var contexto2 = ConstruirContext(nombreBD);
            var cantidad = await contexto2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidad);
        }

        [TestMethod]
        public async Task Put_Retorna404_CuandoAutorNoExiste()
        {
            // Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO: null!);
            
            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }

        private const string contenedor = "Autores";
        private const string cache = "autores-obtener";

        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorSinFoto()
        {
            // Preparación
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new BibliotecaAPI.Entidades.Autor { Nombres = "Rodrigo", Apellidos = "Rojas", Identificacion = "Id" });
            await context.SaveChangesAsync();

            var autorCreacionDTO = new BibliotecaAPI.DTOs.AutorCreacionDTOConFoto
            {
                Nombres = "Nuevo Nombre",
                Apellidos = "Nuevo Apellido",
                Identificacion = "Nuevo Id"
            };

            // Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);
            
            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, actual: resultado!.StatusCode);

            var contexto2 = ConstruirContext(nombreBD);
            var autorActualizado = await contexto2.Autores.SingleAsync();
            Assert.AreEqual(expected: "Nuevo Nombre", actual: autorActualizado.Nombres);
            Assert.AreEqual(expected: "Nuevo Apellido", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "Nuevo Id", actual: autorActualizado.Identificacion);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.DidNotReceiveWithAnyArgs().Editar(default!, default!, default!);
        }
        
        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorConFoto()
        {
            // Preparación
            var context = ConstruirContext(nombreBD);

            var urlAnterior = "URL-1";
            var urlNueva = "URL-2";
            almacenadorArchivos.Editar(default!, default!, default!).ReturnsForAnyArgs(urlNueva);

            context.Autores.Add(new BibliotecaAPI.Entidades.Autor { Nombres = "Rodrigo", Apellidos = "Rojas", Identificacion = "Id", Foto = urlAnterior });
            await context.SaveChangesAsync();

            var formFile = Substitute.For<IFormFile>();

            var autorCreacionDTO = new BibliotecaAPI.DTOs.AutorCreacionDTOConFoto
            {
                Nombres = "Nuevo Nombre",
                Apellidos = "Nuevo Apellido",
                Identificacion = "Nuevo Id",
                Foto = formFile
            };

            // Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);
            
            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, actual: resultado!.StatusCode);

            var contexto2 = ConstruirContext(nombreBD);
            var autorActualizado = await contexto2.Autores.SingleAsync();
            Assert.AreEqual(expected: "Nuevo Nombre", actual: autorActualizado.Nombres);
            Assert.AreEqual(expected: "Nuevo Apellido", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "Nuevo Id", actual: autorActualizado.Identificacion);
            Assert.AreEqual(expected: urlNueva, actual: autorActualizado.Foto);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Editar(urlAnterior, contenedor, formFile);
        }
    }
}
