
using Microsoft.AspNetCore.Mvc;

namespace AppPrint_and_Wear.Controllers
{
    public class HomeEmpresaController : Controller
    {
        // GET: HomeEmpresa
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
