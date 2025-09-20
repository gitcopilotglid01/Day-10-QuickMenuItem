using Microsoft.EntityFrameworkCore;
using QuickBiteMenuAPI.Data;
using QuickBiteMenuAPI.Models;
using QuickBiteMenuAPI.Services;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace QuickBiteMenuAPI.Tests
{
    public class MenuItemServiceEdgeCasesTests : IDisposable
    {
        private readonly MenuDbContext _context;
        private readonly MenuItemService _service;

        public MenuItemServiceEdgeCasesTests()
        {
            var options = new DbContextOptionsBuilder<MenuDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MenuDbContext(options);
            _service = new MenuItemService(_context);
        }

        #region Null and Empty String Handling

        [Fact]
        public async Task GetMenuItemsByCategoryAsync_NullCategory_ReturnsEmptyCollection()
        {
            // Arrange - Add test data
            await SeedTestData();

            // Act
            var result = await _service.GetMenuItemsByCategoryAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMenuItemsByCategoryAsync_WhitespaceOnlyCategory_ReturnsEmptyCollection()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetMenuItemsByCategoryAsync("   \t\n   ");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMenuItemsByDietaryTagAsync_NullTag_ReturnsEmptyCollection()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetMenuItemsByDietaryTagAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMenuItemsByDietaryTagAsync_WhitespaceOnlyTag_ReturnsEmptyCollection()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetMenuItemsByDietaryTagAsync("   \t\r\n   ");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Boundary Value Testing

        [Fact]
        public async Task CreateMenuItemAsync_MinimumValidValues_Success()
        {
            // Arrange - Test minimum boundary values
            var createDto = new CreateMenuItemDto
            {
                Name = "A", // Minimum 1 character
                Description = "", // Empty description allowed
                Price = 0.01m, // Minimum price
                Category = "C", // Minimum 1 character
                DietaryTag = "" // Empty dietary tag should default to "None"
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("A", result.Name);
            Assert.Equal("", result.Description);
            Assert.Equal(0.01m, result.Price);
            Assert.Equal("C", result.Category);
            Assert.Equal("", result.DietaryTag); // Service doesn't enforce default
        }

        [Fact]
        public async Task CreateMenuItemAsync_MaximumValidValues_Success()
        {
            // Arrange - Test maximum boundary values
            var createDto = new CreateMenuItemDto
            {
                Name = new string('A', 100), // Maximum 100 characters
                Description = new string('D', 500), // Maximum 500 characters
                Price = 999.99m, // Maximum price
                Category = new string('C', 50), // Maximum 50 characters
                DietaryTag = new string('T', 50) // Maximum 50 characters
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Name.Length);
            Assert.Equal(500, result.Description.Length);
            Assert.Equal(999.99m, result.Price);
            Assert.Equal(50, result.Category.Length);
            Assert.Equal(50, result.DietaryTag.Length);
        }

        [Fact]
        public async Task CreateMenuItemAsync_ZeroPrice_Success()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "Free Item",
                Description = "A free menu item",
                Price = 0.00m, // Edge case: zero price
                Category = "Special",
                DietaryTag = "Free"
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.00m, result.Price);
        }

        #endregion

        #region Special Character and Unicode Handling

        [Fact]
        public async Task CreateMenuItemAsync_SpecialCharactersInName_Success()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "Café au Lait & Crème Brûlée™ ñoño",
                Description = "Special chars: @#$%^&*()_+-=[]{}|;':\",./<>?",
                Price = 12.99m,
                Category = "Beverages & Desserts",
                DietaryTag = "Gluten-Free™"
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Café au Lait & Crème Brûlée™ ñoño", result.Name);
            Assert.Contains("@#$%^&*()", result.Description);
        }

        [Fact]
        public async Task CreateMenuItemAsync_EmojiAndUnicodeCharacters_Success()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "🍕 Pizza Margherita 🇮🇹",
                Description = "Delicious pizza with 🍅 tomatoes and 🧀 cheese",
                Price = 15.99m,
                Category = "Main Course 🍽️",
                DietaryTag = "Vegetarian 🌱"
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("🍕 Pizza Margherita 🇮🇹", result.Name);
            Assert.Contains("🍅", result.Description);
            Assert.Contains("🧀", result.Description);
        }

        #endregion

        #region Case Sensitivity Edge Cases

        [Fact]
        public async Task GetMenuItemsByCategoryAsync_CaseSensitivityEdgeCases()
        {
            // Arrange
            await _context.MenuItems.AddRangeAsync(
                new MenuItem
                {
                    Name = "Item 1", Description = "Desc", Price = 10m, Category = "MainCourse",
                    DietaryTag = "None", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Name = "Item 2", Description = "Desc", Price = 10m, Category = "MAINCOURSE",
                    DietaryTag = "None", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Name = "Item 3", Description = "Desc", Price = 10m, Category = "maincourse",
                    DietaryTag = "None", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                }
            );
            await _context.SaveChangesAsync();

            // Act & Assert - All should return the same 3 items due to case insensitive search
            var result1 = await _service.GetMenuItemsByCategoryAsync("MainCourse");
            var result2 = await _service.GetMenuItemsByCategoryAsync("MAINCOURSE");
            var result3 = await _service.GetMenuItemsByCategoryAsync("maincourse");

            Assert.Equal(3, result1.Count());
            Assert.Equal(3, result2.Count());
            Assert.Equal(3, result3.Count());
        }

        #endregion

        #region Database Constraint and Error Scenarios

        [Fact]
        public async Task UpdateMenuItemAsync_ConcurrentModification_HandlesCorrectly()
        {
            // Arrange - Create an item
            var createDto = new CreateMenuItemDto
            {
                Name = "Original Item",
                Description = "Original",
                Price = 10.00m,
                Category = "Test",
                DietaryTag = "None"
            };
            var created = await _service.CreateMenuItemAsync(createDto);

            // Simulate concurrent modification by directly updating the database
            var directItem = await _context.MenuItems.FindAsync(created.Id);
            directItem!.Name = "Modified Directly";
            await _context.SaveChangesAsync();

            var updateDto = new UpdateMenuItemDto
            {
                Name = "Updated via Service",
                Description = "Updated description",
                Price = 15.00m,
                Category = "Updated",
                DietaryTag = "Updated"
            };

            // Act - Update should succeed (last writer wins in this implementation)
            var result = await _service.UpdateMenuItemAsync(created.Id, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated via Service", result.Name);
        }

        [Fact]
        public async Task DeleteMenuItemAsync_AlreadyDeletedItem_ReturnsFalse()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "To Delete",
                Description = "Will be deleted",
                Price = 10.00m,
                Category = "Test",
                DietaryTag = "None"
            };
            var created = await _service.CreateMenuItemAsync(createDto);

            // First deletion
            var firstDelete = await _service.DeleteMenuItemAsync(created.Id);
            Assert.True(firstDelete);

            // Act - Second deletion attempt
            var secondDelete = await _service.DeleteMenuItemAsync(created.Id);

            // Assert
            Assert.False(secondDelete);
        }

        #endregion

        #region Extreme Data Volume Tests

        [Fact]
        public async Task GetAllMenuItemsAsync_LargeDataset_PerformanceTest()
        {
            // Arrange - Create 1000 items
            var items = new List<MenuItem>();
            for (int i = 1; i <= 1000; i++)
            {
                items.Add(new MenuItem
                {
                    Name = $"Item {i:D4}",
                    Description = $"Description for item {i}",
                    Price = (decimal)(i % 100 + 1),
                    Category = $"Category {i % 10}",
                    DietaryTag = $"Tag {i % 5}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            _context.MenuItems.AddRange(items);
            await _context.SaveChangesAsync();

            // Act
            var startTime = DateTime.UtcNow;
            var result = await _service.GetAllMenuItemsAsync();
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Assert
            Assert.Equal(1000, result.Count());
            Assert.True(duration.TotalSeconds < 5, $"Query took too long: {duration.TotalSeconds} seconds");
            
            // Verify ordering (by Category, then Name)
            var resultList = result.ToList();
            for (int i = 1; i < resultList.Count; i++)
            {
                var current = resultList[i];
                var previous = resultList[i - 1];
                
                var categoryComparison = string.Compare(current.Category, previous.Category, StringComparison.OrdinalIgnoreCase);
                if (categoryComparison == 0)
                {
                    // Same category, check name ordering
                    Assert.True(string.Compare(current.Name, previous.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else
                {
                    Assert.True(categoryComparison >= 0);
                }
            }
        }

        #endregion

        #region Memory and Resource Tests

        [Fact]
        public async Task CreateAndDeleteManyItems_MemoryManagement()
        {
            // Arrange & Act - Create and immediately delete many items
            var createdIds = new List<int>();
            
            for (int i = 0; i < 100; i++)
            {
                var createDto = new CreateMenuItemDto
                {
                    Name = $"Temp Item {i}",
                    Description = $"Temporary item {i}",
                    Price = 10.00m,
                    Category = "Temporary",
                    DietaryTag = "None"
                };
                
                var created = await _service.CreateMenuItemAsync(createDto);
                createdIds.Add(created.Id);
                
                // Delete every other item immediately
                if (i % 2 == 0)
                {
                    await _service.DeleteMenuItemAsync(created.Id);
                }
            }

            // Assert - Verify final state
            var allItems = await _service.GetAllMenuItemsAsync();
            var remainingCount = allItems.Count(x => x.Category == "Temporary");
            Assert.Equal(50, remainingCount); // Should have 50 remaining (odd numbered items)
        }

        #endregion

        #region Validation Edge Cases

        [Fact]
        public async Task CreateMenuItemAsync_ExtremeWhitespace_HandlesCorrectly()
        {
            // Arrange - Test various whitespace scenarios
            var createDto = new CreateMenuItemDto
            {
                Name = "\t\r\n   Whitespace Test   \t\r\n",
                Description = "   \t  Multiple    spaces   between    words  \r\n  ",
                Price = 10.00m,
                Category = "\n\t Category With Whitespace \t\n",
                DietaryTag = "  \r\n  Tag  \t  "
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert - Verify trimming behavior
            Assert.Equal("Whitespace Test", result.Name);
            Assert.Equal("Multiple    spaces   between    words", result.Description);
            Assert.Equal("Category With Whitespace", result.Category);
            Assert.Equal("Tag", result.DietaryTag);
        }

        [Fact]
        public async Task FilteringOperations_EmptyDatabase_AllReturnEmpty()
        {
            // Act - Test all filtering operations on empty database
            var allItems = await _service.GetAllMenuItemsAsync();
            var categoryItems = await _service.GetMenuItemsByCategoryAsync("AnyCategory");
            var dietaryItems = await _service.GetMenuItemsByDietaryTagAsync("AnyTag");
            var itemById = await _service.GetMenuItemByIdAsync(999);
            var exists = await _service.MenuItemExistsAsync(999);

            // Assert
            Assert.Empty(allItems);
            Assert.Empty(categoryItems);
            Assert.Empty(dietaryItems);
            Assert.Null(itemById);
            Assert.False(exists);
        }

        #endregion

        #region Decimal Precision Tests

        [Fact]
        public async Task CreateMenuItemAsync_DecimalPrecisionEdgeCases()
        {
            // Arrange - Test various decimal precision scenarios
            var testCases = new[]
            {
                0.01m,    // Minimum
                0.99m,    // Less than 1
                1.00m,    // Exactly 1
                12.34m,   // Standard precision
                99.99m,   // High two-digit
                123.45m,  // Three-digit
                999.99m   // Maximum
            };

            // Act & Assert
            foreach (var price in testCases)
            {
                var createDto = new CreateMenuItemDto
                {
                    Name = $"Price Test {price}",
                    Description = "Testing decimal precision",
                    Price = price,
                    Category = "Test",
                    DietaryTag = "None"
                };

                var result = await _service.CreateMenuItemAsync(createDto);
                Assert.Equal(price, result.Price);
            }
        }

        #endregion

        private async Task SeedTestData()
        {
            var items = new[]
            {
                new MenuItem
                {
                    Name = "Test Item 1", Description = "Desc 1", Price = 10m, Category = "Category1",
                    DietaryTag = "Tag1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Name = "Test Item 2", Description = "Desc 2", Price = 20m, Category = "Category2",
                    DietaryTag = "Tag2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                }
            };
            
            _context.MenuItems.AddRange(items);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}