using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.Controllers;
using WeddingPlannerApp.Models;
using Microsoft.AspNetCore.Mvc;
using WeddingPlannerApp.DTOs.Venue;

namespace UnitTests;

[TestClass]
public class VenuesUnitTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [TestMethod]
    public async Task GetAll_ReturnsOk_WhenVenues_Exist()
    {
        var context = GetDbContext();
        context.Venues.Add(new Venue
        {
            Name = "Venue 1",
            Address = "Address for Venue 1",
            MinCapacity = 50,
            MaxCapacity = 200,
            EstimatedPrice = 5000,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Description for Venue 1",
            Rating = 5,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/image.jpg"
        });
        await context.SaveChangesAsync();
        var controller = new VenuesController(context);
        var result = await controller.GetAll();
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetAll_ReturnsOk_WhenNoVenuesExist()
    {
        var context = GetDbContext();
        var controller = new VenuesController(context);
        var result = await controller.GetAll();

        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetById_ReturnsOk_WhenVenueExists()
    {
        var context = GetDbContext();

        context.Venues.Add(new Venue
        {
            Name = "Venue 1",
            Address = "Address for Venue 1",
            MinCapacity = 50,
            MaxCapacity = 200,
            EstimatedPrice = 5000,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Description for Venue 1",
            Rating = 4,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/image.jpg"
        });

        await context.SaveChangesAsync();
        var controller = new VenuesController(context);
        var result = await controller.GetById(1);

        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetById_ReturnsNotFound_WhenVenueDoesNotExist()
    {
        var context = GetDbContext();
        var controller = new VenuesController(context);
        var result = await controller.GetById(999);
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task Create_ReturnsOk_WhenDataIsValid()
        {
        var context = GetDbContext();

        var controller = new VenuesController(context);
        var newVenue = new VenueCreateDto
        {
            Name = "New Venue",
            Address = "Address for New Venue",
            MinCapacity = 30,
            MaxCapacity = 150,
            EstimatedPrice = 3000,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Description for New Venue",
            Rating = 4,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/image.jpg"
        };

        var result = await controller.Create(newVenue);

        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
    }

    [TestMethod]
    public async Task Create_ReturnsBadRequest_WhenMinCapacityGreaterThanMaxCapacity()
    {
        var context = GetDbContext();

        var controller = new VenuesController(context);

        var newVenue = new VenueCreateDto
        {
            Name = "New Venue",
            Address = "Address for New Venue",
            MinCapacity = 200,
            MaxCapacity = 150,
            EstimatedPrice = 3000,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Description for New Venue",
            Rating = 4,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/image.jpg"
        };

        var result = await controller.Create(newVenue);
        
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }


    [TestMethod]
    public async Task Update_ReturnsNoContent_WhenVenueExists()
    {
        var context = GetDbContext();

        context.Venues.Add(new Venue
        {
            Name = "Venue 1",
            Address = "Address for Venue 1",
            MinCapacity = 50,
            MaxCapacity = 200,
            EstimatedPrice = 5000,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Description for Venue 1",
            Rating = 4,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/image.jpg"
        });

        await context.SaveChangesAsync();

        var controller = new VenuesController(context);

        var updateVenue = new VenueUpdateDto
        {
            Name = "Updated Venue",
            Address = "Updated Address",
            MinCapacity = 60,
            MaxCapacity = 250,
            EstimatedPrice = 6000,
            Latitude = 407128,
            Longitude = -740060,
            Description = "Updated Description",
            Rating = 5,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/updated_image.jpg"
        };

        var result = await controller.Update(1, updateVenue);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task Update_ReturnsNotFound_WhenVenueDoesNotExist()
    {
        var context = GetDbContext();
        var controller = new VenuesController(context);
        var updateVenue = new VenueUpdateDto
        {
            Name = "Updated Venue",
            Address = "Updated Address",
            MinCapacity = 60,
            MaxCapacity = 250,
            EstimatedPrice = 6000,
            Latitude = 407128,
            Longitude = -740060,
            Description = "Updated Description",
            Rating = 5,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/updated_image.jpg"
        };
        var result = await controller.Update(999, updateVenue);
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task Update_ReturnsBadRequest_WhenMinCapacityGreaterThanMaxCapacity()
    {
        var context = GetDbContext();
        context.Venues.Add(new Venue
        {
            Name = "Venue 1",
            Address = "Address for Venue 1",
            MinCapacity = 50,
            MaxCapacity = 200,
            EstimatedPrice = 5000,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Description for Venue 1",
            Rating = 4,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/image.jpg"
        });
        await context.SaveChangesAsync();
        var controller = new VenuesController(context);
        var updateVenue = new VenueUpdateDto
        {
            Name = "Updated Venue",
            Address = "Updated Address",
            MinCapacity = 300,
            MaxCapacity = 250,
            EstimatedPrice = 6000m,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Updated Description",
            Rating = 5,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/updated_image.jpg"
        };
        var result = await controller.Update(1, updateVenue);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Delete_ReturnsNoContent_WhenVenueExistsAndHasNoEvents()
    {
        var context = GetDbContext();
        context.Venues.Add(new Venue
        {
            Name = "Venue 1",
            Address = "Address for Venue 1",
            MinCapacity = 50,
            MaxCapacity = 200,
            EstimatedPrice = 5000,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Description for Venue 1",
            Rating = 4,
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/image.jpg"
        });
        await context.SaveChangesAsync();
        var controller = new VenuesController(context);
        var result = await controller.Delete(1);
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task Delete_ReturnsBadRequest_WhenVenueHasEvents()
    {
      var context = GetDbContext();

      var venue = new Venue
      {
          Name = "Venue 1",
          Address = "Address for Venue 1",
          MinCapacity = 50,
          MaxCapacity = 200,
          EstimatedPrice = 5000,
          Latitude = 40.7128m,
          Longitude = -74.0060m,
          Description = "Description for Venue 1",
          Rating = 4,
          Tags = VenueTag.Indoor,
          ImagePath = "path/to/image.jpg"
      };

        context.Venues.Add(venue);
        await context.SaveChangesAsync();

        context.Events.Add(new Event
        {
            Name = "Event 1",
            EventDate = DateTime.Now,
            VenueId = venue.VenueId,
            MenuId = 1
        });

        await context.SaveChangesAsync();
        var controller = new VenuesController(context);
        var result = await controller.Delete(1);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Delete_ReturnsNotFound_WhenVenueDoesNotExist()
    {
        var context = GetDbContext();
        var controller = new VenuesController(context);
        var result = await controller.Delete(999);
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }


    [TestMethod]
    public async Task Create_ReturnsOk_WhenRatingIsDecimal()
    {
        var context = GetDbContext();

        var controller = new VenuesController(context);

        var newVenue = new VenueCreateDto
        {
            Name = "New Venue",
            Address = "Address for New Venue",
            MinCapacity = 30,
            MaxCapacity = 150,
            EstimatedPrice = 3000,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Description for New Venue",
            Rating = (int?)Convert.ToInt32(4.5),
            Tags = VenueTag.Indoor,
            ImagePath = "path/to/image.jpg"
        };

        var result = await controller.Create(newVenue);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Create_ReturnsCreated_WhenTagsAreValid()
    {
        var context = GetDbContext();

        var controller = new VenuesController(context);

        var newVenue = new VenueCreateDto
        {
            Name = "New Venue",
            Address = "Address for New Venue",
            MinCapacity = 30,
            MaxCapacity = 150,
            EstimatedPrice = 3000,
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            Description = "Description for New Venue",
            Rating = 4,
            Tags = VenueTag.Outdoor,
            ImagePath = "path/to/image.jpg"
        };

        var result = await controller.Create(newVenue);

        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));


    }

}

