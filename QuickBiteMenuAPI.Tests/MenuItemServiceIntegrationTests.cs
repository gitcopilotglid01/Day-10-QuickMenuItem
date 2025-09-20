using Microsoft.EntityFrameworkCore;
using QuickBiteMenuAPI.Data;
using QuickBiteMenuAPI.Models;
using QuickBiteMenuAPI.Services;
using Xunit;

namespace QuickBiteMenuAPI.Tests
{
    public class MenuItemServiceIntegrationTests : IDisposable
    {
        private readonly MenuDbContext _context;
        private readonly MenuItemService _service;

        public MenuItemServiceIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<MenuDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MenuDbContext(options);
            _service = new MenuItemService(_context);
        }

        #region Data Integrity Tests

        [Fact]
        public async Task CreateMenuItemAsync_SetsCorrectTimestamps()
        {
            // Arrange
            var beforeCreate = DateTime.UtcNow;
            var createDto = new CreateMenuItemDto
            {
                Name = "Time Test Item",
                Description = "Testing timestamp creation",
                Price = 10.00m,
                Category = "Test",
                DietaryTag = "None"
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);
            var afterCreate = DateTime.UtcNow;

            // Assert
            Assert.True(result.CreatedAt >= beforeCreate && result.CreatedAt <= afterCreate);
            Assert.True(result.UpdatedAt >= beforeCreate && result.UpdatedAt <= afterCreate);
            // CreatedAt and UpdatedAt should be very close (within 1 second)
            var timeDifference = Math.Abs((result.CreatedAt - result.UpdatedAt).TotalMilliseconds);
            Assert.True(timeDifference < 1000, "CreatedAt and UpdatedAt should be set at nearly the same time");
        }

        [Fact]
        public async Task UpdateMenuItemAsync_UpdatesOnlyUpdatedAtTimestamp()
        {
            // Arrange - Create an item first
            var createDto = new CreateMenuItemDto
            {
                Name = "Original Item",
                Description = "Original description",
                Price = 10.00m,
                Category = "Original",
                DietaryTag = "None"
            };
            var createdItem = await _service.CreateMenuItemAsync(createDto);
            var originalCreatedAt = createdItem.CreatedAt;
            var originalUpdatedAt = createdItem.UpdatedAt;

            // Add delay to ensure timestamp difference
            await Task.Delay(50);

            var updateDto = new UpdateMenuItemDto
            {
                Name = "Updated Item",
                Description = "Updated description",
                Price = 15.00m,
                Category = "Updated",
                DietaryTag = "Updated"
            };

            // Act
            var result = await _service.UpdateMenuItemAsync(createdItem.Id, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(originalCreatedAt, result.CreatedAt); // CreatedAt should not change
            Assert.True(result.UpdatedAt > originalUpdatedAt); // UpdatedAt should be newer
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public async Task CreateAndRetrieve_FullWorkflow_Success()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "Workflow Test Pizza",
                Description = "Testing full CRUD workflow",
                Price = 18.99m,
                Category = "Main Course",
                DietaryTag = "Vegetarian"
            };

            // Act & Assert - Create
            var created = await _service.CreateMenuItemAsync(createDto);
            Assert.NotNull(created);
            Assert.True(created.Id > 0);

            // Act & Assert - Retrieve by ID
            var retrieved = await _service.GetMenuItemByIdAsync(created.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(created.Id, retrieved.Id);
            Assert.Equal("Workflow Test Pizza", retrieved.Name);

            // Act & Assert - Retrieve by Category
            var categoryItems = await _service.GetMenuItemsByCategoryAsync("Main Course");
            Assert.Contains(categoryItems, item => item.Id == created.Id);

            // Act & Assert - Retrieve by Dietary Tag
            var dietaryItems = await _service.GetMenuItemsByDietaryTagAsync("Vegetarian");
            Assert.Contains(dietaryItems, item => item.Id == created.Id);

            // Act & Assert - Check existence
            var exists = await _service.MenuItemExistsAsync(created.Id);
            Assert.True(exists);
        }

        [Fact]
        public async Task UpdateAndVerify_FullWorkflow_Success()
        {
            // Arrange - Create an item
            var createDto = new CreateMenuItemDto
            {
                Name = "Original Pizza",
                Description = "Original description",
                Price = 12.00m,
                Category = "Main Course",
                DietaryTag = "None"
            };
            var created = await _service.CreateMenuItemAsync(createDto);

            var updateDto = new UpdateMenuItemDto
            {
                Name = "Updated Pizza",
                Description = "Updated description",
                Price = 15.00m,
                Category = "Premium",
                DietaryTag = "Vegetarian"
            };

            // Act - Update
            var updated = await _service.UpdateMenuItemAsync(created.Id, updateDto);

            // Assert - Verify update
            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal("Updated Pizza", updated.Name);
            Assert.Equal("Updated description", updated.Description);
            Assert.Equal(15.00m, updated.Price);
            Assert.Equal("Premium", updated.Category);
            Assert.Equal("Vegetarian", updated.DietaryTag);

            // Verify categories changed
            var mainCourseItems = await _service.GetMenuItemsByCategoryAsync("Main Course");
            Assert.DoesNotContain(mainCourseItems, item => item.Id == created.Id);

            var premiumItems = await _service.GetMenuItemsByCategoryAsync("Premium");
            Assert.Contains(premiumItems, item => item.Id == created.Id);
        }

        [Fact]
        public async Task DeleteAndVerify_FullWorkflow_Success()
        {
            // Arrange - Create an item
            var createDto = new CreateMenuItemDto
            {
                Name = "To Be Deleted",
                Description = "This item will be deleted",
                Price = 5.00m,
                Category = "Temporary",
                DietaryTag = "None"
            };
            var created = await _service.CreateMenuItemAsync(createDto);
            var createdId = created.Id;

            // Verify it exists
            var existsBefore = await _service.MenuItemExistsAsync(createdId);
            Assert.True(existsBefore);

            // Act - Delete
            var deleteResult = await _service.DeleteMenuItemAsync(createdId);

            // Assert - Verify deletion
            Assert.True(deleteResult);

            var existsAfter = await _service.MenuItemExistsAsync(createdId);
            Assert.False(existsAfter);

            var retrieveAfter = await _service.GetMenuItemByIdAsync(createdId);
            Assert.Null(retrieveAfter);

            var categoryItemsAfter = await _service.GetMenuItemsByCategoryAsync("Temporary");
            Assert.DoesNotContain(categoryItemsAfter, item => item.Id == createdId);
        }

        #endregion

        #region Filtering and Search Tests

        [Fact]
        public async Task FilteringTests_MultipleItemsAndFilters()
        {
            // Arrange - Create diverse test data
            var items = new List<CreateMenuItemDto>
            {
                new() { Name = "Veggie Burger", Description = "Plant-based burger", Price = 12.99m, Category = "Main Course", DietaryTag = "Vegetarian" },
                new() { Name = "Chicken Salad", Description = "Grilled chicken on greens", Price = 11.99m, Category = "Main Course", DietaryTag = "Gluten-Free" },
                new() { Name = "Fruit Salad", Description = "Fresh seasonal fruits", Price = 6.99m, Category = "Dessert", DietaryTag = "Vegan" },
                new() { Name = "Cheese Platter", Description = "Assorted cheeses", Price = 14.99m, Category = "Appetizer", DietaryTag = "Vegetarian" },
                new() { Name = "Fish Tacos", Description = "Grilled fish with salsa", Price = 13.99m, Category = "Main Course", DietaryTag = "None" }
            };

            foreach (var item in items)
            {
                await _service.CreateMenuItemAsync(item);
            }

            // Act & Assert - Category filtering
            var mainCourses = await _service.GetMenuItemsByCategoryAsync("Main Course");
            Assert.Equal(3, mainCourses.Count());

            var desserts = await _service.GetMenuItemsByCategoryAsync("Dessert");
            Assert.Single(desserts);

            var appetizers = await _service.GetMenuItemsByCategoryAsync("Appetizer");
            Assert.Single(appetizers);

            // Act & Assert - Dietary tag filtering
            var vegetarian = await _service.GetMenuItemsByDietaryTagAsync("Vegetarian");
            Assert.Equal(2, vegetarian.Count());

            var glutenFree = await _service.GetMenuItemsByDietaryTagAsync("Gluten-Free");
            Assert.Single(glutenFree);

            var vegan = await _service.GetMenuItemsByDietaryTagAsync("Vegan");
            Assert.Single(vegan);

            var none = await _service.GetMenuItemsByDietaryTagAsync("None");
            Assert.Single(none);
        }

        #endregion

        #region Performance and Load Tests

        [Fact]
        public async Task BulkOperations_PerformanceTest()
        {
            // Arrange - Create 50 items
            var createTasks = new List<Task<MenuItemDto>>();
            for (int i = 0; i < 50; i++)
            {
                var createDto = new CreateMenuItemDto
                {
                    Name = $"Bulk Item {i}",
                    Description = $"Bulk description {i}",
                    Price = (decimal)(10 + (i * 0.5)),
                    Category = $"Category {i % 5}",
                    DietaryTag = $"Tag {i % 3}"
                };
                createTasks.Add(_service.CreateMenuItemAsync(createDto));
            }

            // Act
            var created = await Task.WhenAll(createTasks);

            // Assert
            Assert.Equal(50, created.Length);
            Assert.All(created, item => Assert.True(item.Id > 0));

            // Test bulk retrieval
            var allItems = await _service.GetAllMenuItemsAsync();
            Assert.True(allItems.Count() >= 50);

            // Test category filtering with bulk data
            var category0Items = await _service.GetMenuItemsByCategoryAsync("Category 0");
            Assert.Equal(10, category0Items.Count()); // 50 items / 5 categories = 10 per category
        }

        #endregion

        #region Data Validation Tests

        [Fact]
        public async Task StringTrimming_VariousWhitespaceScenarios()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "\t\r\n  Whitespace Test  \t\r\n",
                Description = "  \t  Description with spaces  \r\n  ",
                Price = 10.00m,
                Category = "\n\t Category Test \t\n",
                DietaryTag = "  Tag Test  "
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            Assert.Equal("Whitespace Test", result.Name);
            Assert.Equal("Description with spaces", result.Description);
            Assert.Equal("Category Test", result.Category);
            Assert.Equal("Tag Test", result.DietaryTag);
        }

        [Fact]
        public async Task CaseInsensitiveFiltering_VariousCases()
        {
            // Arrange
            await _service.CreateMenuItemAsync(new CreateMenuItemDto
            {
                Name = "Test Item",
                Description = "Test",
                Price = 10.00m,
                Category = "Main Course",
                DietaryTag = "Vegetarian"
            });

            // Act & Assert - Test various case combinations
            var testCases = new[]
            {
                "main course", "MAIN COURSE", "Main Course", "mAiN cOuRsE"
            };

            foreach (var testCase in testCases)
            {
                var result = await _service.GetMenuItemsByCategoryAsync(testCase);
                Assert.Single(result);
            }

            var dietaryTestCases = new[]
            {
                "vegetarian", "VEGETARIAN", "Vegetarian", "vEgEtArIaN"
            };

            foreach (var testCase in dietaryTestCases)
            {
                var result = await _service.GetMenuItemsByDietaryTagAsync(testCase);
                Assert.Single(result);
            }
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}