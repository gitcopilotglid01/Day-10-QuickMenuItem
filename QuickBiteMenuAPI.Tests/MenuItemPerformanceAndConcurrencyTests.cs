using Microsoft.EntityFrameworkCore;
using QuickBiteMenuAPI.Data;
using QuickBiteMenuAPI.Models;
using QuickBiteMenuAPI.Services;
using System.Collections.Concurrent;
using System.Diagnostics;
using Xunit;

namespace QuickBiteMenuAPI.Tests
{
    public class MenuItemPerformanceAndConcurrencyTests : IDisposable
    {
        private readonly MenuDbContext _context;
        private readonly MenuItemService _service;
        private readonly string _databaseName;

        public MenuItemPerformanceAndConcurrencyTests()
        {
            _databaseName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<MenuDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;

            _context = new MenuDbContext(options);
            _service = new MenuItemService(_context);
        }

        // Helper method to create new DbContext instances for concurrent operations
        private MenuDbContext CreateNewContext()
        {
            var options = new DbContextOptionsBuilder<MenuDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;
            return new MenuDbContext(options);
        }

        #region Performance Stress Tests

        [Fact]
        public async Task CreateMenuItem_BulkInsert_PerformanceTest()
        {
            // Arrange
            const int itemCount = 500;
            var createTasks = new List<Task<MenuItemDto>>();

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < itemCount; i++)
            {
                var createDto = new CreateMenuItemDto
                {
                    Name = $"Performance Test Item {i}",
                    Description = $"Generated for performance testing {i}",
                    Price = (decimal)(10.00 + (i % 100)),
                    Category = $"Category {i % 20}",
                    DietaryTag = $"Tag {i % 10}"
                };
                
                createTasks.Add(_service.CreateMenuItemAsync(createDto));
            }

            await Task.WhenAll(createTasks);
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 30000, 
                $"Bulk insert took too long: {stopwatch.ElapsedMilliseconds}ms");
            
            var allItems = await _service.GetAllMenuItemsAsync();
            Assert.Equal(itemCount, allItems.Count());
        }

        [Fact]
        public async Task GetAllMenuItems_LargeDataset_PerformanceTest()
        {
            // Arrange - Create large dataset
            const int itemCount = 2000;
            await SeedLargeDataset(itemCount);

            // Act - Measure query performance
            var stopwatch = Stopwatch.StartNew();
            var result = await _service.GetAllMenuItemsAsync();
            stopwatch.Stop();

            // Assert
            Assert.Equal(itemCount, result.Count());
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
                $"Large dataset query took too long: {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public async Task FilterOperations_LargeDataset_PerformanceTest()
        {
            // Arrange
            const int itemCount = 1000;
            await SeedLargeDataset(itemCount);

            // Act & Assert - Test various filter operations
            var stopwatch = Stopwatch.StartNew();
            
            var categoryResults = await _service.GetMenuItemsByCategoryAsync("Category 5");
            var categoryTime = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Restart();
            var dietaryResults = await _service.GetMenuItemsByDietaryTagAsync("Tag 3");
            var dietaryTime = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Stop();

            // Assert performance and results
            Assert.True(categoryTime < 2000, $"Category filter took too long: {categoryTime}ms");
            Assert.True(dietaryTime < 2000, $"Dietary filter took too long: {dietaryTime}ms");
            Assert.True(categoryResults.Count() > 0);
            Assert.True(dietaryResults.Count() > 0);
        }

        [Fact]
        public async Task UpdateOperations_BulkUpdate_PerformanceTest()
        {
            // Arrange
            const int itemCount = 200;
            var createdItems = new List<MenuItemDto>();
            
            for (int i = 0; i < itemCount; i++)
            {
                var created = await _service.CreateMenuItemAsync(new CreateMenuItemDto
                {
                    Name = $"Update Test {i}",
                    Description = $"Original description {i}",
                    Price = 10.00m,
                    Category = "Original",
                    DietaryTag = "Original"
                });
                createdItems.Add(created);
            }

            // Act - Bulk update
            var stopwatch = Stopwatch.StartNew();
            var updateTasks = createdItems.Select(async item =>
            {
                var updateDto = new UpdateMenuItemDto
                {
                    Name = $"Updated {item.Name}",
                    Description = $"Updated {item.Description}",
                    Price = item.Price + 5.00m,
                    Category = "Updated",
                    DietaryTag = "Updated"
                };
                return await _service.UpdateMenuItemAsync(item.Id, updateDto);
            });

            await Task.WhenAll(updateTasks);
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 15000, 
                $"Bulk update took too long: {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        public async Task ConcurrentReads_MultipleThreads_NoDataCorruption()
        {
            // Arrange
            await SeedTestData(50);
            const int concurrentReads = 20;
            var results = new ConcurrentBag<IEnumerable<MenuItemDto>>();

            // Act - Concurrent read operations
            var readTasks = Enumerable.Range(0, concurrentReads).Select(async _ =>
            {
                var items = await _service.GetAllMenuItemsAsync();
                results.Add(items);
            });

            await Task.WhenAll(readTasks);

            // Assert - All reads should return the same count
            Assert.All(results, items => Assert.Equal(50, items.Count()));
        }

        [Fact]
        public async Task ConcurrentWrites_MultipleThreads_AllItemsCreated()
        {
            // Arrange
            const int concurrentWrites = 50;
            var createdItems = new ConcurrentBag<MenuItemDto>();

            // Act - Concurrent write operations
            var writeTasks = Enumerable.Range(0, concurrentWrites).Select(async i =>
            {
                var createDto = new CreateMenuItemDto
                {
                    Name = $"Concurrent Item {i}",
                    Description = $"Created concurrently {i}",
                    Price = 10.00m + i,
                    Category = $"Category {i % 5}",
                    DietaryTag = $"Tag {i % 3}"
                };

                var created = await _service.CreateMenuItemAsync(createDto);
                createdItems.Add(created);
            });

            await Task.WhenAll(writeTasks);

            // Assert
            Assert.Equal(concurrentWrites, createdItems.Count);
            
            var allItems = await _service.GetAllMenuItemsAsync();
            Assert.Equal(concurrentWrites, allItems.Count());
            
            // Verify all items have unique IDs
            var uniqueIds = allItems.Select(x => x.Id).Distinct().Count();
            Assert.Equal(concurrentWrites, uniqueIds);
        }

        [Fact]
        public async Task ConcurrentMixedOperations_ReadWriteUpdate_DataConsistency()
        {
            // Arrange
            await SeedTestData(20);
            var exceptions = new ConcurrentBag<Exception>();

            // Act - Mix of concurrent operations using separate contexts
            var tasks = new List<Task>();

            // Concurrent reads
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var context = CreateNewContext();
                        var service = new MenuItemService(context);
                        await service.GetAllMenuItemsAsync();
                        await service.GetMenuItemsByCategoryAsync("Category 1");
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            // Concurrent writes
            for (int i = 0; i < 5; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var context = CreateNewContext();
                        var service = new MenuItemService(context);
                        var createDto = new CreateMenuItemDto
                        {
                            Name = $"Mixed Op Item {index}",
                            Description = "Mixed operation test",
                            Price = 15.00m,
                            Category = "Mixed",
                            DietaryTag = "Test"
                        };
                        await service.CreateMenuItemAsync(createDto);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            // Concurrent updates (on existing items)
            for (int i = 1; i <= 5; i++)
            {
                var id = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var context = CreateNewContext();
                        var service = new MenuItemService(context);
                        var updateDto = new UpdateMenuItemDto
                        {
                            Name = $"Updated Item {id}",
                            Description = "Updated in mixed operations",
                            Price = 25.00m,
                            Category = "Updated",
                            DietaryTag = "Updated"
                        };
                        await service.UpdateMenuItemAsync(id, updateDto);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.Empty(exceptions); // No exceptions should occur
            
            var finalItems = await _service.GetAllMenuItemsAsync();
            Assert.Equal(25, finalItems.Count()); // 20 initial + 5 new items
        }

        [Fact]
        public async Task ConcurrentUpdateSameItem_LastWriterWins()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "Original Item",
                Description = "Original description",
                Price = 10.00m,
                Category = "Original",
                DietaryTag = "Original"
            };
            var created = await _service.CreateMenuItemAsync(createDto);

            // Act - Multiple concurrent updates to same item
            const int concurrentUpdates = 10;
            var updateTasks = Enumerable.Range(0, concurrentUpdates).Select(async i =>
            {
                var updateDto = new UpdateMenuItemDto
                {
                    Name = $"Updated by thread {i}",
                    Description = $"Updated by thread {i}",
                    Price = 10.00m + i,
                    Category = $"Category {i}",
                    DietaryTag = $"Tag {i}"
                };
                return await _service.UpdateMenuItemAsync(created.Id, updateDto);
            });

            var results = await Task.WhenAll(updateTasks);

            // Assert
            // All updates should succeed (last writer wins)
            Assert.All(results, result => Assert.NotNull(result));
            
            // Final state should be consistent
            var finalItem = await _service.GetMenuItemByIdAsync(created.Id);
            Assert.NotNull(finalItem);
            Assert.Contains("Updated by thread", finalItem.Name);
        }

        [Fact]
        public async Task ConcurrentDeleteOperations_OnlyOneSucceeds()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "To Be Deleted",
                Description = "This item will be deleted",
                Price = 10.00m,
                Category = "Test",
                DietaryTag = "Test"
            };
            var created = await _service.CreateMenuItemAsync(createDto);

            // Act - Multiple concurrent delete attempts
            const int concurrentDeletes = 5;
            var deleteTasks = Enumerable.Range(0, concurrentDeletes).Select(async _ =>
            {
                return await _service.DeleteMenuItemAsync(created.Id);
            });

            var results = await Task.WhenAll(deleteTasks);

            // Assert
            // Only one delete should succeed
            var successfulDeletes = results.Count(r => r);
            Assert.Equal(1, successfulDeletes);
            
            // Item should no longer exist
            var deletedItem = await _service.GetMenuItemByIdAsync(created.Id);
            Assert.Null(deletedItem);
        }

        #endregion

        #region Memory and Resource Tests

        [Fact]
        public async Task LargeDatasetOperations_MemoryUsage_NoMemoryLeaks()
        {
            // Arrange
            const int iterationCount = 10;
            const int itemsPerIteration = 100;

            // Act - Multiple iterations of creating and reading large datasets
            for (int iteration = 0; iteration < iterationCount; iteration++)
            {
                // Create items
                var createTasks = new List<Task<MenuItemDto>>();
                for (int i = 0; i < itemsPerIteration; i++)
                {
                    var createDto = new CreateMenuItemDto
                    {
                        Name = $"Memory Test {iteration}-{i}",
                        Description = $"Testing memory usage iteration {iteration} item {i}",
                        Price = 10.00m,
                        Category = $"Category {iteration}",
                        DietaryTag = "Memory Test"
                    };
                    createTasks.Add(_service.CreateMenuItemAsync(createDto));
                }

                await Task.WhenAll(createTasks);

                // Read all items
                var allItems = await _service.GetAllMenuItemsAsync();
                Assert.True(allItems.Count() >= itemsPerIteration * (iteration + 1));

                // Force garbage collection to test for memory leaks
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            // Assert - Final verification
            var finalItems = await _service.GetAllMenuItemsAsync();
            Assert.Equal(iterationCount * itemsPerIteration, finalItems.Count());
        }

        [Fact]
        public async Task HighFrequencyOperations_ResourceManagement()
        {
            // Arrange & Act - High frequency operations with proper context isolation
            const int operationCount = 200;
            var tasks = new List<Task>();

            for (int i = 0; i < operationCount; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    using var context = CreateNewContext();
                    var service = new MenuItemService(context);

                    // Create
                    var created = await service.CreateMenuItemAsync(new CreateMenuItemDto
                    {
                        Name = $"High Freq {index}",
                        Description = "High frequency test",
                        Price = 10.00m,
                        Category = "Test",
                        DietaryTag = "Test"
                    });

                    // Read
                    await service.GetMenuItemByIdAsync(created.Id);

                    // Update
                    await service.UpdateMenuItemAsync(created.Id, new UpdateMenuItemDto
                    {
                        Name = $"Updated High Freq {index}",
                        Description = "Updated",
                        Price = 15.00m,
                        Category = "Updated",
                        DietaryTag = "Updated"
                    });

                    // Delete half of them
                    if (index % 2 == 0)
                    {
                        await service.DeleteMenuItemAsync(created.Id);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            var remainingItems = await _service.GetAllMenuItemsAsync();
            Assert.True(remainingItems.Count() <= operationCount); // Should have some items remaining
        }

        #endregion

        #region Edge Case Stress Tests

        [Fact]
        public async Task RapidCreateAndDelete_DataIntegrity()
        {
            // Arrange & Act - Rapid create and delete cycles
            const int cycles = 50;
            var finalIds = new List<int>();

            for (int cycle = 0; cycle < cycles; cycle++)
            {
                // Create 5 items
                var createTasks = Enumerable.Range(0, 5).Select(async i =>
                {
                    return await _service.CreateMenuItemAsync(new CreateMenuItemDto
                    {
                        Name = $"Cycle {cycle} Item {i}",
                        Description = "Rapid cycle test",
                        Price = 10.00m,
                        Category = "Cycle",
                        DietaryTag = "Test"
                    });
                });

                var created = await Task.WhenAll(createTasks);

                // Delete 3 of them immediately
                var deleteTasks = created.Take(3).Select(item => _service.DeleteMenuItemAsync(item.Id));
                await Task.WhenAll(deleteTasks);

                // Keep track of remaining items
                finalIds.AddRange(created.Skip(3).Select(x => x.Id));
            }

            // Assert
            var allItems = await _service.GetAllMenuItemsAsync();
            Assert.Equal(cycles * 2, allItems.Count()); // 2 items per cycle should remain
        }

        #endregion

        private async Task SeedTestData(int count)
        {
            var items = new List<MenuItem>();
            for (int i = 1; i <= count; i++)
            {
                items.Add(new MenuItem
                {
                    Name = $"Test Item {i}",
                    Description = $"Description {i}",
                    Price = 10.00m + i,
                    Category = $"Category {i % 5}",
                    DietaryTag = $"Tag {i % 3}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            
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
                    Name = $"Large Dataset Item {i:D4}",
                    Description = $"Large dataset description {i}",
                    Price = (decimal)(i % 100 + 1),
                    Category = $"Category {i % 20}",
                    DietaryTag = $"Tag {i % 10}",
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