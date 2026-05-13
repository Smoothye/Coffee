using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenusController(ApplicationDbContext context) : ControllerBase
{
    // GET: api/Menus
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await context.Menus.ToListAsync());
    }

    // POST: api/Menus
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var menu = new Menu
        {
            Name = "Premium Wedding Menu",
            Description = "Luxury 4 course wedding dinner",
            Price = 350
        };

        context.Menus.Add(menu);
        await context.SaveChangesAsync();

        return Ok(menu);
    }
}