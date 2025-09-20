using Microsoft.EntityFrameworkCore;
using QuickBiteMenuAPI.Data;
using QuickBiteMenuAPI.Models;

namespace QuickBiteMenuAPI.Services
{
    /// <summary>
    /// Interface for menu item service operations
    /// </summary>
    public interface IMenuItemService
    {
        Task<IEnumerable<MenuItemDto>> GetAllMenuItemsAsync();
        Task<MenuItemDto?> GetMenuItemByIdAsync(int id);
        Task<IEnumerable<MenuItemDto>> GetMenuItemsByCategoryAsync(string category);
        Task<IEnumerable<MenuItemDto>> GetMenuItemsByDietaryTagAsync(string dietaryTag);
        Task<IEnumerable<MenuItemDto>> GetMenuItemsByNameAsync(string name);
        Task<IEnumerable<MenuItemDto>> SearchMenuItemsAsync(string searchTerm, bool exactMatch = false);
        Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto createMenuItemDto);
        Task<MenuItemDto?> UpdateMenuItemAsync(int id, UpdateMenuItemDto updateMenuItemDto);
        Task<bool> DeleteMenuItemAsync(int id);
        Task<bool> MenuItemExistsAsync(int id);
    }

    /// <summary>
    /// Service for managing menu item operations
    /// </summary>
    public class MenuItemService : IMenuItemService
    {
        private readonly MenuDbContext _context;

        public MenuItemService(MenuDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MenuItemDto>> GetAllMenuItemsAsync()
        {
            var menuItems = await _context.MenuItems
                .OrderBy(mi => mi.Category)
                .ThenBy(mi => mi.Name)
                .ToListAsync();

            return menuItems.Select(MapToDto);
        }

        public async Task<MenuItemDto?> GetMenuItemByIdAsync(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            return menuItem == null ? null : MapToDto(menuItem);
        }

        public async Task<IEnumerable<MenuItemDto>> GetMenuItemsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return new List<MenuItemDto>();
            }

            var menuItems = await _context.MenuItems
                .Where(mi => mi.Category.ToLower() == category.ToLower())
                .OrderBy(mi => mi.Name)
                .ToListAsync();

            return menuItems.Select(MapToDto);
        }

        public async Task<IEnumerable<MenuItemDto>> GetMenuItemsByDietaryTagAsync(string dietaryTag)
        {
            if (string.IsNullOrWhiteSpace(dietaryTag))
            {
                return new List<MenuItemDto>();
            }

            var menuItems = await _context.MenuItems
                .Where(mi => mi.DietaryTag.ToLower() == dietaryTag.ToLower())
                .OrderBy(mi => mi.Category)
                .ThenBy(mi => mi.Name)
                .ToListAsync();

            return menuItems.Select(MapToDto);
        }

        /// <summary>
        /// Gets menu items by name using secure parameterized queries with Entity Framework LINQ.
        /// This method demonstrates proper security practices using ORM methods.
        /// </summary>
        /// <param name="name">The name to search for (supports partial matching)</param>
        /// <returns>Menu items matching the name</returns>
        public async Task<IEnumerable<MenuItemDto>> GetMenuItemsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new List<MenuItemDto>();
            }

            // SECURE APPROACH: Using Entity Framework LINQ with parameterized queries
            // This approach prevents SQL injection attacks through automatic parameterization
            var menuItems = await _context.MenuItems
                .Where(mi => mi.Name.Contains(name)) // EF automatically parameterizes this
                .OrderBy(mi => mi.Name)
                .ToListAsync();

            return menuItems.Select(MapToDto);
        }

        /// <summary>
        /// Enhanced search method demonstrating multiple secure approaches for querying data.
        /// Shows different parameterized query techniques and case-insensitive searching.
        /// </summary>
        /// <param name="searchTerm">The term to search for in name and description</param>
        /// <param name="exactMatch">Whether to perform exact match or partial match</param>
        /// <returns>Menu items matching the search criteria</returns>
        public async Task<IEnumerable<MenuItemDto>> SearchMenuItemsAsync(string searchTerm, bool exactMatch = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<MenuItemDto>();
            }

            // Normalize search term for case-insensitive comparison
            var normalizedSearchTerm = searchTerm.Trim().ToLower();

            IQueryable<MenuItem> query = _context.MenuItems;

            if (exactMatch)
            {
                // SECURE: Exact match using parameterized queries
                query = query.Where(mi => mi.Name.ToLower() == normalizedSearchTerm ||
                                         mi.Description.ToLower() == normalizedSearchTerm);
            }
            else
            {
                // SECURE: Partial match using parameterized queries
                // EF Core automatically parameterizes these expressions
                query = query.Where(mi => mi.Name.ToLower().Contains(normalizedSearchTerm) ||
                                         mi.Description.ToLower().Contains(normalizedSearchTerm) ||
                                         mi.Category.ToLower().Contains(normalizedSearchTerm));
            }

            var menuItems = await query
                .OrderBy(mi => mi.Name)
                .ThenBy(mi => mi.Category)
                .ToListAsync();

            return menuItems.Select(MapToDto);
        }

        public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto createMenuItemDto)
        {
            var menuItem = new MenuItem
            {
                Name = createMenuItemDto.Name.Trim(),
                Description = createMenuItemDto.Description.Trim(),
                Price = createMenuItemDto.Price,
                Category = createMenuItemDto.Category.Trim(),
                DietaryTag = createMenuItemDto.DietaryTag.Trim()
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return MapToDto(menuItem);
        }

        public async Task<MenuItemDto?> UpdateMenuItemAsync(int id, UpdateMenuItemDto updateMenuItemDto)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return null;
            }

            menuItem.Name = updateMenuItemDto.Name.Trim();
            menuItem.Description = updateMenuItemDto.Description.Trim();
            menuItem.Price = updateMenuItemDto.Price;
            menuItem.Category = updateMenuItemDto.Category.Trim();
            menuItem.DietaryTag = updateMenuItemDto.DietaryTag.Trim();

            _context.Entry(menuItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return MapToDto(menuItem);
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return false;
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MenuItemExistsAsync(int id)
        {
            return await _context.MenuItems.AnyAsync(mi => mi.Id == id);
        }

        private static MenuItemDto MapToDto(MenuItem menuItem)
        {
            return new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                Category = menuItem.Category,
                DietaryTag = menuItem.DietaryTag,
                CreatedAt = menuItem.CreatedAt,
                UpdatedAt = menuItem.UpdatedAt
            };
        }
    }
}