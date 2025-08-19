using BibliotecaAPI.Controllers.v1;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.v1;
using BibliotecaAPITest.Utilidades;
using BibliotecaAPITest.Utilidades.Dobles;
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
        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {
            // Preparación
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = null!;

            var controller = new AutoresController(
                context, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutores);

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
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = null!;

            context.Autores.Add(new BibliotecaAPI.Entidades.Autor { Nombres = "Rodrigo", Apellidos = "Rojas" });

            await context.SaveChangesAsync();
            var context2 = ConstruirContext(nombreBD);

            var controller = new AutoresController(
                context2, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutores);

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
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = null!;

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
            var context2 = ConstruirContext(nombreBD);

            var controller = new AutoresController(
                context2, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutores);

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
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = Substitute.For<IServicioAutores>();

            var controller = new AutoresController(
                context, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutores);

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
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = new OutputCacheStoreFalse();
            IServicioAutores servicioAutores = null!;

            var nuevoAutor = new BibliotecaAPI.DTOs.AutorCreacionDTO
            {
                Nombres = "Rodrigo",
                Apellidos = "Rojas"
            };

            var controller = new AutoresController(
                context, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutores);

            // Prueba
            var respuesta = await controller.Post(nuevoAutor);

            // Verificación
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            var contexto2 = ConstruirContext(nombreBD);
            var cantidad = await contexto2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidad);
        }
    }
}
