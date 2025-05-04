using Microsoft.AspNetCore.Mvc;

namespace Kinomatrix.Controllers
{
    public static class ToastExtensions
    {
        public static IActionResult WithToast(this IActionResult result, Controller controller, string message)
        {
            controller.TempData["ToastMessage"] = message;
            return result;
        }
    }
}
