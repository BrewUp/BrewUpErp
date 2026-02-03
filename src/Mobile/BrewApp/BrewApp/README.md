# BrewApp - Sales Module

This is a .NET MAUI application for managing sales orders for the BrewUp ERP system.

## Project Structure

### Models (BrewApp/Models/Sales)
- `SalesOrder.cs` - Main sales order model
- `CreateSalesOrderRequest.cs` - Request model for creating sales orders
- `SalesOrderListResponse.cs` - Response model for sales order list
- `SalesOrderRow.cs` - Model for sales order line items
- `Quantity.cs` - Model for quantity with unit of measure
- `Price.cs` - Model for price with currency

### Services (BrewApp/Services/Sales)
- `ISalesOrderService.cs` - Interface for sales order API service
- `SalesOrderService.cs` - Implementation of sales order API service

### ViewModels/PageModels (BrewApp/PageModels/Sales)
- `SalesOrderListPageModel.cs` - ViewModel for sales order list page
- `SalesOrderDetailPageModel.cs` - ViewModel for sales order detail page
- `CreateSalesOrderPageModel.cs` - ViewModel for creating sales orders

### Pages (BrewApp/Pages/Sales)
- `SalesOrderListPage.xaml/.cs` - Page to display list of sales orders
- `SalesOrderDetailPage.xaml/.cs` - Page to display sales order details
- `CreateSalesOrderPage.xaml/.cs` - Page to create new sales orders

### Configuration
- `AppSettings.cs` - Contains API base URL configuration

## Features

1. **View Sales Orders List**
   - Display all sales orders with pagination support
   - Pull-to-refresh functionality
   - Tap on an order to view details

2. **View Sales Order Details**
   - Display complete order information
   - Show customer details
   - List all order items with quantities and prices

3. **Create Sales Orders**
   - Enter order information
   - Add customer details
   - Add multiple order items
   - Validation before submission

## API Integration

The app connects to your BrewUp ERP API with the following endpoints:

- `GET /api/salesorders` - Get sales orders list (with pagination)
- `GET /api/salesorders/{id}` - Get sales order by ID
- `POST /api/salesorders` - Create a new sales order

### Configuration

Update the API base URL in `BrewApp/Configuration/AppSettings.cs`:

```csharp
public const string ApiBaseUrl = "https://your-api-url.com";
```

## Next Steps

1. **Update API URL**: Configure your actual API base URL in `AppSettings.cs`
2. **Add Authentication**: Implement authentication if your API requires it
3. **Error Handling**: Enhance error handling and user feedback
4. **Add Additional Modules**: Create similar structures for other ERP modules (Inventory, Production, etc.)
5. **Offline Support**: Consider adding local database for offline functionality
6. **Search and Filters**: Add search and filtering capabilities to the sales order list

## Architecture Pattern

This app follows the MVVM (Model-View-ViewModel) pattern:
- **Models**: Data structures matching your API
- **Services**: API communication layer
- **ViewModels (PageModels)**: Business logic and state management using CommunityToolkit.Mvvm
- **Views (Pages)**: XAML-based UI

## Dependencies

- .NET MAUI (.NET 10)
- CommunityToolkit.Maui
- CommunityToolkit.Mvvm
- Syncfusion.Maui.Toolkit
- System.Net.Http.Json
