using MovieStore.UI.Models;

namespace MovieStore.UI.Components;

public class CartViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View(new CartSummary(0m, 0));
}
