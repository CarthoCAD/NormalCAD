using Avalonia.Input;
using System;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;
using NormalCAD.View.Controls;
using NormalCAD.Controller.Commands;

namespace NormalCAD.Controller
{
    public class CadController
    {
        public Database Database { get; private set; }
        public CadViewport Viewport { get; }
        public CmdManager CmdManager { get; }

        private ICadCommand? _activeCommand;
        public string ActiveLayer { get; set; } = "0";
        public EntityColor ActiveColor { get; set; } = EntityColor.ByLayer;
        public bool IsLightTheme { get; set; } = false;

        public event Action? SelectionChanged;
        public event Action? DatabaseChanged;
        public event Action<string>? ActiveCommandChanged;

        public CadController(Database database, CadViewport viewport)
        {
            Database = database;
            Viewport = viewport;
            Viewport.Database = database;
            Viewport.Controller = this;
            CmdManager = new CmdManager(this);

            SetCommand(new SelectCommand());
        }

        public void SetDatabase(Database db)
        {
            Database = db;
            Viewport.Database = db;
            Viewport.SelectedEntityIds.Clear();
            Viewport.ActiveCommandPreview = null;
            SetCommand(new SelectCommand());
            DatabaseChanged?.Invoke();
            Viewport.InvalidateVisual();
        }

        public void SetCommand(ICadCommand command)
        {
            _activeCommand?.Deactivate();
            Viewport.ActiveCommandPreview = null;
            _activeCommand = command;
            _activeCommand.Activate(this);
            
            ActiveCommandChanged?.Invoke(_activeCommand.Name);
            Viewport.InvalidateVisual();
        }

        public void CancelCurrentCommand()
        {
            SetCommand(new SelectCommand());
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            _activeCommand?.OnPointerPressed(worldPt, e);
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            _activeCommand?.OnPointerMoved(worldPt);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelCurrentCommand();
                e.Handled = true;
            }
            else
            {
                _activeCommand?.OnKeyDown(e);
            }
        }

        public void NotifySelectionChanged()
        {
            SelectionChanged?.Invoke();
        }

        public void NotifyDatabaseChanged()
        {
            DatabaseChanged?.Invoke();
            Viewport.InvalidateVisual();
        }
    }
}
