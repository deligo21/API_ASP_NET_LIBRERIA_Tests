using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BibliotecaAPITest.Utilidades
{
    public class UsuarioFalsoFiltro : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>{
                new Claim("email", "ejemplo@hotmail.com")
            }, "prueba"));

            await next();


        }
    }
}
