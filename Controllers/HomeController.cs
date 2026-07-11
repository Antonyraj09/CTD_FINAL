using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    [Route("/Home/Error")]
    public IActionResult Error()
    {
        ViewData["Title"] = "Something went wrong";
        return View();
    }

    [Route("/Home/StatusCode")]
    public new IActionResult StatusCode(int code)
    {
        ViewData["Title"] = code switch
        {
            404 => "Page Not Found",
            403 => "Forbidden",
            _ => "Unexpected Error"
        };
        ViewData["Code"] = code;
        return View();
    }
}
