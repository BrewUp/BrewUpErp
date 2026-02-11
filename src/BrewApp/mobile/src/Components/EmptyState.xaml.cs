namespace BrewApp.Mobile.Components;

public partial class EmptyState : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(EmptyState), "ℹ️", propertyChanged: OnIconChanged);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyState), string.Empty, propertyChanged: OnTitleChanged);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(EmptyState), string.Empty, propertyChanged: OnMessageChanged);

    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(nameof(ActionText), typeof(string), typeof(EmptyState), string.Empty, propertyChanged: OnActionTextChanged);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public event EventHandler? ActionClicked;

    public EmptyState()
    {
        InitializeComponent();
        ActionButton.Clicked += (s, e) => ActionClicked?.Invoke(this, EventArgs.Empty);
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (EmptyState)bindable;
        control.IconLabel.Text = newValue?.ToString() ?? "ℹ️";
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (EmptyState)bindable;
        control.TitleLabel.Text = newValue?.ToString() ?? string.Empty;
    }

    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (EmptyState)bindable;
        control.MessageLabel.Text = newValue?.ToString() ?? string.Empty;
    }

    private static void OnActionTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (EmptyState)bindable;
        var text = newValue?.ToString() ?? string.Empty;
        control.ActionButton.Text = text;
        control.ActionButton.IsVisible = !string.IsNullOrWhiteSpace(text);
    }
}
