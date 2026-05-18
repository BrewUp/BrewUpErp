using BrewSpa.Chat.Application.Models;
using BrewSpa.Chat.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BrewSpa.Chat.Facade;

public partial class Chat : ComponentBase
{
    [Inject] private IChatService ChatService { get; set; } = null!;

    private List<BeerCatalogItem> _beerCatalog = [];
    private List<ChatMessage> _chatHistory = [];
    private string _userMessage = string.Empty;
    private string? _conversationId;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private ElementReference _messagesEndRef;

    protected override async Task OnInitializedAsync()
    {
        await LoadBeerCatalogAsync();
        _isLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender && _chatHistory.Any())
        {
            // Scroll to bottom after new message
            await Task.Delay(100);
        }
    }

    private async Task LoadBeerCatalogAsync()
    {
        var result = await ChatService.GetBeersCatalogAsync();
        result.Match(
            success =>
            {
                _beerCatalog = success;
                return true;
            },
            error =>
            {
                Console.WriteLine($"[Chat] Error loading beer catalog: {error.Message}");
                return false;
            });
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(_userMessage))
            return;

        var userMessageText = _userMessage;
        _userMessage = string.Empty;

        // Add user message to history
        _chatHistory.Add(new ChatMessage
        {
            Content = userMessageText,
            IsUser = true
        });

        _isProcessing = true;
        StateHasChanged();

        try
        {
            var request = new ChatRequest
            {
                Message = userMessageText,
                ConversationId = _conversationId
            };

            var result = await ChatService.AskBrewUpChatAsync(request);
            result.Match(
                success =>
                {
                    _conversationId = success.ConversationId;
                    _chatHistory.Add(new ChatMessage
                    {
                        Content = success.Answer,
                        IsUser = false
                    });
                    return true;
                },
                error =>
                {
                    _chatHistory.Add(new ChatMessage
                    {
                        Content = $"Sorry, I encountered an error: {error.Message}",
                        IsUser = false
                    });
                    Console.WriteLine($"[Chat] Error sending message: {error.Message}");
                    return false;
                });
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessageAsync();
        }
    }

    private class ChatMessage
    {
        public string Content { get; set; } = string.Empty;
        public bool IsUser { get; set; }
    }
}
