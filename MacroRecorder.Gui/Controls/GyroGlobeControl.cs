using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MacroRecorder.Gui.ViewModels;

namespace MacroRecorder.Gui.Controls;

public sealed class GyroGlobeControl : Control
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(GyroGlobeControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnItemsSourceChanged));

    public static readonly DependencyProperty SelectedFrameProperty = DependencyProperty.Register(
        nameof(SelectedFrame),
        typeof(MacroStepViewModel),
        typeof(GyroGlobeControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    private INotifyCollectionChanged? _collection;
    private readonly List<MacroStepViewModel> _observedItems = new();

    static GyroGlobeControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GyroGlobeControl), new FrameworkPropertyMetadata(typeof(GyroGlobeControl)));
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public MacroStepViewModel? SelectedFrame
    {
        get => (MacroStepViewModel?)GetValue(SelectedFrameProperty);
        set => SetValue(SelectedFrameProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
        if (bounds.Width <= 1 || bounds.Height <= 1)
        {
            return;
        }

        var center = new Point(bounds.Width / 2, bounds.Height / 2);
        var radius = Math.Max(8, Math.Min(bounds.Width, bounds.Height) / 2 - 14);
        var sphere = new Rect(center.X - radius, center.Y - radius, radius * 2, radius * 2);

        drawingContext.DrawEllipse(new RadialGradientBrush(Color.FromRgb(54, 110, 180), Color.FromRgb(13, 28, 48))
        {
            GradientOrigin = new Point(0.35, 0.28),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.75,
            RadiusY = 0.75
        }, new Pen(Brushes.SteelBlue, 2), center, radius, radius);

        var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(85, 210, 230, 255)), 1);
        gridPen.Freeze();
        for (var i = -2; i <= 2; i++)
        {
            var offset = i * radius / 3;
            var latHeight = 2 * Math.Sqrt(Math.Max(0, radius * radius - offset * offset));
            drawingContext.DrawEllipse(null, gridPen, new Point(center.X, center.Y + offset), latHeight / 2, radius / 9);
            drawingContext.DrawLine(gridPen, new Point(center.X + offset, center.Y - radius), new Point(center.X + offset, center.Y + radius));
        }

        var trajectory = BuildTrajectory(center, radius).ToList();
        if (trajectory.Count > 1)
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(trajectory[0].Point, false, false);
                context.PolyLineTo(trajectory.Skip(1).Select(p => p.Point).ToList(), true, false);
            }
            geometry.Freeze();
            drawingContext.DrawGeometry(null, new Pen(Brushes.Orange, 3), geometry);
        }

        foreach (var point in trajectory.Skip(1).Select(p => p.Point))
        {
            drawingContext.DrawEllipse(Brushes.Gold, null, point, 2.8, 2.8);
        }

        if (SelectedFrame is not null)
        {
            var selectedPoint = trajectory.LastOrDefault(p => ReferenceEquals(p.Step, SelectedFrame))?.Point;
            if (selectedPoint is Point point)
            {
                drawingContext.DrawEllipse(Brushes.White, new Pen(Brushes.Red, 2), point, 7, 7);
                DrawVector(drawingContext, center, point);
            }
        }

        drawingContext.DrawText(
            new FormattedText("Gyro tilt globe", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip),
            new Point(8, 8));
    }

    private IEnumerable<TrajectoryPoint> BuildTrajectory(Point center, double radius)
    {
        var rawPoints = BuildRawTrajectory().ToList();
        if (rawPoints.Count == 0)
        {
            yield break;
        }

        var maxDistance = rawPoints.Max(p => Math.Max(Math.Abs(p.X), Math.Abs(p.Y)));
        var scale = maxDistance > 0 ? radius * 0.86 / maxDistance : 1;
        foreach (var rawPoint in rawPoints)
        {
            yield return new TrajectoryPoint(new Point(center.X + rawPoint.X * scale, center.Y - rawPoint.Y * scale), rawPoint.Step);
        }
    }

    private IEnumerable<RawTrajectoryPoint> BuildRawTrajectory()
    {
        yield return new RawTrajectoryPoint(0, 0, null);
        if (ItemsSource is null) yield break;

        const double scale = 16.384;
        double x = 0;
        double y = 0;
        foreach (var step in ItemsSource.OfType<MacroStepViewModel>())
        {
            var frameCount = Math.Max(1, step.Frames);
            for (var frame = 0; frame < frameCount; frame++)
            {
                var rollDegrees = step.Roll / scale;
                var pitchDegrees = step.Pitch / scale;
                var yawDegrees = step.Yaw / scale;
                x += yawDegrees + rollDegrees * 0.35;
                y += pitchDegrees;
                yield return new RawTrajectoryPoint(x, y, step);
            }
        }
    }

    private sealed record TrajectoryPoint(Point Point, MacroStepViewModel? Step);

    private sealed record RawTrajectoryPoint(double X, double Y, MacroStepViewModel? Step);

    private static void DrawVector(DrawingContext drawingContext, Point center, Point selectedPoint)
    {
        var pen = new Pen(Brushes.Red, 2);
        drawingContext.DrawLine(pen, center, selectedPoint);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (GyroGlobeControl)d;
        control.DetachItems();
        if (e.NewValue is INotifyCollectionChanged collection)
        {
            control._collection = collection;
            collection.CollectionChanged += control.OnCollectionChanged;
        }
        control.AttachItems();
        control.InvalidateVisual();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DetachItemPropertyChanged();
        AttachItems();
        InvalidateVisual();
    }

    private void AttachItems()
    {
        if (ItemsSource is null) return;
        foreach (var item in ItemsSource.OfType<MacroStepViewModel>())
        {
            _observedItems.Add(item);
            item.PropertyChanged += OnItemPropertyChanged;
        }
    }

    private void DetachItems()
    {
        if (_collection is not null)
        {
            _collection.CollectionChanged -= OnCollectionChanged;
            _collection = null;
        }
        DetachItemPropertyChanged();
    }

    private void DetachItemPropertyChanged()
    {
        foreach (var item in _observedItems)
        {
            item.PropertyChanged -= OnItemPropertyChanged;
        }
        _observedItems.Clear();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) => InvalidateVisual();
}
