using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using Microsoft.AspNetCore.Mvc;
using WeddingPlannerApp.Controllers;
using WeddingPlannerApp.Models;

namespace UnitTests;

[TestClass]
public class MenuUnitTest
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [TestMethod]
    public async Task GetAll_ReturnsOk_WhenMenusExist()
    {
        var context = GetDbContext();

        context.Menus.Add(new Menu
        {
            Name = "Menu 1",
            Description = "Description for Menu 1",
            Price = 100
        });

        await context.SaveChangesAsync();

        var controller = new MenusController(context);
        var result = await controller.GetAll();

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));

    }


    [TestMethod]
    public async Task GetAll_ReturnsOk_WhenNoMenusExist()
    {
        var context = GetDbContext();

        var controller = new MenusController(context);
        var result = await controller.GetAll();

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }



    [TestMethod]
    public async Task Create_ReturnsOk_WhenMenuIsCreated()
    {
        var context = GetDbContext();
        var controller = new MenusController(context);
        var result = await controller.Create();


        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task Create_AddsMenuToDatabase()
    {
        var context = GetDbContext();
        var controller = new MenusController(context);
        await controller.Create();

        Assert.AreEqual(1, context.Menus.Count());

    }

}
