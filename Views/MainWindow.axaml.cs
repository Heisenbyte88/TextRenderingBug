using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace TextRenderingBug.Views;

public partial class MainWindow : Window
{
    private bool _isDndActive;
    private Popup? _popup;
    private PointerPoint? _lastClickPosition;

    public MainWindow()
    {
        InitializeComponent();

        DragBorder.PointerPressed += DragBorder_PointerPressed;
        DragBorder.PointerMoved += DragBorder_PointerMoved;
        DragBorder.PointerCaptureLost += DragBorder_PointerCaptureLost;
        DragBorder.PointerReleased += DragBorder_PointerReleased;
    }

    private void DragBorder_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(DragBorder).Properties.IsLeftButtonPressed)
            return;

        _lastClickPosition = e.GetCurrentPoint(Root);
        e.Pointer.Capture(DragBorder);
    }

    private void DragBorder_PointerMoved(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(Root);

        if (!_isDndActive && _lastClickPosition is { } start &&
            (Math.Abs(position.X - start.Position.X) > 3 || Math.Abs(position.Y - start.Position.Y) > 3))
        {
            BeginDnd(e);
        }

        if (_isDndActive)
            MovePopup(position);
    }

    private void DragBorder_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _lastClickPosition = null;
        e.Pointer.Capture(null);
        EndDnd();
    }

    private void DragBorder_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _lastClickPosition = null;
        EndDnd();
    }

    private void ShowPopup(Point point)
    {
        _popup = new Popup
        {
            Placement = PlacementMode.LeftEdgeAlignedTop,
            PlacementTarget = Root,
            Child = CreateDragImage()
        };

        Root.Children.Add(_popup);
        _popup.Open();
        MovePopup(point);
    }

    private void MovePopup(Point point)
    {
        if (_popup?.Child is not { } child)
            return;

        _popup.HorizontalOffset = point.X + child.DesiredSize.Width / 2;
        _popup.VerticalOffset = point.Y - child.DesiredSize.Height / 2;
    }

    private void BeginDnd(PointerEventArgs e)
    {
        _isDndActive = true;
        e.Pointer.Capture(DragBorder);
        DragBorder.Opacity = 0.1;
        ShowPopup(e.GetPosition(Root));
    }

    private void EndDnd()
    {
        if (!_isDndActive)
            return;

        _isDndActive = false;

        if (_popup is not null)
        {
            _popup.Close();
            Root.Children.Remove(_popup);
            _popup = null;
        }

        DragBorder.Opacity = 1;
    }

    private Border CreateDragImage()
    {
        var icon = new Image
        {
            Width = 16,
            Height = 16,
            Source = new Bitmap(AssetLoader.Open(new Uri("avares://TextRenderingBug/Assets/avalonia-logo.ico")))
        };

        var text = new TextBlock
        {
            Text = "Drop me"
        };

        Grid.SetColumn(text, 2);


        var textRenderingMode =
            TextRenderingModeComboBox.SelectedItem is ComboBoxItem selectedItem &&
            selectedItem.Content is TextRenderingMode mode 
            ? mode
            : TextRenderingMode.Unspecified;

        TextOptions.SetTextRenderingMode(text, textRenderingMode);

        var grid = new Grid
        {
            Margin = new Thickness(4),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            ColumnDefinitions = new ColumnDefinitions("16,8,*"),
            Children = { icon, text }
        };

        var dragImage = new Border
        {
            Background = Brushes.Transparent,
            Child = grid
        };

        return dragImage;
    }
}