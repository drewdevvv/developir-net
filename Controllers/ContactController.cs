using Microsoft.AspNetCore.Mvc;
using Developir.Web.Models;
using Developir.Web.Services;

namespace Developir.Web.Controllers;

public class ContactController : Controller
{
    private readonly EmailService _emailService;
    
    public ContactController(EmailService emailService)
    {
        _emailService = emailService;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return View(new ContactFormModel());
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        try
        {
            await _emailService.SendContactEmail(
                model.Name,
                model.Email,
                model.Organization,
                model.Message
            );
            
            TempData["Message"] = "Thank you for your message. We'll get back to you soon!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "There was a problem sending your message. Please try again later.");
            return View(model);
        }
    }
}