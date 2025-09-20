using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickBiteMenuAPI.Controllers;
using QuickBiteMenuAPI.Data;
using QuickBiteMenuAPI.Models;
using QuickBiteMenuAPI.Services;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace QuickBiteMenuAPI.Tests
{
    public class MenuItemControllerEdgeCasesTests : IDisposable
    {
        private readonly MenuDbContext _context;
        private readonly MenuItemService _service;
        private readonly MenuItemController _controller;

        public MenuItemControllerEdgeCasesTests()
        {
            var options = new DbContextOptionsBuilder<MenuDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MenuDbContext(options);
            _service = new MenuItemService(_context);
            _controller = new MenuItemController(_service);
        }

        #region Invalid ID Edge Cases

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        [InlineData(int.MinValue)]
        public async Task GetMenuItem_InvalidIds_ReturnsNotFound(int invalidId)
        {
            // Act
            var result = await _controller.GetMenuItem(invalidId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        [InlineData(int.MinValue)]
        public async Task UpdateMenuItem_InvalidIds_ReturnsNotFound(int invalidId)
        {
            // Arrange
            var updateDto = new UpdateMenuItemDto
            {
                Name = "Test",
                Description = "Test",
                Price = 10.00m,
                Category = "Test",
                DietaryTag = "Test"
            };

            // Act
            var result = await _controller.UpdateMenuItem(invalidId, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        [InlineData(int.MinValue)]
        public async Task DeleteMenuItem_InvalidIds_ReturnsNotFound(int invalidId)
        {
            // Act
            var result = await _controller.DeleteMenuItem(invalidId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Extreme Boundary Values

        [Fact]
        public async Task CreateMenuItem_MaximumBoundaryValues_ReturnsCreated()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = new string('A', 100), // Maximum length
                Description = new string('D', 500), // Maximum length
                Price = 999.99m, // Maximum price
                Category = new string('C', 50), // Maximum length
                DietaryTag = new string('T', 50) // Maximum length
            };

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var menuItem = Assert.IsType<MenuItemDto>(createdResult.Value);
            Assert.Equal(100, menuItem.Name.Length);
            Assert.Equal(500, menuItem.Description.Length);
            Assert.Equal(999.99m, menuItem.Price);
        }

        [Fact]
        public async Task CreateMenuItem_MinimumBoundaryValues_ReturnsCreated()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "A", // Minimum length
                Description = "", // Empty allowed
                Price = 0.01m, // Minimum price
                Category = "C", // Minimum length
                DietaryTag = "" // Empty allowed
            };

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var menuItem = Assert.IsType<MenuItemDto>(createdResult.Value);
            Assert.Equal("A", menuItem.Name);
            Assert.Equal(0.01m, menuItem.Price);
        }

        #endregion

        #region Null and Empty Parameter Handling

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public async Task GetMenuItemsByCategory_InvalidCategories_ReturnsEmptyOkResult(string? category)
        {
            // Act
            var result = await _controller.GetMenuItemsByCategory(category!);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<MenuItemDto>>(okResult.Value);
            Assert.Empty(items);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public async Task GetMenuItemsByDietaryTag_InvalidTags_ReturnsEmptyOkResult(string? dietaryTag)
        {
            // Act
            var result = await _controller.GetMenuItemsByDietaryTag(dietaryTag!);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<MenuItemDto>>(okResult.Value);
            Assert.Empty(items);
        }

        #endregion

        #region Special Characters and Unicode

        [Fact]
        public async Task CreateMenuItem_SpecialCharacters_HandledCorrectly()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "Café & Crème Brûlée™",
                Description = "Special chars: @#$%^&*()_+-=[]{}|;':\",./<>?`~",
                Price = 15.99m,
                Category = "Desserts & Beverages",
                DietaryTag = "Gluten-Free™"
            };

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var menuItem = Assert.IsType<MenuItemDto>(createdResult.Value);
            Assert.Equal("Café & Crème Brûlée™", menuItem.Name);
            Assert.Contains("@#$%^&*()", menuItem.Description);
        }

        [Fact]
        public async Task CreateMenuItem_EmojiCharacters_HandledCorrectly()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "🍕 Pizza Margherita 🇮🇹",
                Description = "Delicious pizza with 🍅 tomatoes and 🧀 cheese",
                Price = 18.99m,
                Category = "Main Course 🍽️",
                DietaryTag = "Vegetarian 🌱"
            };

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var menuItem = Assert.IsType<MenuItemDto>(createdResult.Value);
            Assert.Equal("🍕 Pizza Margherita 🇮🇹", menuItem.Name);
            Assert.Contains("🍅", menuItem.Description);
        }

        #endregion

        #region Decimal Precision Edge Cases

        [Theory]
        [InlineData(0.01)]
        [InlineData(0.99)]
        [InlineData(1.00)]
        [InlineData(12.34)]
        [InlineData(99.99)]
        [InlineData(123.45)]
        [InlineData(999.99)]
        public async Task CreateMenuItem_VariousDecimalPrices_HandledCorrectly(double priceValue)
        {
            // Arrange
            var price = (decimal)priceValue;
            var createDto = new CreateMenuItemDto
            {
                Name = $"Price Test {price}",
                Description = "Testing decimal precision",
                Price = price,
                Category = "Test",
                DietaryTag = "None"
            };

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var menuItem = Assert.IsType<MenuItemDto>(createdResult.Value);
            Assert.Equal(price, menuItem.Price);
        }

        #endregion

        #region Case Sensitivity Tests

        [Fact]
        public async Task GetMenuItemsByCategory_CaseInsensitive_ReturnsAllMatches()
        {
            // Arrange - Create items with different case categories
            await SeedDataWithVariedCase();

            var testCases = new[] { "maincourse", "MAINCOURSE", "MainCourse", "mAiNcOuRsE" };

            foreach (var category in testCases)
            {
                // Act
                var result = await _controller.GetMenuItemsByCategory(category);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var items = Assert.IsAssignableFrom<IEnumerable<MenuItemDto>>(okResult.Value).ToList();
                Assert.Equal(3, items.Count); // Should return all 3 items regardless of case
            }
        }

        [Fact]
        public async Task GetMenuItemsByDietaryTag_CaseInsensitive_ReturnsAllMatches()
        {
            // Arrange
            await SeedDataWithVariedCase();

            var testCases = new[] { "vegetarian", "VEGETARIAN", "Vegetarian", "vEgEtArIaN" };

            foreach (var tag in testCases)
            {
                // Act
                var result = await _controller.GetMenuItemsByDietaryTag(tag);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var items = Assert.IsAssignableFrom<IEnumerable<MenuItemDto>>(okResult.Value).ToList();
                Assert.Equal(2, items.Count); // Should return all 2 vegetarian items
            }
        }

        #endregion

        #region Whitespace Handling

        [Fact]
        public async Task CreateMenuItem_ExcessiveWhitespace_TrimmedCorrectly()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "\t\r\n   Whitespace Test   \t\r\n",
                Description = "   \t  Multiple    spaces   between    words  \r\n  ",
                Price = 10.00m,
                Category = "\n\t Category With Whitespace \t\n",
                DietaryTag = "  \r\n  Tag With Spaces  \t  "
            };

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var menuItem = Assert.IsType<MenuItemDto>(createdResult.Value);
            Assert.Equal("Whitespace Test", menuItem.Name);
            Assert.Equal("Multiple    spaces   between    words", menuItem.Description);
            Assert.Equal("Category With Whitespace", menuItem.Category);
            Assert.Equal("Tag With Spaces", menuItem.DietaryTag);
        }

        #endregion

        #region Large Data Volume Tests

        [Fact]
        public async Task GetMenuItems_LargeDataset_ReturnsAllItemsOrdered()
        {
            // Arrange - Create 100 items with varied data
            await SeedLargeDataset(100);

            // Act
            var result = await _controller.GetMenuItems();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<MenuItemDto>>(okResult.Value).ToList();
            Assert.Equal(100, items.Count);

            // Verify ordering (by Category, then Name)
            for (int i = 1; i < items.Count; i++)
            {
                var current = items[i];
                var previous = items[i - 1];
                
                var categoryComparison = string.Compare(current.Category, previous.Category, StringComparison.OrdinalIgnoreCase);
                if (categoryComparison == 0)
                {
                    Assert.True(string.Compare(current.Name, previous.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else
                {
                    Assert.True(categoryComparison >= 0);
                }
            }
        }

        #endregion

        #region HTTP Status Code Edge Cases

        [Fact]
        public async Task UpdateMenuItem_NonExistentItem_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new UpdateMenuItemDto
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 25.99m,
                Category = "Updated Category",
                DietaryTag = "Updated Tag"
            };

            // Act
            var result = await _controller.UpdateMenuItem(999999, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteMenuItem_NonExistentItem_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteMenuItem(999999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetMenuItem_NonExistentItem_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetMenuItem(999999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region Concurrent Operation Simulation

        [Fact]
        public async Task CreateMultipleItems_Concurrently_AllSucceed()
        {
            // Arrange
            var createTasks = new List<Task<ActionResult<MenuItemDto>>>();
            
            for (int i = 0; i < 10; i++)
            {
                var createDto = new CreateMenuItemDto
                {
                    Name = $"Concurrent Item {i}",
                    Description = $"Created concurrently {i}",
                    Price = 10.00m + i,
                    Category = $"Category {i % 3}",
                    DietaryTag = "None"
                };
                
                createTasks.Add(_controller.CreateMenuItem(createDto));
            }

            // Act
            var results = await Task.WhenAll(createTasks);

            // Assert
            Assert.All(results, result => 
            {
                Assert.IsType<CreatedAtActionResult>(result.Result);
            });

            // Verify all items were created
            var allItems = await _controller.GetMenuItems();
            var okResult = Assert.IsType<OkObjectResult>(allItems.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<MenuItemDto>>(okResult.Value);
            Assert.Equal(10, items.Count());
        }

        #endregion

        #region Model Validation Edge Cases

        [Fact]
        public async Task CreateMenuItem_EmptyName_StillProcessed()
        {
            // Note: In this implementation, empty names are allowed by the service
            // But would typically be caught by model validation in a real scenario
            
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "", // Empty name
                Description = "Valid description",
                Price = 10.00m,
                Category = "Valid Category",
                DietaryTag = "Valid Tag"
            };

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var menuItem = Assert.IsType<MenuItemDto>(createdResult.Value);
            Assert.Equal("", menuItem.Name);
        }

        #endregion

        private async Task SeedDataWithVariedCase()
        {
            var items = new[]
            {
                new MenuItem
                {
                    Name = "Item 1", Description = "Desc 1", Price = 10m, Category = "MainCourse",
                    DietaryTag = "Vegetarian", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Name = "Item 2", Description = "Desc 2", Price = 20m, Category = "MAINCOURSE",
                    DietaryTag = "VEGETARIAN", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Name = "Item 3", Description = "Desc 3", Price = 30m, Category = "maincourse",
                    DietaryTag = "None", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                }
            };
            
            _context.MenuItems.AddRange(items);
            await _context.SaveChangesAsync();
        }

        private async Task SeedLargeDataset(int count)
        {
            var items = new List<MenuItem>();
            for (int i = 1; i <= count; i++)
            {
                items.Add(new MenuItem
                {
                    Name = $"Item {i:D3}",
                    Description = $"Description for item {i}",
                    Price = (decimal)(i % 50 + 1),
                    Category = $"Category {i % 10}",
                    DietaryTag = $"Tag {i % 5}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            
            _context.MenuItems.AddRange(items);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}