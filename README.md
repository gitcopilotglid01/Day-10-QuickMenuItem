# QuickBite Menu API

A robust .NET 7 Web API for managing restaurant menu items, built with SQLite database and comprehensive test coverage following Test-Driven Development (TDD) practices.

## 🚀 Features

- **Full CRUD Operations**: Create, Read, Update, and Delete menu items
- **SQLite Database**: Lightweight, serverless database with Entity Framework Core
- **Swagger UI**: Interactive API documentation and testing interface
- **Input Validation**: Comprehensive validation with data annotations
- **Secure Coding**: Parameterized queries through Entity Framework Core ORM
- **Dockerized**: Ready for containerized deployment
- **Test Coverage**: Comprehensive unit tests following TDD methodology
- **RESTful Design**: Following REST principles with proper HTTP status codes

## 📋 Menu Item Structure

Each menu item contains:
- **ID**: Unique identifier (auto-generated)
- **Name**: Menu item name (1-100 characters, required)
- **Description**: Item description (up to 500 characters)
- **Price**: Price in USD (0.01 - 999.99, required)
- **Category**: Item category (1-50 characters, required)
- **DietaryTag**: Dietary information (up to 50 characters, default: "None")
- **CreatedAt**: Timestamp when item was created
- **UpdatedAt**: Timestamp when item was last modified

## 🛠 Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)

## 📦 Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd QuickBiteMenuAPI
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build the Project
```bash
dotnet build
```

### 4. Run the Application
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## 🧪 Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Project
```bash
cd QuickBiteMenuAPI.Tests
dotnet test
```

## 🐳 Docker Deployment

### Build Docker Image
```bash
docker build -t quickbite-menu-api .
```

### Run Container
```bash
docker run -d -p 8080:80 --name quickbite-api quickbite-menu-api
```

The API will be available at `http://localhost:8080`

### Using Docker Compose (Recommended)
```yaml
version: '3.8'
services:
  quickbite-api:
    build: .
    ports:
      - "8080:80"
    volumes:
      - ./data:/app/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

## 🔌 API Endpoints

### Base URL
- Local: `https://localhost:5001/api`
- Docker: `http://localhost:8080/api`

### Menu Items Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/menuitem` | Get all menu items |
| GET | `/menuitem/{id}` | Get menu item by ID |
| GET | `/menuitem/category/{category}` | Get items by category |
| GET | `/menuitem/dietary/{dietaryTag}` | Get items by dietary tag |
| POST | `/menuitem` | Create new menu item |
| PUT | `/menuitem/{id}` | Update existing menu item |
| DELETE | `/menuitem/{id}` | Delete menu item |

### Example Requests

#### Create Menu Item
```bash
curl -X POST "https://localhost:5001/api/menuitem" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Margherita Pizza",
    "description": "Classic pizza with tomato sauce, mozzarella, and fresh basil",
    "price": 12.99,
    "category": "Main Course",
    "dietaryTag": "Vegetarian"
  }'
```

#### Get All Menu Items
```bash
curl -X GET "https://localhost:5001/api/menuitem"
```

#### Update Menu Item
```bash
curl -X PUT "https://localhost:5001/api/menuitem/1" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Pizza Name",
    "description": "Updated description",
    "price": 15.99,
    "category": "Main Course",
    "dietaryTag": "Vegetarian"
  }'
```

## 📊 Database Schema

The SQLite database contains a single `MenuItems` table with the following structure:

```sql
CREATE TABLE MenuItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    Price DECIMAL(6,2) NOT NULL,
    Category TEXT NOT NULL,
    DietaryTag TEXT DEFAULT 'None',
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL
);

-- Indexes for better performance
CREATE INDEX IX_MenuItems_Category ON MenuItems (Category);
CREATE INDEX IX_MenuItems_DietaryTag ON MenuItems (DietaryTag);
```

## 🔒 Security Features

- **Input Validation**: All inputs are validated using data annotations
- **Parameterized Queries**: Entity Framework Core prevents SQL injection
- **Model Binding**: Automatic validation of request models
- **CORS**: Configurable Cross-Origin Resource Sharing
- **HTTPS**: SSL/TLS encryption in production

## 📖 API Documentation

Interactive API documentation is available via Swagger UI when running the application:
- Local: `https://localhost:5001/swagger`
- Docker: `http://localhost:8080/swagger`

The Swagger interface provides:
- Complete API documentation
- Interactive testing capabilities
- Request/response examples
- Schema definitions

## 🧪 Test-Driven Development (TDD)

This project was built following TDD principles:

1. **Red Phase**: Write failing tests first
2. **Green Phase**: Implement minimum code to pass tests
3. **Refactor Phase**: Improve code while keeping tests green

### Test Categories

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test component interactions
- **Controller Tests**: Test API endpoints with in-memory database

### Test Coverage Areas

- ✅ CRUD operations for all endpoints
- ✅ Input validation scenarios
- ✅ Error handling and edge cases
- ✅ Business logic validation
- ✅ Database operations

## 🏗 Project Structure

```
QuickBiteMenuAPI/
├── Controllers/
│   └── MenuItemController.cs      # API endpoints
├── Data/
│   └── MenuDbContext.cs          # Database context
├── Models/
│   └── MenuItem.cs               # Entity and DTOs
├── Services/
│   ├── IMenuItemService.cs       # Service interface
│   └── MenuItemService.cs        # Business logic
├── Program.cs                    # Application entry point
├── appsettings.json             # Configuration
├── Dockerfile                   # Container definition
└── QuickBiteMenuAPI.csproj      # Project file

QuickBiteMenuAPI.Tests/
├── MenuItemControllerTests.cs    # Controller tests
├── MenuItemServiceTests.cs       # Service tests
└── QuickBiteMenuAPI.Tests.csproj # Test project file
```

## 🚀 Performance Optimizations

- **Indexed Queries**: Database indexes on frequently queried columns
- **Async Operations**: All database operations are asynchronous
- **Efficient Mapping**: Direct mapping between entities and DTOs
- **Connection Pooling**: Built-in Entity Framework connection pooling

## 🔧 Configuration

### Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=quickbite_menu.db"
  }
}
```

### Logging Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## � GitHub Copilot Experience

This project extensively leveraged GitHub Copilot to accelerate development and improve code quality. Here are two key experiences that highlight both the power and limitations of AI-assisted development:

### 🚀 **Achievement: Accelerated Test Suite Development**

**What Copilot Helped Achieve Faster:**

GitHub Copilot significantly accelerated the creation of our comprehensive test suite (110+ tests across 5 test files). When implementing edge case and performance testing scenarios, Copilot excelled at:

- **Pattern Recognition**: After writing the first few unit tests, Copilot intelligently suggested similar test patterns for remaining CRUD operations, reducing development time by ~60%
- **Edge Case Generation**: Copilot suggested comprehensive edge cases I might have missed, including Unicode character handling, boundary value testing, and null parameter scenarios
- **Bulk Test Creation**: For performance testing, Copilot generated complex concurrent operation tests and bulk data scenarios that would have taken hours to write manually
- **xUnit Syntax Mastery**: Copilot provided accurate xUnit assertions and test structure, eliminating the need to reference documentation repeatedly

**Impact**: What would have been 2-3 days of manual test writing was completed in approximately 6-8 hours, allowing more time for actual testing and refinement.

### 🔄 **Challenge: Concurrency and DbContext Threading Issues**

**When I Had to Reject/Refactor Copilot's Code:**

During the implementation of performance and concurrency tests, Copilot initially suggested a flawed approach for multi-threading scenarios:

**Copilot's Original Suggestion:**
```csharp
// Copilot suggested sharing a single DbContext across multiple threads
var tasks = Enumerable.Range(0, 10).Select(async i =>
{
    // This causes "A second operation was started on this context instance" errors
    await service.CreateMenuItemAsync(new MenuItem { ... });
}).ToArray();
```

**Why It Was Problematic:**
- DbContext is not thread-safe and cannot handle concurrent operations
- Shared context instances caused database locking and concurrency violations
- Tests were failing intermittently due to race conditions

**My Refactored Solution:**
```csharp
// Created isolated DbContext instances for each concurrent operation
private MenuDbContext CreateNewContext()
{
    var options = new DbContextOptionsBuilder<MenuDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    return new MenuDbContext(options);
}

var tasks = Enumerable.Range(0, 10).Select(async i =>
{
    using var context = CreateNewContext();
    var service = new MenuItemService(context);
    await service.CreateMenuItemAsync(new MenuItem { ... });
}).ToArray();
```

**Key Learning**: While Copilot excels at generating code patterns and suggestions, it sometimes lacks the deeper understanding of framework-specific constraints like Entity Framework's threading limitations. Critical thinking and domain expertise remain essential for robust solutions.

## �🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Write tests for new functionality (TDD approach)
4. Implement the feature
5. Ensure all tests pass (`dotnet test`)
6. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
7. Push to the branch (`git push origin feature/AmazingFeature`)
8. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Troubleshooting

### Common Issues

1. **Database Lock Issues**
   - Ensure only one instance is running
   - Check file permissions for SQLite database

2. **Port Already in Use**
   ```bash
   # Find process using port 5001
   netstat -ano | findstr :5001
   # Kill the process
   taskkill /PID <process_id> /F
   ```

3. **Package Restore Issues**
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

### Getting Help

- Check the [Issues](../../issues) page for known problems
- Review the Swagger documentation at `/swagger`
- Examine the test files for usage examples

## 📈 Future Enhancements

- [ ] Authentication and authorization
- [ ] Rate limiting
- [ ] Caching layer (Redis)
- [ ] File upload for menu item images
- [ ] Advanced search and filtering
- [ ] Audit logging
- [ ] Multi-tenant support
- [ ] GraphQL endpoint

---

**Built with ❤️ using .NET 7, Entity Framework Core, and SQLite**