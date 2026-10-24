namespace MovieStore.UI.Controllers;

public class CartController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Add(int id, string? returnUrl)
    {
        TempData["CartMessage"] = $"Билет на фильм с ID {id} добавлен в корзину.";
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }
}
