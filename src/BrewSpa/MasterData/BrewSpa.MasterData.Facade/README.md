# Customer Management Component

This component provides a comprehensive interface for managing customer data in the BrewSpa MasterData module.

## Features

- ✅ Grid-based customer display with pagination
- ✅ Inline editing capabilities
- ✅ Add, Edit, Delete, Save operations
- ✅ Form validation
- ✅ Responsive design
- ✅ Service-based architecture for API integration
- ✅ Mock data for development/testing

## Files Created

### Models
- `Models/Customer.cs` - Customer, Address, and CustomerResponse models

### Components
- `Components/Customers.razor` - Main customer management component
- `Components/Customers.razor.cs` - Component logic
- `Components/Customers.razor.css` - Component styling

### Services
- `Services/CustomerService.cs` - Service for API integration with mock data fallback

### Extensions
- `Extensions/ServiceCollectionExtensions.cs` - Dependency injection setup

## Usage

### 1. Register Services (Required)

In your main application's `Program.cs`, add the MasterData services:

```csharp
using BrewSpa.MasterData.Facade.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ... existing configuration ...

// Add MasterData services
builder.Services.AddMasterDataServices();

// ... rest of the configuration ...
```

### 2. Navigation

The component is accessible at `/masterdata/customers` and can be reached by clicking the "Customers" tile in the MasterData main page.

### 3. API Integration

To integrate with your actual API, update the `CustomerService.cs` file:

```csharp
public async Task<CustomerResponse> GetCustomersAsync(int page = 0, int pageSize = 10)
{
    // Replace the try-catch with your actual API endpoint
    var response = await _httpClient.GetFromJsonAsync<CustomerResponse>(
        $"your-api-base-url/customers?page={page}&pageSize={pageSize}"
    );
    return response ?? new CustomerResponse();
}
```

Update the base address in your `Program.cs`:

```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://your-api-base-url/") 
});
```

## Expected API Endpoints

The service expects the following REST API endpoints:

- `GET /api/customers?page={page}&pageSize={pageSize}` - Get paginated customers
- `GET /api/customers/{customerId}` - Get customer by ID
- `POST /api/customers` - Create new customer
- `PUT /api/customers/{customerId}` - Update existing customer
- `DELETE /api/customers/{customerId}` - Delete customer

## API Response Format

The API should return customers in this format:

```json
{
  "results": [
    {
      "customerId": "019c295e-2b9b-7ae9-8388-d48420e04e54",
      "ragioneSociale": "Il Grottino del Muflone",
      "partitaIva": "IT01234567890",
      "consumerLevel": "teetotaler",
      "indirizzo": {
        "via": "Del Bosco",
        "numeroCivico": "15",
        "citta": "Brescia",
        "provincia": "BS",
        "cap": "20100",
        "nazione": "ITA"
      }
    }
  ],
  "pageSize": 10,
  "page": 0,
  "totalRecords": 1
}
```

## Customization

### Consumer Levels
The component supports these consumer levels:
- `teetotaler` (Secondary badge)
- `moderate` (Info badge)
- `regular` (Primary badge)  
- `enthusiast` (Success badge)

### Styling
Modify `Components/Customers.razor.css` to match your application's design system.

### Validation
Add custom validation rules in the `SaveEdit()` method in `Customers.razor.cs`.

## Testing

The component includes mock data for testing without an API. The mock data will be used when API calls fail, allowing you to test the component functionality immediately.

## Next Steps

1. Register the services in your main application
2. Configure the API base URL
3. Implement the actual API endpoints
4. Customize styling to match your design system
5. Add additional validation as needed

## Toolbar Functions

- **Add** - Creates a new customer record in edit mode
- **Save** - Saves all pending changes (currently shows success message)
- **Delete** - Deletes the selected customer (with confirmation)
- **Refresh** - Reloads customer data from the API
- **Close** - Navigates back to the MasterData main page

## Grid Features

- **Selection** - Radio buttons for selecting customers
- **Inline Editing** - Click Edit button to modify customer data
- **Pagination** - Navigate through large datasets
- **Page Size** - Configurable number of records per page (10, 25, 50, 100)
- **Responsive** - Adapts to different screen sizes
