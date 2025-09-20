using Microsoft.AspNetCore.Mvc;
using QuickBiteMenuAPI.Models;
using QuickBiteMenuAPI.Services;

namespace QuickBiteMenuAPI.Controllers
{
    /// <summary>
    /// Controller for managing menu items
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MenuItemController : ControllerBase
    {
        private readonly IMenuItemService _menuItemService;

        public MenuItemController(IMenuItemService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        /// <summary>
        /// Get all menu items
        /// </summary>
        /// <returns>List of menu items</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItems()
        {
            var menuItems = await _menuItemService.GetAllMenuItemsAsync();
            return Ok(menuItems);
        }

        /// <summary>
        /// Get menu item by ID
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>Menu item if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MenuItemDto>> GetMenuItem(int id)
        {
            var menuItem = await _menuItemService.GetMenuItemByIdAsync(id);
            
            if (menuItem == null)
            {
                return NotFound();
            }

            return Ok(menuItem);
        }

        /// <summary>
        /// Get menu items by category
        /// </summary>
        /// <param name="category">Category to filter by</param>
        /// <returns>List of menu items in the specified category</returns>
        [HttpGet("category/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItemsByCategory(string category)
        {
            var menuItems = await _menuItemService.GetMenuItemsByCategoryAsync(category);
            return Ok(menuItems);
        }

        /// <summary>
        /// Get menu items by dietary tag
        /// </summary>
        /// <param name="dietaryTag">Dietary tag to filter by</param>
        /// <returns>List of menu items with the specified dietary tag</returns>
        [HttpGet("dietary/{dietaryTag}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItemsByDietaryTag(string dietaryTag)
        {
            var menuItems = await _menuItemService.GetMenuItemsByDietaryTagAsync(dietaryTag);
            return Ok(menuItems);
        }

        /// <summary>
        /// Get menu items by name using SQL string concatenation
        /// WARNING: This endpoint demonstrates SQL injection vulnerability and should NOT be used in production!
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>List of menu items matching the name</returns>
        [HttpGet("name/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItemsByName(string name)
        {
            var menuItems = await _menuItemService.GetMenuItemsByNameAsync(name);
            return Ok(menuItems);
        }

        /// <summary>
        /// Enhanced search for menu items across multiple fields
        /// </summary>
        /// <param name="searchTerm">Search term to look for</param>
        /// <param name="exactMatch">Whether to perform exact match (default: false for partial match)</param>
        /// <returns>List of menu items matching the search criteria</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> SearchMenuItems(
            [FromQuery] string searchTerm, 
            [FromQuery] bool exactMatch = false)
        {
            var menuItems = await _menuItemService.SearchMenuItemsAsync(searchTerm, exactMatch);
            return Ok(menuItems);
        }

        /// <summary>
        /// Create a new menu item
        /// </summary>
        /// <param name="createMenuItemDto">Menu item data</param>
        /// <returns>Created menu item</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MenuItemDto>> CreateMenuItem(CreateMenuItemDto createMenuItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var menuItem = await _menuItemService.CreateMenuItemAsync(createMenuItemDto);
            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }

        /// <summary>
        /// Update an existing menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <param name="updateMenuItemDto">Updated menu item data</param>
        /// <returns>Updated menu item</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MenuItemDto>> UpdateMenuItem(int id, UpdateMenuItemDto updateMenuItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var menuItem = await _menuItemService.UpdateMenuItemAsync(id, updateMenuItemDto);
            
            if (menuItem == null)
            {
                return NotFound();
            }

            return Ok(menuItem);
        }

        /// <summary>
        /// Delete a menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var result = await _menuItemService.DeleteMenuItemAsync(id);
            
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}