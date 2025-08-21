using BibliotecaAPITest.Utilidades;
using Microsoft.AspNetCore.OutputCaching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITest.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class LibrosControllerPruebas: BasePruebas
    {
        [TestMethod]
        public async Task Get_RetornarCeroLibros_CuandoNoHayLibros()
        {
            // Preparación
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IOutputCacheStore outputCacheStore = null!;

            var controller = new BibliotecaAPI.Controllers.v1.LibrosController(contexto, mapper, null!, outputCacheStore);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var paginacionDTO = new BibliotecaAPI.DTOs.PaginacionDTO(1, 1);

            // Prueba

            var respuesta = await controller.Get(paginacionDTO);

            // Verificación
            Assert.AreEqual(expected: 0, actual: respuesta.Count());
        }
    }
}
