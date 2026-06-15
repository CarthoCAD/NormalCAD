using Avalonia.Input;
using System;
using System.Collections.Generic;
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
        public InputManager InputManager { get; }

        private ICadCommand? _activeCommand;
        public ICadCommand? ActiveCommand => _activeCommand;

        public string ActiveLayer { get; set; } = "0";
        public EntityColor ActiveColor { get; set; } = EntityColor.ByLayer;
        public bool IsLightTheme { get; set; } = false;

        private readonly HashSet<ObjectId> _selectedEntityIds = [];
        public IReadOnlyCollection<ObjectId> SelectedEntityIds => _selectedEntityIds;

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
            InputManager = new InputManager(this);

            SetCommand(new BaseCommand());
        }

        public void SetDatabase(Database db)
        {
            Database = db;
            Viewport.Database = db;
            ClearSelection();
            Viewport.ActiveCommandPreview = null;
            SetCommand(new BaseCommand());
            DatabaseChanged?.Invoke();
            Viewport.InvalidateVisual();
        }

        public void SetCommand(ICadCommand command)
        {
            _activeCommand?.Deactivate();
            Viewport.ActiveCommandPreview = null;
            _activeCommand = command;
            _activeCommand.Activate(this);
            
            InputManager.SetCurrentPrompt(_activeCommand.LocalName);
            ActiveCommandChanged?.Invoke(_activeCommand.LocalName);
            Viewport.InvalidateVisual();
        }

        public void CancelCurrentCommand()
        {
            ClearSelection();
            SetCommand(new BaseCommand());
        }

        public bool IsSelected(ObjectId id) => _selectedEntityIds.Contains(id);

        public void AddToSelection(ObjectId id)
        {
            _selectedEntityIds.Add(id);
            NotifySelectionChanged();
        }

        public void RemoveFromSelection(ObjectId id)
        {
            _selectedEntityIds.Remove(id);
            NotifySelectionChanged();
        }

        public void ClearSelection()
        {
            _selectedEntityIds.Clear();
            NotifySelectionChanged();
        }

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            InputManager.OnPointerPressed(worldPt, e);
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            InputManager.OnPointerMoved(worldPt);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            InputManager.OnKeyDown(e);
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
