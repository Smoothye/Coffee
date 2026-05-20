using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlannerApp.Controllers;
using WeddingPlannerApp.Data;
using WeddingPlannerApp.DTOs.Guest;
using WeddingPlannerApp.Models;



namespace UnitTests;

[TestClass]
public class GuestsUnitTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [TestMethod]
    public async Task GetAll_ReturnsOk_WhenEventExists()
    {
        // Arrange
        var context = GetDbContext();

        var eventItem = new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        };

        context.Events.Add(eventItem);

        var guestItem = new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = eventItem.EventId,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegetarian
        };

        context.Guests.Add(guestItem);

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        // Act
        var result = await controller.GetAll(eventItem.EventId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetAll_ReturnsBadRequest_WhenEventDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();
        var controller = new WeddingPlannerApp.Controllers.GuestsController(context);
        // Act
        var result = await controller.GetAll(999); // Non-existent eventId
        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetById_ReturnsOk_WhenGuestExists()
    {
        // Arrange
        var context = GetDbContext();

        var eventItem = new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        };

        context.Events.Add(eventItem);

        var guestItem = new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = eventItem.EventId,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegan
        };

        context.Guests.Add(guestItem);

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        // Act
        var result = await controller.GetById(eventItem.EventId, guestItem.GuestId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetById_ReturnsBadRequest_WhenEventDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();
        var controller = new WeddingPlannerApp.Controllers.GuestsController(context);
        // Act
        var result = await controller.GetById(999, 1);
    }

    [TestMethod]
    public async Task Create_ReturnsCreated_WhenValidGuest()
    {
        // Arrange
        var context = GetDbContext();

        var eventItem = new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        };

        context.Events.Add(eventItem);
        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var newGuest = new GuestDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Fasting,
            HasPlusOne = false
        };

        // Act
        var result = await controller.Create(newGuest, eventItem.EventId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
    }

    [TestMethod]
    public async Task Create_ReturnsBadRequest_WhenEventDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();
        var controller = new GuestsController(context);

        var newGuest = new GuestDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Standard,
            HasPlusOne = false
        };

        // Act
        var result = await controller.Create(newGuest, 999);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Bulk_Create_ReturnsCreated_WhenValidGuests()
    {
        // Arrange
        var context = GetDbContext();

        var eventItem = new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        };

        context.Events.Add(eventItem);
        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var newGuests = new List<GuestCreateDto>
    {
        new GuestCreateDto
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice@test.com"
        },
        new GuestCreateDto
        {
            FirstName = "Bob",
            LastName = "Brown",
            Email = "bob@test.com"
        }
    };

        // Act
        var result = await controller.BulkCreate(eventItem.EventId, newGuests);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
    }

    [TestMethod]
    public async Task Bulk_Create_ReturnsNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();
        var controller = new GuestsController(context);

        var newGuests = new List<GuestCreateDto>
    {
        new GuestCreateDto
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice@test.com"
        },
        new GuestCreateDto
        {
            FirstName = "Bob",
            LastName = "Brown",
            Email = "bob@test.com"
        }
    };

        // Act
        var result = await controller.BulkCreate(999, newGuests);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task BulkCreate_ReturnsNotFound_WhenEventDoesNotExists()
    {
        var context = GetDbContext();
        var controller = new GuestsController(context);

        var result = await controller.BulkCreate(999, new List<GuestCreateDto>
    {
        new GuestCreateDto
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "a@test.com"
        },
        new GuestCreateDto
        {
            FirstName = "Bob",
            LastName = "Brown",
            Email = "b@test.com"
        }
    });

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task Update_ReturnsNoContent_WhenValid()
    {
        var context = GetDbContext();

        var eventItem = new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        };

        context.Events.Add(eventItem);

        var guest = new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = 1,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegan
        };

        context.Guests.Add(guest);

        await context.SaveChangesAsync();

        var controller = new WeddingPlannerApp.Controllers.GuestsController(context);

        var model = new GuestUpdateDto
        {
            FirstName = "Updated",
            LastName = "Updated",
            Email = "upd@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Standard
            
        };

        var result = await controller.Update(1, 1, model);
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task Update_ReturnsNotFound_WhenGuestDoesNotExist()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestUpdateDto
        {
            FirstName = "Test",
            LastName = "Test",
            Email = "test@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegan,
            HasPlusOne = false
        };

        var result = await controller.Update(1, 999, model);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task Update_ReturnsBadRequest_WhenGuestNotInEvent()
    {
        var context = GetDbContext();

        context.Events.Add(new Event { EventId = 1, Name = "Test Event", EventDate = DateTime.Now });
        context.Events.Add(new Event { EventId = 2, Name = "Another Event", EventDate = DateTime.Now });

        context.Guests.Add(new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = 2,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegan
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestUpdateDto
        {
            FirstName = "Updated",
            LastName = "Updated",
            Email = "upd@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Fasting
        };

        var result = await controller.Update(1, 1, model);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Delete_ReturnsNoContent_WhenGuestExists()
    {
        var context = GetDbContext();
        var eventItem = new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        };

        context.Events.Add(eventItem);

        var guestItem = new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = eventItem.EventId,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegan
        };

        context.Guests.Add(guestItem);

        await context.SaveChangesAsync();
        var controller = new WeddingPlannerApp.Controllers.GuestsController(context);
        var result = await controller.Delete(1, 1);
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task Delete_ReturnsNotFound_WhenGuestDoesNotExist()
    {
        var context = GetDbContext();
        context.Events.Add(new Event { Name = "Test Event", EventDate = DateTime.Now });
        await context.SaveChangesAsync();
        var controller = new WeddingPlannerApp.Controllers.GuestsController(context);
        var result = await controller.Delete(1, 999);
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

     [TestMethod]
     public async Task Delete_ReturnsBadRequest_WhenGuestNotInEvent()
     {
         var context = GetDbContext();
        var event1 = new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        };

        var event2 = new Event
        {
            EventId = 2,
            Name = "Another Event",
            EventDate = DateTime.Now
        };

        context.Events.AddRange(event1, event2);

        var guest = new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = 1,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegan
        };

        context.Guests.Add(guest);

        await context.SaveChangesAsync();

        var controller = new WeddingPlannerApp.Controllers.GuestsController(context);
        var result = await controller.Delete(2, 1);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

    }

    [TestMethod]
    public async Task Create_ReturnsBadRequest_WhenAgeIsNegative()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Age = -5, 
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Standard
        };

        var result = await controller.Create(model, 1);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }


    [TestMethod]
    public async Task BulkCreate_ReturnsBadRequest_WhenAgeIsNegative()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var list = new List<GuestCreateDto>
    {
        new GuestCreateDto
        {
            FirstName = "Alice",
            LastName = "Brown",
            Email = "a@test.com",
            Age = -1
        },
        new GuestCreateDto
        {
            FirstName = "Bob",
            LastName = "Smith",
            Email = "b@test.com",
            Age = 25
        }
    };

        var result = await controller.BulkCreate(1, list);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Update_ReturnsBadRequest_WhenAgeIsNegative()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        context.Guests.Add(new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = 1,
            Age = 20,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegan
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestUpdateDto
        {
            FirstName = "Updated",
            LastName = "Updated",
            Email = "upd@test.com",
            Age = -5,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Standard
        };

        var result = await controller.Update(1, 1, model);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }


    [TestMethod]
    public async Task Create_ReturnsBadRequest_WhenTableIdIsNegative()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            TableId = -1,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegetarian
        };

        var result = await controller.Create(model, 1);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task BulkCreate_ReturnsBadRequest_WhenTableIdIsNegative()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            TableId = -1,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegetarian
        };

        var result = await controller.Create(model, 1);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }


    [TestMethod]
    public async Task Update_ReturnsBadRequest_WhenTableNumberIsNegative()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            TableId = -1,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Standard
        };

        var result = await controller.Create(model, 1);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Create_AllowsHyphenInName()
    {
        var context = GetDbContext();
        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });
        await context.SaveChangesAsync();
        var controller = new GuestsController(context);
        var model = new GuestDto
        {
            FirstName = "Mary-Jane",
            LastName = "Smith",
            Email = "test@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Fasting
        };
        var result = await controller.Create(model, 1);

        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
    }

    [TestMethod]
    public async Task Update_AllowsHyphenInName()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        context.Guests.Add(new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            EventId = 1,
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Fasting
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestUpdateDto
        {
            FirstName = "Mary-Jane",  
            LastName = "Doe",
            Email = "upd@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegetarian
        };

        var result = await controller.Update(1, 1, model);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }


    [TestMethod]
    public async Task BulkCreate_AllowsHyphenInFirstName()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var list = new List<GuestCreateDto>
    {
        new GuestCreateDto
        {
            FirstName = "Mary-Jane", 
            LastName = "Smith",
            Email = "a@test.com"
        },
        new GuestCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "b@test.com"
        }
    };

        var result = await controller.BulkCreate(1, list);

        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
    }



    [TestMethod]
    public async Task Create_BadRequest_WhenNameContainsNumber()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });
        await context.SaveChangesAsync();

        var controller = new GuestsController(context);
        
        var model = new GuestDto
        {
            FirstName = "John1",
            LastName = "Doe",
            Email = "john@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegan
        };

        var result = await controller.Create(model, 1);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Update_ReturnsBadRequest_WhenFirstNameContainsNumber()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        context.Guests.Add(new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = 1
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestUpdateDto
        {
            FirstName = "Mary1", 
            LastName = "Doe",
            Email = "upd@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Fasting
        };

        var result = await controller.Update(1, 1, model);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task BulkCreate_ReturnsBadRequest_WhenFirstNameContainsNumber()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var list = new List<GuestCreateDto>
    {
        new GuestCreateDto
        {
            FirstName = "Alice1", 
            LastName = "Brown",
            Email = "a@test.com"
        },
        new GuestCreateDto
        {
            FirstName = "Bob",
            LastName = "Smith",
            Email = "b@test.com"
        }
    };

        var result = await controller.BulkCreate(1, list);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }



    [TestMethod]
    public async Task Create_BadRequest_WhenNameHasSpecialCharacters()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });
        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestDto
        {
            FirstName = "John@",
            LastName = "Doe",
            Email = "john@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Vegetarian
        };

        var result = await controller.Create(model, 1);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Update_ReturnsBadRequest_WhenFirstNameHasSpecialCharacters()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        context.Guests.Add(new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = 1
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestUpdateDto
        {
            FirstName = "Mary@", 
            LastName = "Doe",
            Email = "upd@test.com",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Fasting
        };

        var result = await controller.Update(1, 1, model);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task BulkCreate_ReturnsBadRequest_WhenFirstNameHasSpecialCharacters()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var list = new List<GuestCreateDto>
    {
        new GuestCreateDto
        {
            FirstName = "Alice@",
            LastName = "Brown",
            Email = "a@test.com"
        },
        new GuestCreateDto
        {
            FirstName = "Bob",
            LastName = "Smith",
            Email = "b@test.com"
        }
    };

        var result = await controller.BulkCreate(1, list);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Create_BadRequest_WhenEmailIsInvalid()
    {
        var context = GetDbContext();
        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });
        await context.SaveChangesAsync();
        var controller = new GuestsController(context);
        var model = new GuestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email",
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Standard
        };
        var result = await controller.Create(model, 1);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Update_ReturnsBadRequest_WhenEmailIsInvalid()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        context.Guests.Add(new Guest
        {
            GuestId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            EventId = 1
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var model = new GuestUpdateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email", 
            RsvpStatus = RsvpStatus.Pending,
            Group = Group.Friends,
            DietaryRequirements = DietaryRequirements.Standard
        };

        var result = await controller.Update(1, 1, model);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }


    [TestMethod]
    public async Task BulkCreate_ReturnsBadRequest_WhenEmailIsInvalid()
    {
        var context = GetDbContext();

        context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Test Event",
            EventDate = DateTime.Now
        });

        await context.SaveChangesAsync();

        var controller = new GuestsController(context);

        var list = new List<GuestCreateDto>
    {
        new GuestCreateDto
        {
            FirstName = "Alice",
            LastName = "Brown",
            Email = "invalid-email" 
        },
        new GuestCreateDto
        {
            FirstName = "Bob",
            LastName = "Smith",
            Email = "bob@test.com"
        }
    };

        var result = await controller.BulkCreate(1, list);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

}

