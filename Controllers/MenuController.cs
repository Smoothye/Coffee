using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Menu;
using WeddingPlannerApp.Models;

namespace WeddingPlannerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenusController(ApplicationDbContext context) : ControllerBase
{
    // GET: api/Menus
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuDto>>> GetAll()
    {
        var menus = await context.Menus
            .Include(menu => menu.MenuItems)
            .OrderBy(menu => menu.DietaryType)
            .ThenBy(menu => menu.Name)
            .ToListAsync();

        return Ok(menus.Select(ToDto));
    }

    // GET: api/Menus/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MenuDto>> GetById(int id)
    {
        var menu = await context.Menus
            .Include(menu => menu.MenuItems)
            .SingleOrDefaultAsync(m => m.MenuId == id);
        
        if (menu == null)
            return NotFound($"Menu with id: {id} was not found.");
        
        return Ok(ToDto(menu));
    }

    // POST: api/Menus
    [HttpPost]
    public async Task<ActionResult<MenuDto>> Create([FromBody] MenuCreateDto model)
    {
        if (model.MenuItems.Count == 0)
            return BadRequest("At least one menu item is required.");

        var menu = new Menu
        {
            Name = model.Name,
            Description = model.Description,
            DietaryType = model.DietaryType,
            Price = model.Price,
            MenuItems = model.MenuItems
                .OrderBy(item => item.DisplayOrder)
                .Select((item, index) => new MenuItem
                {
                    CourseName = item.CourseName,
                    Name = item.Name,
                    Description = item.Description,
                    DisplayOrder = index
                })
                .ToList()
        };

        context.Menus.Add(menu);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = menu.MenuId }, ToDto(menu));
    }

    // PUT: api/Menus/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MenuUpdateDto model)
    {
        if (model.MenuItems.Count == 0)
            return BadRequest("At least one menu item is required.");

        var menuExists = await context.Menus.AnyAsync(m => m.MenuId == id);
        if (!menuExists)
            return NotFound($"Menu with id: {id} was not found.");

        var existingItems = await context.MenuItems
            .Where(item => item.MenuId == id)
            .ToListAsync();

        context.MenuItems.RemoveRange(existingItems);

        var menu = new Menu
        {
            MenuId = id,
            Name = model.Name,
            Description = model.Description,
            DietaryType = model.DietaryType,
            Price = model.Price
        };

        context.Menus.Update(menu);
        context.MenuItems.AddRange(model.MenuItems
            .OrderBy(item => item.DisplayOrder)
            .Select((item, index) => new MenuItem
            {
                MenuId = id,
                CourseName = item.CourseName,
                Name = item.Name,
                Description = item.Description,
                DisplayOrder = index
            })
            .ToList());

        await context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Menus/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var menu = await context.Menus
            .Include(m => m.Events)
            .SingleOrDefaultAsync(m => m.MenuId == id);

        if (menu == null)
            return NotFound($"Menu with id: {id} was not found.");

        if (menu.Events.Count != 0)
            return BadRequest("Cannot delete a menu that is assigned to one or more events.");

        context.Menus.Remove(menu);
        await context.SaveChangesAsync();

        return NoContent();
    }

    static MenuDto ToDto(Menu menu) => new()
    {
        MenuId = menu.MenuId,
        Name = menu.Name,
        Price = menu.Price,
        DietaryType = menu.DietaryType,
        Description = menu.Description,
        MenuItems = menu.MenuItems
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new MenuItemDto
            {
                MenuItemId = item.MenuItemId,
                MenuId = item.MenuId,
                CourseName = item.CourseName,
                Name = item.Name,
                Description = item.Description,
                DisplayOrder = item.DisplayOrder
            })
            .ToList()
    };
}
