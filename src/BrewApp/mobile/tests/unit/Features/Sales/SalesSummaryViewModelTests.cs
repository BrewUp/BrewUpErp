using BrewApp.Mobile.Features.Sales;
using BrewApp.Mobile.Models;
using BrewApp.Mobile.Services.Api;
using BrewApp.Mobile.Services.Api.Dtos;
using BrewApp.Mobile.Services.Ui;

namespace BrewApp.Mobile.Tests.Features.Sales;

/// <summary>
/// Unit tests for SalesSummaryViewModel.
/// Tests period selection, loading states, success paths, and error handling.
/// </summary>
public class SalesSummaryViewModelTests
{
    private readonly Mock<ISalesApiClient> _mockApiClient;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly SalesSummaryViewModel _viewModel;

    public SalesSummaryViewModelTests()
    {
        _mockApiClient = new Mock<ISalesApiClient>();
        _mockNotificationService = new Mock<INotificationService>();
        _viewModel = new SalesSummaryViewModel(_mockApiClient.Object, _mockNotificationService.Object);
    }

    [Fact]
    public void Constructor_InitializesWithDefaultPeriod()
    {
        // Assert
        _viewModel.SelectedPeriod.Should().Be("today");
        _viewModel.Periods.Should().Contain(new[] { "today", "week", "month", "year" });
        _viewModel.SalesSummary.Should().BeNull();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadSummaryAsync_SuccessPath_PopulatesSummaryData()
    {
        // Arrange
        var mockDto = new SalesSummaryDto
        {
            Period = "today",
            TotalRevenue = 1250.50m,
            OrdersCount = 15,
            AverageOrderValue = 83.37m,
            TopSellingBeers = new List<string> { "IPA", "Lager", "Stout" }
        };

        _mockApiClient
            .Setup(x => x.GetSalesSummaryAsync("today", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDto);

        // Act
        await _viewModel.LoadSummaryAsync();

        // Assert
        _viewModel.SalesSummary.Should().NotBeNull();
        _viewModel.SalesSummary!.Period.Should().Be("today");
        _viewModel.SalesSummary.TotalRevenue.Should().Be(1250.50m);
        _viewModel.SalesSummary.OrdersCount.Should().Be(15);
        _viewModel.SalesSummary.AverageOrderValue.Should().Be(83.37m);
        _viewModel.SalesSummary.TopSellingBeers.Should().HaveCount(3);
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.HasError.Should().BeFalse();
        _viewModel.HasData.Should().BeTrue();
    }

    [Fact]
    public async Task LoadSummaryAsync_SetsIsLoadingDuringExecution()
    {
        // Arrange
        var loadingStates = new List<bool>();
        var mockDto = new SalesSummaryDto { Period = "today", TotalRevenue = 100m };

        _mockApiClient
            .Setup(x => x.GetSalesSummaryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                loadingStates.Add(_viewModel.IsLoading);
                await Task.Delay(10);
                return mockDto;
            });

        // Act
        await _viewModel.LoadSummaryAsync();
        loadingStates.Add(_viewModel.IsLoading);

        // Assert
        loadingStates.Should().Contain(true); // Was loading at some point
        _viewModel.IsLoading.Should().BeFalse(); // Not loading after completion
    }

    [Fact]
    public async Task LoadSummaryAsync_ApiReturnsNull_ShowsToastAndClearsSummary()
    {
        // Arrange
        _mockApiClient
            .Setup(x => x.GetSalesSummaryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesSummaryDto?)null);

        // Act
        await _viewModel.LoadSummaryAsync();

        // Assert
        _viewModel.SalesSummary.Should().BeNull();
        _viewModel.HasData.Should().BeFalse();
        _mockNotificationService.Verify(
            x => x.ShowToastAsync(It.Is<string>(msg => msg.Contains("No sales data")), It.IsAny<ToastDuration>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadSummaryAsync_ApiThrowsException_SetsErrorStateAndShowsDialog()
    {
        // Arrange
        _mockApiClient
            .Setup(x => x.GetSalesSummaryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Network error"));

        // Act
        await _viewModel.LoadSummaryAsync();

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.SalesSummary.Should().BeNull();
        _viewModel.HasData.Should().BeFalse();
        _mockNotificationService.Verify(
            x => x.ShowErrorAsync(
                It.Is<string>(title => title.Contains("Error")),
                It.IsAny<string>(),
                It.Is<string?>(retry => retry == "Retry")),
            Times.Once);
    }

    [Theory]
    [InlineData("today")]
    [InlineData("week")]
    [InlineData("month")]
    [InlineData("year")]
    public async Task LoadSummaryAsync_PassesCorrectPeriodToApi(string period)
    {
        // Arrange
        _viewModel.SelectedPeriod = period;
        var mockDto = new SalesSummaryDto { Period = period };
        
        _mockApiClient
            .Setup(x => x.GetSalesSummaryAsync(period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDto);

        // Act
        await _viewModel.LoadSummaryAsync();

        // Assert
        _mockApiClient.Verify(
            x => x.GetSalesSummaryAsync(period, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SelectedPeriod_WhenChanged_TriggersAutoLoad()
    {
        // Arrange
        var mockDto = new SalesSummaryDto { Period = "week", TotalRevenue = 5000m };
        _mockApiClient
            .Setup(x => x.GetSalesSummaryAsync("week", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDto);

        // Act
        _viewModel.SelectedPeriod = "week";
        await Task.Delay(100); // Give time for async load

        // Assert
        _mockApiClient.Verify(
            x => x.GetSalesSummaryAsync("week", It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task LoadSummaryCommand_ExecutesLoadSummaryAsync()
    {
        // Arrange
        var mockDto = new SalesSummaryDto { Period = "today" };
        _mockApiClient
            .Setup(x => x.GetSalesSummaryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDto);

        // Act
        _viewModel.LoadSummaryCommand.Execute(null);
        await Task.Delay(100); // Give time for async execution

        // Assert
        _mockApiClient.Verify(
            x => x.GetSalesSummaryAsync("today", It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}
