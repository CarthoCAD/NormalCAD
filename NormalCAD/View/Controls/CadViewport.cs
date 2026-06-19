using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;
using NormalCAD.View.Drawing;

namespace NormalCAD.View.Controls;

public class CadViewport : Control
{
    public static readonly StyledProperty<bool> IsLightThemeProperty =
        AvaloniaProperty.Register<CadViewport, bool>(nameof(IsLightTheme), defaultValue: false);

    public bool IsLightTheme
    {
        get => GetValue(IsLightThemeProperty);
        set => SetValue(IsLightThemeProperty, value);
    }

    public CadCursorState CurrentCursorState {get; set;} = CadCursorState.PickCross;
    private Point _currentMouseScreenPos;

    public Controller.CadController? Controller { get; set; }

    public Point3d WorldCenter { get; set; } = Point3d.Origin;
    public double Zoom { get; set; } = 1.0;

    public Entity? ActiveCommandPreview { get; set; }

    public void RestoreViewport(ViewportTableRecord vtr)
    {
        WorldCenter = vtr.Center;
        if (Bounds.Height > 0)
            Zoom = Bounds.Height / vtr.ViewHeight;
        else
            Zoom = 1.0;
        InvalidateVisual();
    }

    public void UpdateViewportRecord(ViewportTableRecord vtr)
    {
        vtr.Center = WorldCenter;
        vtr.ViewHeight = Bounds.Height > 0 ? Bounds.Height / Zoom : 100.0;
    }
    public Point? SelectionStartPoint { get; set; }
    public Point? SelectionEndPoint { get; set; }

    public SnapType ActiveSnapType { get; private set; } = SnapType.None;
    public Point3d? ActiveSnapPoint { get; private set; }

    private bool _isPanning = false;
    private Point _lastMousePos;
    private readonly DrawingService _drawingService = new();

    static CadViewport()
    {
        AffectsRender<CadViewport>(IsLightThemeProperty);
    }

    public CadViewport()
    {
        ClipToBounds = true;
        Focusable = true;
        _currentMouseScreenPos = new Point(120, 120);
        UpdateSystemCursor();
    }

    public void UpdateSystemCursor()
    {
        if(_isPanning)
        {
            Cursor = new Cursor(StandardCursorType.Hand);
        }
        else
        {
            Cursor = new Cursor(StandardCursorType.None);
        }
    }

    public Point WorldToScreen(Point3d worldPt)
    {
        double viewportCenterX = Bounds.Width / 2;
        double viewportCenterY = Bounds.Height / 2;

        double screenX = viewportCenterX + (worldPt.X - WorldCenter.X) * Zoom;
        double screenY = viewportCenterY - (worldPt.Y - WorldCenter.Y) * Zoom;

        return new Point(screenX, screenY);
    }

    public Point3d ScreenToWorld(Point screenPt)
    {
        double viewportCenterX = Bounds.Width / 2;
        double viewportCenterY = Bounds.Height / 2;

        double worldX = WorldCenter.X + (screenPt.X - viewportCenterX) / Zoom;
        double worldY = WorldCenter.Y - (screenPt.Y - viewportCenterY) / Zoom;

        return new Point3d(worldX, worldY, 0.0);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var pos = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;

        if (properties.IsMiddleButtonPressed)
        {
            _isPanning = true;
            _lastMousePos = pos;
            UpdateSystemCursor();
            e.Handled = true;
        }
        else if (properties.IsLeftButtonPressed)
        {
            var snappedPt = GetSnappedPoint(pos, out var snapType, out var snapPt);
            Controller?.OnPointerPressed(snappedPt, e);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _currentMouseScreenPos = e.GetPosition(this);

        if (_isPanning)
        {
            double dx = _currentMouseScreenPos.X - _lastMousePos.X;
            double dy = _currentMouseScreenPos.Y - _lastMousePos.Y;
            WorldCenter = new Point3d(WorldCenter.X - dx / Zoom, WorldCenter.Y + dy / Zoom, 0);
            _lastMousePos = _currentMouseScreenPos;
            InvalidateVisual();
        }
        else
        {
            var snappedPt = GetSnappedPoint(_currentMouseScreenPos, out var snapType, out var snapPt);
            ActiveSnapType = snapType;
            ActiveSnapPoint = snapType != SnapType.None ? snapPt : null;

            Controller?.OnPointerMoved(snappedPt);
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_isPanning && e.InitialPressMouseButton == MouseButton.Middle)
        {
            _isPanning = false;
            UpdateSystemCursor();
            e.Handled = true;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        var screenPos = e.GetPosition(this);
        var worldPos = ScreenToWorld(screenPos);

        double scale = e.Delta.Y > 0 ? 1.15 : 1 / 1.15;
        double newZoom = Math.Clamp(Zoom * scale, 0.001, 1000.0);

        double viewportCenterX = Bounds.Width / 2;
        double viewportCenterY = Bounds.Height / 2;

        // Ajusta o WorldCenter para manter o ponto sob o mouse na mesma posição de tela
        WorldCenter = new Point3d(
            worldPos.X - (screenPos.X - viewportCenterX) / newZoom,
            worldPos.Y + (screenPos.Y - viewportCenterY) / newZoom,
            0.0
        );

        Zoom = newZoom;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // 1. Desenha o Fundo
        Application.Current!.Resources.TryGetResource("Theme.ViewportBg", Application.Current.ActualThemeVariant, out var resource);
        var bgBrush = resource as SolidColorBrush;
        context.DrawRectangle(bgBrush, null, new Rect(0, 0, Bounds.Width, Bounds.Height));

        // 2. Desenha a Grade (Grid)
        DrawGrid(context);

        // 3. Desenha Eixos do Sistema
        DrawSystemAxes(context);

        // 4. Desenha as Entidades do Banco de Dados
        if (Controller != null)
            _drawingService.DrawDatabase(context, Controller, WorldToScreen, Zoom);

        // 5. Desenha o Preview do Comando Ativo
        if (ActiveCommandPreview != null && Controller != null)
        {
            _drawingService.DrawEntity(context, ActiveCommandPreview, Controller,
                                    isSelected: false, isPreview: true,
                                    WorldToScreen, Zoom);
        }

        // 6. Desenha o Indicador de Snap
        DrawSnapIndicator(context);

        // 7. Desenha o Cursor Customizado (CAD Style)
        DrawCadCursor(context);

        // 8. Desenha o Retângulo de Seleção (Crossing / Window)
        DrawSelectionBox(context);
    }

    private void DrawSelectionBox(DrawingContext context)
    {
        if (!SelectionStartPoint.HasValue || !SelectionEndPoint.HasValue) return;

        var p1 = SelectionStartPoint.Value;
        var p2 = SelectionEndPoint.Value;

        var rect = new Rect(
            Math.Min(p1.X, p2.X),
            Math.Min(p1.Y, p2.Y),
            Math.Abs(p1.X - p2.X),
            Math.Abs(p1.Y - p2.Y)
        );

        bool isCrossing = p2.X < p1.X;

        Color fillColor = isCrossing ? Color.FromArgb(40, 0, 204, 122) : Color.FromArgb(40, 0, 122, 204);
        Color borderColor = isCrossing ? Color.FromRgb(0, 204, 122) : Color.FromRgb(0, 122, 204);

        var brush = new SolidColorBrush(fillColor);
        var pen = new Pen(new SolidColorBrush(borderColor), 1.0);
        if (isCrossing)
        {
            pen.DashStyle = DashStyle.Dash;
        }

        context.DrawRectangle(brush, pen, rect);
    }

    private void DrawCadCursor(DrawingContext context)
    {
        if (_isPanning) return;

        var pen = new Pen(IsLightTheme ? Brushes.Black : Brushes.White, 1.0);
        double pickBoxSize = 10;
        double crossSize = 100; // porcentagem da largura ou altura do viewport
        double size = Math.Max(Bounds.Width, Bounds.Height) * crossSize / 100;

        if (CurrentCursorState == CadCursorState.PickCross || CurrentCursorState == CadCursorState.Crosshair)
        {
            // Cruz com caixa de seleção
            context.DrawLine(pen, new Point(_currentMouseScreenPos.X - size, _currentMouseScreenPos.Y), new Point(_currentMouseScreenPos.X + size, _currentMouseScreenPos.Y));
            context.DrawLine(pen, new Point(_currentMouseScreenPos.X, _currentMouseScreenPos.Y - size), new Point(_currentMouseScreenPos.X, _currentMouseScreenPos.Y + size));
        }

        Application.Current!.Resources.TryGetResource("Theme.ViewportBg", Application.Current.ActualThemeVariant, out var resource);
        var pickBoxBg = resource as SolidColorBrush;

        if (CurrentCursorState == CadCursorState.PickCross || CurrentCursorState == CadCursorState.Pickbox)
        {
            // Quadrado de seleção
            context.DrawRectangle(pickBoxBg, pen, new Rect(_currentMouseScreenPos.X - pickBoxSize / 2, _currentMouseScreenPos.Y - pickBoxSize / 2, pickBoxSize, pickBoxSize));
        }
    }

    private void DrawGrid(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

        var worldMin = ScreenToWorld(new Point(0, Bounds.Height));
        var worldMax = ScreenToWorld(new Point(Bounds.Width, 0));

        double minSpacingPixels = 50;
        double worldMinSpacing = minSpacingPixels / Zoom;

        double log = Math.Log10(worldMinSpacing);
        double powerOf10 = Math.Pow(10, Math.Floor(log));

        double gridSpacing;
        double ratio = worldMinSpacing / powerOf10;
        if (ratio < 2) gridSpacing = powerOf10;
        else if (ratio < 5) gridSpacing = powerOf10 * 2;
        else gridSpacing = powerOf10 * 5;

        double startX = Math.Ceiling(worldMin.X / gridSpacing) * gridSpacing;
        double startY = Math.Ceiling(worldMin.Y / gridSpacing) * gridSpacing;

        Application.Current!.Resources.TryGetResource("Theme.ViewportGrid", Application.Current.ActualThemeVariant, out var resource);
        var gridBrush = resource as SolidColorBrush;
        var gridPen = new Pen(gridBrush, 0.8);

        // Linhas verticais
        for (double x = startX; x <= worldMax.X; x += gridSpacing)
        {
            if (Math.Abs(x) < 1e-6) continue;
            Point p1 = WorldToScreen(new Point3d(x, worldMin.Y));
            Point p2 = WorldToScreen(new Point3d(x, worldMax.Y));
            context.DrawLine(gridPen, p1, p2);
        }

        // Linhas horizontais
        for (double y = startY; y <= worldMax.Y; y += gridSpacing)
        {
            if (Math.Abs(y) < 1e-6) continue;
            Point p1 = WorldToScreen(new Point3d(worldMin.X, y));
            Point p2 = WorldToScreen(new Point3d(worldMax.X, y));
            context.DrawLine(gridPen, p1, p2);
        }
    }

    private void DrawSystemAxes(DrawingContext context)
    {
        var worldMin = ScreenToWorld(new Point(0, Bounds.Height));
        var worldMax = ScreenToWorld(new Point(Bounds.Width, 0));

        Application.Current!.Resources.TryGetResource("Theme.ViewportAxisX", Application.Current.ActualThemeVariant, out var resourceX);
        var xAxisBrush = resourceX as SolidColorBrush;
        Application.Current!.Resources.TryGetResource("Theme.ViewportAxisY", Application.Current.ActualThemeVariant, out var resourceY);
        var yAxisBrush = resourceY as SolidColorBrush;

        var xAxisPen = new Pen(xAxisBrush, 1.2);
        var yAxisPen = new Pen(yAxisBrush, 1.2);

        // Eixo Y (X = 0)
        if (worldMin.X <= 0 && worldMax.X >= 0)
        {
            Point p1 = WorldToScreen(new Point3d(0, worldMin.Y));
            Point p2 = WorldToScreen(new Point3d(0, worldMax.Y));
            context.DrawLine(yAxisPen, p1, p2);
        }

        // Eixo X (Y = 0)
        if (worldMin.Y <= 0 && worldMax.Y >= 0)
        {
            Point p1 = WorldToScreen(new Point3d(worldMin.X, 0));
            Point p2 = WorldToScreen(new Point3d(worldMax.X, 0));
            context.DrawLine(xAxisPen, p1, p2);
        }
    }

    private void DrawSnapIndicator(DrawingContext context)
    {
        if (ActiveSnapType == SnapType.None || !ActiveSnapPoint.HasValue) return;

        Point screenSnap = WorldToScreen(ActiveSnapPoint.Value);
        var snapPen = new Pen(Brushes.Lime, 2.0);
        double size = 12;
        double half = size / 2;

        if (ActiveSnapType == SnapType.Endpoint)
        {
            context.DrawRectangle(null, snapPen, new Rect(screenSnap.X - half, screenSnap.Y - half, size, size));
        }
        else if (ActiveSnapType == SnapType.Midpoint)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(screenSnap.X, screenSnap.Y - half), IsClosed = true };
            figure.Segments!.Add(new LineSegment { Point = new Point(screenSnap.X - half, screenSnap.Y + half) });
            figure.Segments!.Add(new LineSegment { Point = new Point(screenSnap.X + half, screenSnap.Y + half) });
            geometry.Figures!.Add(figure);
            context.DrawGeometry(null, snapPen, geometry);
        }
        else if (ActiveSnapType == SnapType.Center)
        {
            context.DrawEllipse(null, snapPen, screenSnap, half, half);
        }
    }

    private Point3d GetSnappedPoint(Point screenMousePos, out SnapType activeSnap, out Point3d snapPoint)
    {
        activeSnap = SnapType.None;
        snapPoint = Point3d.Origin;

        var db = Controller?.Database;
        if (db == null || CurrentCursorState != CadCursorState.Crosshair)
            return ScreenToWorld(screenMousePos);

        double snapAperturePixels = 15;
        double bestDistance = snapAperturePixels;

        if (!db.TryGetObject(db.BlockTableId, out var btObj) || btObj is not BlockTable bt)
            return ScreenToWorld(screenMousePos);

        ObjectId modelSpaceId = bt[BlockTableRecord.ModelSpace];
        if (modelSpaceId.IsNull || !db.TryGetObject(modelSpaceId, out var btrObj) || btrObj is not BlockTableRecord modelSpace)
            return ScreenToWorld(screenMousePos);

        foreach (var entId in modelSpace.GetEntityIds())
        {
            if (!db.TryGetObject(entId, out var entObj) || entObj is not Entity ent)
                continue;

            if (ent is Entity)
            {
                foreach (var (pt, type) in ent.GetOsnapPoints())
                    CheckSnapCandidate(screenMousePos, pt, type, ref activeSnap, ref snapPoint, ref bestDistance);
            }
        }

        if (activeSnap != SnapType.None)
        {
            return snapPoint;
        }

        return ScreenToWorld(screenMousePos);
    }

    private void CheckSnapCandidate(Point screenMousePos, Point3d worldCandidate, SnapType type, ref SnapType activeSnap, ref Point3d snapPoint, ref double bestDistance)
    {
        Point screenCandidate = WorldToScreen(worldCandidate);
        double dx = screenMousePos.X - screenCandidate.X;
        double dy = screenMousePos.Y - screenCandidate.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        if (dist < bestDistance)
        {
            bestDistance = dist;
            activeSnap = type;
            snapPoint = worldCandidate;
        }
    }
}


public enum CadCursorState
{
    PickCross,  // Modo de seleção (Cruz com caixa de seleção)
    Crosshair,  // Modo de desenho (Cruz)
    Pickbox    // Modo de seleção (Quadrado)
}
