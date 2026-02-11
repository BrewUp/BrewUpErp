namespace BrewApp.Mobile.Components;

public partial class SummaryCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SummaryCard), string.Empty, propertyChanged: OnTitleChanged);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(SummaryCard), string.Empty, propertyChanged: OnValueChanged);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(SummaryCard), string.Empty, propertyChanged: OnSubtitleChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public SummaryCard()
    {
        InitializeComponent();
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SummaryCard)bindable;
        control.TitleLabel.Text = newValue?.ToString() ?? string.Empty;
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SummaryCard)bindable;
        control.ValueLabel.Text = newValue?.ToString() ?? string.Empty;
    }

    private static void OnSubtitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SummaryCard)bindable;
        var subtitle = newValue?.ToString() ?? string.Empty;
        control.SubtitleLabel.Text = subtitle;
        control.SubtitleLabel.IsVisible = !string.IsNullOrWhiteSpace(subtitle);
    }
}
