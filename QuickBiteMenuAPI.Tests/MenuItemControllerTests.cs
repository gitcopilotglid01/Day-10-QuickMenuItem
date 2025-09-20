using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickBiteMenuAPI.Controllers;
using QuickBiteMenuAPI.Data;
using QuickBiteMenuAPI.Models;
using QuickBiteMenuAPI.Services;

namespace QuickBiteMenuAPI.Tests.Controllers
{
    public class MenuItemControllerTests : IDisposable
    {
        private readonly MenuDbContext _context;
        private readonly MenuItemService _service;
        private readonly MenuItemController _controller;

        public MenuItemControllerTests()
        {
            var options = new DbContextOptionsBuilder<MenuDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MenuDbContext(options);
            _service = new MenuItemService(_context);
            _controller = new MenuItemController(_service);
        }

        [Fact]
        public async Task GetAllMenuItems_ShouldReturnOkResult_WithEmptyList()
        {
            // Act
            var result = await _controller.GetMenuItems();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var menuItems = Assert.IsAssignableFrom<IEnumerable<MenuItemDto>>(okResult.Value);
            Assert.Empty(menuItems);
        }

        [Fact]
        public async Task CreateMenuItem_ShouldReturnCreatedResult_WithValidMenuItem()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "Test Pizza",
                Description = "A delicious test pizza",
                Price = 15.99m,
                Category = "Main Course",
                DietaryTag = "Vegetarian"
            };

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var menuItem = Assert.IsType<MenuItemDto>(createdResult.Value);
            Assert.Equal("Test Pizza", menuItem.Name);
            Assert.Equal(15.99m, menuItem.Price);
        }

        [Fact]
        public async Task GetMenuItemById_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            // Act
            var result = await _controller.GetMenuItem(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateMenuItem_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateMenuItemDto
            {
                Name = "Updated Item",
                Description = "Updated description",
                Price = 20.99m,
                Category = "Main Course",
                DietaryTag = "Vegan"
            };

            // Act
            var result = await _controller.UpdateMenuItem(999, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteMenuItem_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteMenuItem(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetMenuItemById_ShouldReturnOkResult_WhenItemExists()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "Test Item",
                Description = "Test Description",
                Price = 12.99m,
                Category = "Appetizer",
                DietaryTag = "None"
            };

            var createdResult = await _controller.CreateMenuItem(createDto);
            var createdItem = (createdResult.Result as CreatedAtActionResult)?.Value as MenuItemDto;

            // Act
            var result = await _controller.GetMenuItem(createdItem!.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var menuItem = Assert.IsType<MenuItemDto>(okResult.Value);
            Assert.Equal("Test Item", menuItem.Name);
        }

        [Fact]
        public async Task UpdateMenuItem_ShouldReturnOkResult_WhenItemExists()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "Original Item",
                Description = "Original Description",
                Price = 10.99m,
                Category = "Appetizer",
                DietaryTag = "None"
            };

            var createdResult = await _controller.CreateMenuItem(createDto);
            var createdItem = (createdResult.Result as CreatedAtActionResult)?.Value as MenuItemDto;

            var updateDto = new UpdateMenuItemDto
            {
                Name = "Updated Item",
                Description = "Updated Description",
                Price = 15.99m,
                Category = "Main Course",
                DietaryTag = "Vegetarian"
            };

            // Act
            var result = await _controller.UpdateMenuItem(createdItem!.Id, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var updatedItem = Assert.IsType<MenuItemDto>(okResult.Value);
            Assert.Equal("Updated Item", updatedItem.Name);
            Assert.Equal(15.99m, updatedItem.Price);
        }

        [Fact]
        public async Task DeleteMenuItem_ShouldReturnNoContent_WhenItemExists()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "Item to Delete",
                Description = "Will be deleted",
                Price = 8.99m,
                Category = "Dessert",
                DietaryTag = "None"
            };

            var createdResult = await _controller.CreateMenuItem(createDto);
            var createdItem = (createdResult.Result as CreatedAtActionResult)?.Value as MenuItemDto;

            // Act
            var result = await _controller.DeleteMenuItem(createdItem!.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify item is deleted
            var getResult = await _controller.GetMenuItem(createdItem.Id);
            Assert.IsType<NotFoundResult>(getResult.Result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}