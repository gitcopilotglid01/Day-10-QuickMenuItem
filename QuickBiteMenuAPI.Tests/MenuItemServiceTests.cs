using Microsoft.EntityFrameworkCore;
using QuickBiteMenuAPI.Data;
using QuickBiteMenuAPI.Models;
using QuickBiteMenuAPI.Services;
using Xunit;

namespace QuickBiteMenuAPI.Tests
{
    public class MenuItemServiceTests : IDisposable
    {
        private readonly MenuDbContext _context;
        private readonly MenuItemService _service;

        public MenuItemServiceTests()
        {
            var options = new DbContextOptionsBuilder<MenuDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MenuDbContext(options);
            _service = new MenuItemService(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    Name = "Margherita Pizza",
                    Description = "Classic pizza with tomato sauce, mozzarella, and fresh basil",
                    Price = 12.99m,
                    Category = "Main Course",
                    DietaryTag = "Vegetarian",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new MenuItem
                {
                    Id = 2,
                    Name = "Caesar Salad",
                    Description = "Fresh romaine lettuce with Caesar dressing, croutons, and parmesan",
                    Price = 8.99m,
                    Category = "Appetizer",
                    DietaryTag = "Vegetarian",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new MenuItem
                {
                    Id = 3,
                    Name = "Grilled Chicken Breast",
                    Description = "Tender grilled chicken breast with herbs and spices",
                    Price = 16.99m,
                    Category = "Main Course",
                    DietaryTag = "Gluten-Free",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new MenuItem
                {
                    Id = 4,
                    Name = "Chocolate Cake",
                    Description = "Rich chocolate cake with chocolate frosting",
                    Price = 6.99m,
                    Category = "Dessert",
                    DietaryTag = "None",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _context.MenuItems.AddRange(menuItems);
            _context.SaveChanges();
        }

        #region GetAllMenuItemsAsync Tests

        [Fact]
        public async Task GetAllMenuItemsAsync_ReturnsAllItems_OrderedByCategoryThenName()
        {
            // Act
            var result = await _service.GetAllMenuItemsAsync();
            var resultList = result.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, resultList.Count);

            // Verify ordering: Appetizer, Dessert, Main Course (2 items)
            Assert.Equal("Caesar Salad", resultList[0].Name);
            Assert.Equal("Appetizer", resultList[0].Category);
            
            Assert.Equal("Chocolate Cake", resultList[1].Name);
            Assert.Equal("Dessert", resultList[1].Category);
            
            Assert.Equal("Grilled Chicken Breast", resultList[2].Name);
            Assert.Equal("Main Course", resultList[2].Category);
            
            Assert.Equal("Margherita Pizza", resultList[3].Name);
            Assert.Equal("Main Course", resultList[3].Category);
        }

        [Fact]
        public async Task GetAllMenuItemsAsync_EmptyDatabase_ReturnsEmptyCollection()
        {
            // Arrange
            _context.MenuItems.RemoveRange(_context.MenuItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllMenuItemsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetMenuItemByIdAsync Tests

        [Fact]
        public async Task GetMenuItemByIdAsync_ExistingId_ReturnsMenuItemDto()
        {
            // Act
            var result = await _service.GetMenuItemByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Margherita Pizza", result.Name);
            Assert.Equal("Classic pizza with tomato sauce, mozzarella, and fresh basil", result.Description);
            Assert.Equal(12.99m, result.Price);
            Assert.Equal("Main Course", result.Category);
            Assert.Equal("Vegetarian", result.DietaryTag);
        }

        [Fact]
        public async Task GetMenuItemByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _service.GetMenuItemByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMenuItemByIdAsync_NegativeId_ReturnsNull()
        {
            // Act
            var result = await _service.GetMenuItemByIdAsync(-1);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetMenuItemsByCategoryAsync Tests

        [Fact]
        public async Task GetMenuItemsByCategoryAsync_ExistingCategory_ReturnsMatchingItems()
        {
            // Act
            var result = await _service.GetMenuItemsByCategoryAsync("Main Course");
            var resultList = result.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, item => Assert.Equal("Main Course", item.Category));
            
            // Verify ordering by name
            Assert.Equal("Grilled Chicken Breast", resultList[0].Name);
            Assert.Equal("Margherita Pizza", resultList[1].Name);
        }

        [Fact]
        public async Task GetMenuItemsByCategoryAsync_CaseInsensitive_ReturnsMatchingItems()
        {
            // Act
            var result = await _service.GetMenuItemsByCategoryAsync("MAIN COURSE");
            var resultList = result.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, item => Assert.Equal("Main Course", item.Category));
        }

        [Fact]
        public async Task GetMenuItemsByCategoryAsync_NonExistingCategory_ReturnsEmptyCollection()
        {
            // Act
            var result = await _service.GetMenuItemsByCategoryAsync("NonExistent");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMenuItemsByCategoryAsync_EmptyCategory_ReturnsEmptyCollection()
        {
            // Act
            var result = await _service.GetMenuItemsByCategoryAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetMenuItemsByDietaryTagAsync Tests

        [Fact]
        public async Task GetMenuItemsByDietaryTagAsync_ExistingTag_ReturnsMatchingItems()
        {
            // Act
            var result = await _service.GetMenuItemsByDietaryTagAsync("Vegetarian");
            var resultList = result.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, item => Assert.Equal("Vegetarian", item.DietaryTag));
            
            // Verify ordering by category then name
            Assert.Equal("Caesar Salad", resultList[0].Name);
            Assert.Equal("Margherita Pizza", resultList[1].Name);
        }

        [Fact]
        public async Task GetMenuItemsByDietaryTagAsync_CaseInsensitive_ReturnsMatchingItems()
        {
            // Act
            var result = await _service.GetMenuItemsByDietaryTagAsync("VEGETARIAN");
            var resultList = result.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, item => Assert.Equal("Vegetarian", item.DietaryTag));
        }

        [Fact]
        public async Task GetMenuItemsByDietaryTagAsync_NonExistingTag_ReturnsEmptyCollection()
        {
            // Act
            var result = await _service.GetMenuItemsByDietaryTagAsync("Vegan");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region CreateMenuItemAsync Tests

        [Fact]
        public async Task CreateMenuItemAsync_ValidDto_CreatesAndReturnsMenuItem()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "   New Item   ",
                Description = "   A new test item   ",
                Price = 15.99m,
                Category = "   Test Category   ",
                DietaryTag = "   Test Tag   "
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("New Item", result.Name); // Trimmed
            Assert.Equal("A new test item", result.Description); // Trimmed
            Assert.Equal(15.99m, result.Price);
            Assert.Equal("Test Category", result.Category); // Trimmed
            Assert.Equal("Test Tag", result.DietaryTag); // Trimmed
            Assert.True(result.CreatedAt <= DateTime.UtcNow);
            Assert.True(result.UpdatedAt <= DateTime.UtcNow);

            // Verify item was saved to database
            var savedItem = await _context.MenuItems.FindAsync(result.Id);
            Assert.NotNull(savedItem);
            Assert.Equal("New Item", savedItem.Name);
        }

        [Fact]
        public async Task CreateMenuItemAsync_TrimsWhitespace_SavesTrimmedValues()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "   Spaced Name   ",
                Description = "   Spaced Description   ",
                Price = 10.00m,
                Category = "   Spaced Category   ",
                DietaryTag = "   Spaced Tag   "
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            Assert.Equal("Spaced Name", result.Name);
            Assert.Equal("Spaced Description", result.Description);
            Assert.Equal("Spaced Category", result.Category);
            Assert.Equal("Spaced Tag", result.DietaryTag);
        }

        #endregion

        #region UpdateMenuItemAsync Tests

        [Fact]
        public async Task UpdateMenuItemAsync_ExistingId_UpdatesAndReturnsMenuItem()
        {
            // Arrange
            var updateDto = new UpdateMenuItemDto
            {
                Name = "   Updated Pizza   ",
                Description = "   Updated description   ",
                Price = 14.99m,
                Category = "   Updated Category   ",
                DietaryTag = "   Updated Tag   "
            };

            // Act
            var result = await _service.UpdateMenuItemAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Updated Pizza", result.Name); // Trimmed
            Assert.Equal("Updated description", result.Description); // Trimmed
            Assert.Equal(14.99m, result.Price);
            Assert.Equal("Updated Category", result.Category); // Trimmed
            Assert.Equal("Updated Tag", result.DietaryTag); // Trimmed

            // Verify item was updated in database
            var updatedItem = await _context.MenuItems.FindAsync(1);
            Assert.NotNull(updatedItem);
            Assert.Equal("Updated Pizza", updatedItem.Name);
            Assert.Equal(14.99m, updatedItem.Price);
        }

        [Fact]
        public async Task UpdateMenuItemAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var updateDto = new UpdateMenuItemDto
            {
                Name = "Updated Item",
                Description = "Updated description",
                Price = 10.00m,
                Category = "Updated Category",
                DietaryTag = "Updated Tag"
            };

            // Act
            var result = await _service.UpdateMenuItemAsync(999, updateDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateMenuItemAsync_UpdatesTimestamp()
        {
            // Arrange
            var originalItem = await _context.MenuItems.FindAsync(1);
            var originalUpdatedAt = originalItem!.UpdatedAt;
            
            var updateDto = new UpdateMenuItemDto
            {
                Name = "Updated Name",
                Description = "Updated description",
                Price = 10.00m,
                Category = "Updated Category",
                DietaryTag = "Updated Tag"
            };

            // Add a small delay to ensure timestamp difference
            await Task.Delay(10);

            // Act
            var result = await _service.UpdateMenuItemAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.UpdatedAt > originalUpdatedAt);
        }

        #endregion

        #region DeleteMenuItemAsync Tests

        [Fact]
        public async Task DeleteMenuItemAsync_ExistingId_DeletesItemAndReturnsTrue()
        {
            // Act
            var result = await _service.DeleteMenuItemAsync(1);

            // Assert
            Assert.True(result);

            // Verify item was deleted from database
            var deletedItem = await _context.MenuItems.FindAsync(1);
            Assert.Null(deletedItem);

            // Verify other items still exist
            var remainingItems = await _context.MenuItems.CountAsync();
            Assert.Equal(3, remainingItems);
        }

        [Fact]
        public async Task DeleteMenuItemAsync_NonExistingId_ReturnsFalse()
        {
            // Act
            var result = await _service.DeleteMenuItemAsync(999);

            // Assert
            Assert.False(result);

            // Verify no items were deleted
            var itemCount = await _context.MenuItems.CountAsync();
            Assert.Equal(4, itemCount);
        }

        #endregion

        #region MenuItemExistsAsync Tests

        [Fact]
        public async Task MenuItemExistsAsync_ExistingId_ReturnsTrue()
        {
            // Act
            var result = await _service.MenuItemExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task MenuItemExistsAsync_NonExistingId_ReturnsFalse()
        {
            // Act
            var result = await _service.MenuItemExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MenuItemExistsAsync_NegativeId_ReturnsFalse()
        {
            // Act
            var result = await _service.MenuItemExistsAsync(-1);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Edge Cases and Performance Tests

        [Fact]
        public async Task Service_HandlesConcurrentOperations()
        {
            // Arrange
            var tasks = new List<Task>();

            // Act - Perform multiple concurrent read operations
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_service.GetAllMenuItemsAsync());
                tasks.Add(_service.GetMenuItemByIdAsync(1));
                tasks.Add(_service.MenuItemExistsAsync(2));
            }

            // Assert - All operations complete successfully
            await Task.WhenAll(tasks);
            Assert.True(tasks.All(t => t.IsCompletedSuccessfully));
        }

        [Fact]
        public async Task Service_HandlesLargeDataSet()
        {
            // Arrange - Add many items
            var items = new List<MenuItem>();
            for (int i = 100; i < 200; i++)
            {
                items.Add(new MenuItem
                {
                    Id = i,
                    Name = $"Item {i}",
                    Description = $"Description {i}",
                    Price = i * 0.99m,
                    Category = $"Category {i % 5}",
                    DietaryTag = $"Tag {i % 3}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            _context.MenuItems.AddRange(items);
            await _context.SaveChangesAsync();

            // Act
            var allItems = await _service.GetAllMenuItemsAsync();
            var categoryItems = await _service.GetMenuItemsByCategoryAsync("Category 1");

            // Assert
            Assert.True(allItems.Count() >= 100);
            Assert.NotEmpty(categoryItems);
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}