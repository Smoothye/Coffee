using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WeddingPlannerApp.Controllers;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Event;
using WeddingPlannerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace UnitTests

{
    [TestClass]
    public  class Test1
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [TestMethod]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            var context = GetDbContext();
            context.Events.Add(new Event
            {
                EventId = 1,
                VenueId = 1,
                MenuId = 1,
                Name = "Test",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 10,
                TotalBudget = 100
            });
            await context.SaveChangesAsync();
            var controller = new EventsController(context);
            var result = await controller.GetById(1);

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }


        [TestMethod]
        public async Task GetById_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var context = GetDbContext();
            var controller = new EventsController(context);
            var result = await controller.GetById(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAll_ReturnsListOfEvents()
        {
            var context = GetDbContext();

            context.Events.Add(new Event
            {
                EventId = 1,
                VenueId = 1,
                MenuId = 1,
                Name = "Event 1",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 10,
                TotalBudget = 100
            });

            context.Events.Add(new Event
            {
                EventId = 2,
                VenueId = 1,
                MenuId = 1,
                Name = "Event 2",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 20,
                TotalBudget = 200
            });

            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            var result = await controller.GetAll();

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

            var okResult = result.Result as OkObjectResult;
            var events = okResult.Value as IEnumerable<EventDto>;

            Assert.AreEqual(2, events.Count());
        }

        [TestMethod]
        public async Task CreateEvent_Valid_ReturnsCreated()
        {
            var context = GetDbContext();

            context.Venues.Add(new Venue
            {
                VenueId = 1,
                Name = "Venue",
                Address = "Cluj",
                MinCapacity = 50,
                MaxCapacity = 200,
                EstimatedPrice = 1000
            });
            context.Menus.Add(new Menu
            {
                MenuId = 1,
                Name = "Menu",
                Price = 100,
                Description = "Test menu"
            });

            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            var dto = new EventCreateDto
            {
                VenueId = 1,
                MenuId = 1,
                Name = "Event",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 100,
                TotalBudget = 5000,
                Notes = "Test event"
            };

            var result = await controller.Create(dto);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }


        [TestMethod]
        public async Task Create_ReturnsBadRequest_WhenVenueDoesNotExist()
        {
            var context = GetDbContext();

            context.Menus.Add(new Menu
            {
                MenuId = 1,
                Name = "Standard Menu",
                Price = 100,
                Description = "Basic wedding menu"
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            var model = new EventCreateDto
            {
                VenueId = 1,
                MenuId = 1,
                Name = "Wedding Event",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 100,
                TotalBudget = 5000,
                Notes = "Test event"
            };

            var result = await controller.Create(model);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        }

        [TestMethod]
        public async Task Create_ReturnsCreated_WhenDataIsValid()
        {
            var context = GetDbContext();

            context.Venues.Add(new Venue
            {
                VenueId = 1,
                Name = "Test Venue",
                Address = "Cluj-Napoca",
                MinCapacity = 50,
                MaxCapacity = 200,
                EstimatedPrice = 3000
            });
            context.Menus.Add(new Menu
            {
                MenuId = 1,
                Name = "Standard Menu",
                Price = 100,
                Description = "Basic wedding menu"
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            var model = new EventCreateDto
            {
                VenueId = 1,
                MenuId = 1,
                Name = "Wedding",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 100,
                TotalBudget = 5000,
                Notes = "Test"
            };

            var result = await controller.Create(model);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        }

        [TestMethod]
        public async Task DeleteEvent_Valid_ReturnsNoContent()
        {
            var context = GetDbContext();

            context.Events.Add(new Event
            {
                EventId = 1,
                VenueId = 1,
                MenuId = 1,
                Name = "Test Event",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 10,
                TotalBudget = 2000,
                Notes = "Test event",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            var controller = new EventsController(context);
            var result = await controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var context = GetDbContext();
            var controller = new EventsController(context);

            var result = await controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsNotContent_WhenEventExists()
        {
            var context = GetDbContext();

            context.Venues.Add(new Venue
            {
                VenueId = 1,
                Name = "Venue",
                Address = "Cluj",
                MinCapacity = 10,
                MaxCapacity = 100,
                EstimatedPrice = 1000
            });

            context.Menus.Add(new Menu
            {
                MenuId = 1,
                Name = "Menu",
                Price = 100,
                Description = "Test menu"
            });

            context.Events.Add(new Event
            {
                EventId = 1,
                VenueId = 1,
                MenuId = 1,
                Name = " Test Event",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 10,
                TotalBudget = 2000,
                Notes = "Test event",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });


            await context.SaveChangesAsync();
            var controller = new EventsController(context);
            var result = await controller.Delete(1);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Update_ReturnsNoContent_WhenEventExists()
        {
            var context = GetDbContext();
            context.Venues.Add(new Venue
            {
                VenueId = 1,
                Name = "V",
                Address = "A",
                MinCapacity = 10,
                MaxCapacity = 100,
                EstimatedPrice = 1000
            });
            context.Menus.Add(new Menu
            {
                MenuId = 1,
                Name = "M",
                Price = 100,
                Description = "D"
            });
            context.Events.Add(new Event
            {
                EventId = 1,
                VenueId = 1,
                MenuId = 1,
                Name = "Old",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 10,
                TotalBudget = 2000,
                Notes = "Test event",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            var controller = new EventsController(context);
            var model = new EventUpdateDto
            {
                VenueId = 1,
                MenuId = 1,
                Name = "Updated Event",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 100,
                TotalBudget = 5000,
                Notes = "Updated"
            };
            var result = await controller.Update(1, model);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Update_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var context = GetDbContext();
            var controller = new EventsController(context);

            var model = new EventUpdateDto
            {
                VenueId = 1,
                MenuId = 1,
                Name = "Updated Event",
                EventDate = DateTime.UtcNow,
                EstimatedGuests = 100,
                TotalBudget = 5000,
                Notes = "Updated"

            };

            var result = await controller.Update(1, model);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

        }



        ////[TestMethod]
        //public async Task Create_ReturnsBadRequest_WhenNameIsEmpty()
        //{
        //    var context = GetDbContext();

        //    context.Venues.Add(new Venue
        //    {
        //        VenueId = 1,
        //        Name = "Test Venue",
        //        Address = "Cluj-Napoca",
        //        MinCapacity = 50,
        //        MaxCapacity = 200,
        //        EstimatedPrice = 3000
        //    });

        //    context.Menus.Add(new Menu
        //    {
        //        MenuId = 1,
        //        Name = "Standard Menu",
        //        Price = 100,
        //        Description = "Basic wedding menu"
        //    });

        //    await context.SaveChangesAsync();
        //    var controller = new EventsController(context);

        //    var dto = new EventCreateDto
        //    {
        //        VenueId = 1,
        //        MenuId = 1,
        //        Name = "",
        //        EventDate = DateTime.UtcNow,
        //        EstimatedGuests = 100,
        //        TotalBudget = 5000,
        //        Notes = "Test event"
        //    };

        //    var result = await controller.Create(dto);
        //    Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        //}

        ////[TestMethod]
        //public async Task Create_ReturnsBadRequest_WheenEstimatedGuestsIsZeroOrLess()
        //{
        //    var context = GetDbContext();

        //    context.Venues.Add(new Venue
        //    {
        //        VenueId = 1,
        //        Name = "Test Venue",
        //        Address = "Cluj-Napoca",
        //        MinCapacity = 50,
        //        MaxCapacity = 200,
        //        EstimatedPrice = 3000
        //    });

        //    context.Menus.Add(new Menu
        //    {
        //        MenuId = 1,
        //        Name = "Standard Menu",
        //        Price = 100,
        //        Description = "Basic wedding menu"
        //    });

        //    await context.SaveChangesAsync();
        //    var controller = new EventsController(context);

        //    var dto = new EventCreateDto
        //    {
        //        VenueId = 1,
        //        MenuId = 1,
        //        Name = "Test Event",
        //        EventDate = DateTime.UtcNow,
        //        EstimatedGuests = 0,
        //        TotalBudget = 5000,
        //        Notes = "Test event"
        //    };

        //    var result = await controller.Create(dto);
        //    Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        //}

        ////[TestMethod]
        //public async Task Create_ReturnsBadRequest_WhenTotalBudgetIsNegative()
        //{
        //    var context = GetDbContext();
        //    context.Venues.Add(new Venue
        //    {
        //        VenueId = 1,
        //        Name = "Test Venue",
        //        Address = "Cluj-Napoca",
        //        MinCapacity = 50,
        //        MaxCapacity = 200,
        //        EstimatedPrice = 3000
        //    });
        //    context.Menus.Add(new Menu
        //    {
        //        MenuId = 1,
        //        Name = "Standard Menu",
        //        Price = 100,
        //        Description = "Basic wedding menu"
        //    });
        //    await context.SaveChangesAsync();
        //    var controller = new EventsController(context);
        //    var dto = new EventCreateDto
        //    {
        //        VenueId = 1,
        //        MenuId = 1,
        //        Name = "Test Event",
        //        EventDate = DateTime.UtcNow,
        //        EstimatedGuests = 100,
        //        TotalBudget = -5000,
        //        Notes = "Test event"
        //    };
        //    var result = await controller.Create(dto);
        //    Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        //}
    }
}
