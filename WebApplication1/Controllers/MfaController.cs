using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class MfaController : Controller
    {
        private readonly GraphServiceClient _graphClient;

        public MfaController(GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetMfa(string userUpn)
        {
            var user = await _graphClient.Users[userUpn]
                .Request()
                .GetAsync();

            var authMethods = await _graphClient.Users[userUpn]
                .Authentication
                .Methods
                .Request()
                .GetAsync();

            foreach (var method in authMethods.CurrentPage)
            {
                await _graphClient.Users[userUpn]
                    .Authentication
                    .Methods[method.Id]
                    .Request()
                    .DeleteAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
