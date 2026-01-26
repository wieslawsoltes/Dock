using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit.TextMate;
using DockReactiveUIRiderSample.ViewModels.Documents;
using TextMateSharp.Grammars;

namespace DockReactiveUIRiderSample.Views.Documents;

public partial class EditorDocumentView : UserControl
{
    private readonly RegistryOptions _registryOptions;
    private TextMate.Installation? _textMateInstallation;

    public EditorDocumentView()
    {
        InitializeComponent();

        _registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        if (Editor is not null)
        {
            _textMateInstallation = Editor.InstallTextMate(_registryOptions);
            _textMateInstallation.AppliedTheme += OnTextMateAppliedTheme;
            Editor.TextArea.Caret.PositionChanged += OnCaretPositionChanged;
        }

        DataContextChanged += (_, _) => UpdateGrammar();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnCaretPositionChanged(object? sender, EventArgs e)
    {
        if (DataContext is EditorDocumentViewModel viewModel && Editor is not null)
        {
            viewModel.CaretLine = Editor.TextArea.Caret.Line;
            viewModel.CaretColumn = Editor.TextArea.Caret.Column;
        }
    }

    private void UpdateGrammar()
    {
        if (_textMateInstallation is null || DataContext is not EditorDocumentViewModel viewModel)
        {
            return;
        }

        var extension = viewModel.Extension;
        var scope = string.IsNullOrWhiteSpace(extension)
            ? _registryOptions.GetScopeByLanguageId("plaintext")
            : _registryOptions.GetScopeByExtension(extension);

        if (string.IsNullOrWhiteSpace(scope))
        {
            scope = _registryOptions.GetScopeByLanguageId("plaintext");
        }

        if (!string.IsNullOrWhiteSpace(scope))
        {
            _textMateInstallation.SetGrammar(scope);
        }
    }

    private void OnTextMateAppliedTheme(object? sender, TextMate.Installation installation)
    {
        if (Editor is null)
        {
            return;
        }

        if (installation.TryGetThemeColor("editor.background", out var background) &&
            Color.TryParse(background, out var backgroundColor))
        {
            Editor.Background = new SolidColorBrush(backgroundColor);
        }

        if (installation.TryGetThemeColor("editor.foreground", out var foreground) &&
            Color.TryParse(foreground, out var foregroundColor))
        {
            Editor.Foreground = new SolidColorBrush(foregroundColor);
        }
    }
}
