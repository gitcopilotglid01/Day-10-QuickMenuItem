using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickBiteMenuAPI.Models
{
    /// <summary>
    /// Represents a menu item in the QuickBite restaurant system
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Unique identifier for the menu item
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Name of the menu item
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the menu item
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Price of the menu item in dollars
        /// </summary>
        [Required]
        [Range(0.01, 999.99, ErrorMessage = "Price must be between $0.01 and $999.99")]
        [Column(TypeName = "decimal(6,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Category of the menu item (e.g., "Appetizer", "Main Course", "Dessert", "Beverage")
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Dietary tag for the menu item (e.g., "Vegetarian", "Vegan", "Gluten-Free", "None")
        /// </summary>
        [StringLength(50)]
        public string DietaryTag { get; set; } = "None";

        /// <summary>
        /// Timestamp when the menu item was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the menu item was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Data Transfer Object for creating a new menu item
    /// </summary>
    public class CreateMenuItemDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999.99, ErrorMessage = "Price must be between $0.01 and $999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Category must be between 1 and 50 characters")]
        public string Category { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Dietary tag cannot exceed 50 characters")]
        public string DietaryTag { get; set; } = "None";
    }

    /// <summary>
    /// Data Transfer Object for updating an existing menu item
    /// </summary>
    public class UpdateMenuItemDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999.99, ErrorMessage = "Price must be between $0.01 and $999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Category must be between 1 and 50 characters")]
        public string Category { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Dietary tag cannot exceed 50 characters")]
        public string DietaryTag { get; set; } = "None";
    }

    /// <summary>
    /// Data Transfer Object for returning menu item data
    /// </summary>
    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string DietaryTag { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}