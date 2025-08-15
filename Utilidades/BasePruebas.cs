using AutoMapper;
using BibliotecaAPI.Datos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITest.Utilidades
{
    public class BasePruebas
    {
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
    }
}
