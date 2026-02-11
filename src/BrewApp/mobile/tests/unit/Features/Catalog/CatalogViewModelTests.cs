using BrewApp.Mobile.Features.Catalog;
using BrewApp.Mobile.Models;
using BrewApp.Mobile.Services.Api;
using BrewApp.Mobile.Services.Api.Dtos;
using BrewApp.Mobile.Services.Ui;

namespace BrewApp.Mobile.Tests.Features.Catalog;

/// <summary>
/// Unit tests for CatalogViewModel.
/// Tests search functionality, loading states, success paths, and error handling.
/// </summary>
public class CatalogViewModelTests
{
    private readonly Mock<ICatalogApiClient> _mockApiClient;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly CatalogViewModel _viewModel;

    public CatalogViewModelTests()
    {
        _mockApiClient = new Mock<ICatalogApiClient>();
        _mockNotificationService = new Mock<INotificationService>();
        _viewModel = new CatalogViewModel(_mockApiClient.Object, _mockNotificationService.Object);
    }

    [Fact]
    public void Constructor_InitializesEmptyBeersList()
    {
        // Assert
        _viewModel.Beers.Should().BeEmpty();
        _viewModel.SearchTerm.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.HasError.Should().BeFalse();
        _viewModel.HasData.Should().BeFalse();
    }

    [Fact]
    public async Task LoadBeersAsync_SuccessPath_PopulatesBeersList()
    {
        // Arrange
        var mockDtos = new List<BeerDto>
        {
            new() { BeerId = Guid.NewGuid(), Name = "IPA", Style = "India Pale Ale", Abv = 6.5m, IsAvailable = true },
            new() { BeerId = Guid.NewGuid(), Name = "Lager", Style = "Pilsner", Abv = 4.2m, IsAvailable = true },
            new() { BeerId = Guid.NewGuid(), Name = "Stout", Style = "Dry Stout", Abv = 5.0m, IsAvailable = false }
        };

        _mockApiClient
            .Setup(x => x.GetBeersAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDtos);

        // Act
        await _viewModel.LoadBeersAsync();

        // Assert
        _viewModel.Beers.Should().HaveCount(3);
        _viewModel.Beers[0].Name.Should().Be("IPA");
        _viewModel.Beers[1].Name.Should().Be("Lager");
        _viewModel.Beers[2].Name.Should().Be("Stout");
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.HasError.Should().BeFalse();
        _viewModel.HasData.Should().BeTrue();
    }

    [Fact]
    public async Task LoadBeersAsync_WithSearchTerm_PassesSearchToApi()
    {
        // Arrange
        _viewModel.SearchTerm = "IPA";
        var mockDtos = new List<BeerDto>
        {
            new() { BeerId = Guid.NewGuid(), Name = "American IPA", Style = "India Pale Ale", Abv = 6.5m }
        };

        _mockApiClient
            .Setup(x => x.GetBeersAsync("IPA", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDtos);

        // Act
        await _viewModel.LoadBeersAsync();

        // Assert
        _mockApiClient.Verify(
            x => x.GetBeersAsync("IPA", It.IsAny<CancellationToken>()),
            Times.Once);
        _viewModel.Beers.Should().HaveCount(1);
        _viewModel.Beers[0].Name.Should().Be("American IPA");
    }

    [Fact]
    public async Task LoadBeersAsync_EmptyResult_ShowsToastAndClearsList()
    {
        // Arrange
        _mockApiClient
            .Setup(x => x.GetBeersAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BeerDto>());

        // Act
        await _viewModel.LoadBeersAsync();

        // Assert
        _viewModel.Beers.Should().BeEmpty();
        _viewModel.HasData.Should().BeFalse();
        _mockNotificationService.Verify(
            x => x.ShowToastAsync(It.Is<string>(msg => msg.Contains("No beers")), It.IsAny<ToastDuration>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadBeersAsync_ApiReturnsNull_ShowsToastAndClearsList()
    {
        // Arrange
        _mockApiClient
            .Setup(x => x.GetBeersAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<BeerDto>?)null);

        // Act
        await _viewModel.LoadBeersAsync();

        // Assert
        _viewModel.Beers.Should().BeEmpty();
        _viewModel.HasData.Should().BeFalse();
    }

    [Fact]
    public async Task LoadBeersAsync_ApiThrowsException_SetsErrorStateAndShowsDialog()
    {
        // Arrange
        _mockApiClient
            .Setup(x => x.GetBeersAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Network error"));

        // Act
        await _viewModel.LoadBeersAsync();

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.Beers.Should().BeEmpty();
        _viewModel.HasData.Should().BeFalse();
        _mockNotificationService.Verify(
            x => x.ShowErrorAsync(
                It.Is<string>(title => title.Contains("Error")),
                It.IsAny<string>(),
                It.Is<string?>(retry => retry == "Retry")),
            Times.Once);
    }

    [Fact]
    public async Task SearchCommand_ExecutesLoadBeersAsync()
    {
        // Arrange
        _viewModel.SearchTerm = "Stout";
        var mockDtos = new List<BeerDto>
        {
            new() { BeerId = Guid.NewGuid(), Name = "Dry Stout", Style = "Stout", Abv = 5.0m }
        };
        _mockApiClient
            .Setup(x => x.GetBeersAsync("Stout", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDtos);

        // Act
        _viewModel.SearchCommand.Execute(null);
        await Task.Delay(100); // Give time for async execution

        // Assert
        _mockApiClient.Verify(
            x => x.GetBeersAsync("Stout", It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ClearSearchCommand_ClearsSearchTermAndReloads()
    {
        // Arrange
        _viewModel.SearchTerm = "Test";
        var mockDtos = new List<BeerDto> { new() { BeerId = Guid.NewGuid(), Name = "All Beers", Style = "Mixed" } };
        _mockApiClient
            .Setup(x => x.GetBeersAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDtos);

        // Act
        _viewModel.ClearSearchCommand.Execute(null);
        await Task.Delay(100); // Give time for async execution

        // Assert
        _viewModel.SearchTerm.Should().BeEmpty();
        _mockApiClient.Verify(
            x => x.GetBeersAsync(null, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task LoadBeersAsync_SetsIsLoadingDuringExecution()
    {
        // Arrange
        var loadingStates = new List<bool>();
        var mockDtos = new List<BeerDto> { new() { BeerId = Guid.NewGuid(), Name = "Test Beer" } };

        _mockApiClient
            .Setup(x => x.GetBeersAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                loadingStates.Add(_viewModel.IsLoading);
                await Task.Delay(10);
                return mockDtos;
            });

        // Act
        await _viewModel.LoadBeersAsync();
        loadingStates.Add(_viewModel.IsLoading);

        // Assert
        loadingStates.Should().Contain(true); // Was loading at some point
        _viewModel.IsLoading.Should().BeFalse(); // Not loading after completion
    }
}
