namespace MovieStore.UI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieStore.UI.Models;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View(CreateList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(int listDemo)
    {
        ViewData["Title"] = "Лабораторная работа №2";
        ViewData["FormMessage"] = $"Форма отправлена. Выбран элемент с ID {listDemo}.";
        return View(CreateList(listDemo));
    }

    private SelectList CreateList(int? selectedValue = null)
    {
        ViewData["Title"] = "Лабораторная работа №2";
        var items = Enumerable.Range(1, 4)
            .Select(number => new ListDemo { Id = number, Name = $"Элемент {number}" })
            .ToList();

        return new SelectList(items, nameof(ListDemo.Id), nameof(ListDemo.Name), selectedValue);
    }
}
