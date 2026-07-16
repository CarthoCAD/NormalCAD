using System;
using System.Collections.Generic;
using Avalonia.Input;
using NormalCAD.Core;
using NormalCAD.Core.ApplicationServices;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Host;
using NormalCAD.View.Controls;
using NormalCAD.Controller.Commands;
using NormalCAD.Controller.Providers;

namespace NormalCAD.Controller
{
    public class CadController
    {
        public CadViewport Viewport { get; }
        public CmdManager CmdManager { get; }
        public InputManager InputManager { get; }
        public EntityPropertyManager EntityPropertyManager { get; }

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

        public CadController(CadViewport viewport)
        {
            if (Application.Host == null)
            {
                Application.Host = new Host.ApplicationHost();
            }

            Application.Host.CreateDocument();
            Viewport = viewport;
            Viewport.Controller = this;
            CmdManager = new CmdManager(this);
            InputManager = new InputManager(this);
            EntityPropertyManager = new EntityPropertyManager(this);

            SubscribeToDatabaseEvents(
                Application.DocumentManager.MdiActiveDocument!.Database);

            SetCommand(new BaseCommand());
        }

        private void SubscribeToDatabaseEvents(Database db)
        {
            db.ObjectAppended += OnDatabaseObjectEvent;
            db.ObjectModified += OnDatabaseObjectEvent;
            db.ObjectErased += OnDatabaseObjectEvent;
        }

        private void UnsubscribeFromDatabaseEvents(Database db)
        {
            db.ObjectAppended -= OnDatabaseObjectEvent;
            db.ObjectModified -= OnDatabaseObjectEvent;
            db.ObjectErased -= OnDatabaseObjectEvent;
        }

        private void OnDatabaseObjectEvent(object? sender, ObjectEventArgs e)
        {
            DatabaseChanged?.Invoke();
            Viewport.InvalidateVisual();
        }

        public void SetDocument(Document document)
        {
            var previous = Application.DocumentManager.MdiActiveDocument;
            if (previous != null && previous != document)
                UnsubscribeFromDatabaseEvents(previous.Database);

            Application.DocumentManager.SetActive(document);
            SubscribeToDatabaseEvents(document.Database);

            ClearSelection();
            SetCommand(new BaseCommand());
            RestoreViewportState();
            DatabaseChanged?.Invoke();
            Viewport.InvalidateVisual();
        }

        public void SetDatabase(Database db, string filePath)
        {
            db.Filename = filePath;
            var doc = new Document(db);
            doc.Editor = new Editor(doc);
            Application.DocumentManager.Add(doc);
            SetDocument(doc);
        }

        public void SaveViewportState()
        {
            var db = Application.DocumentManager.MdiActiveDocument?.Database;
            if (db == null) return;

            if (!db.TryGetObject(db.ViewportTableId, out var vtObj) || vtObj is not ViewportTable vt)
                return;

            var activeId = vt[ViewportTable.ActiveViewport];
            if (activeId.IsNull) return;

            var vpr = vt.GetRecord(activeId);
            Viewport.UpdateViewportRecord(vpr);
        }

        public void RestoreViewportState()
        {
            var db = Application.DocumentManager.MdiActiveDocument?.Database;
            if (db == null) return;

            if (!db.TryGetObject(db.ViewportTableId, out var vtObj) || vtObj is not ViewportTable vt)
                return;

            var activeId = vt[ViewportTable.ActiveViewport];
            if (activeId.IsNull) return;

            var vpr = vt.GetRecord(activeId);
            Viewport.RestoreViewport(vpr);
        }

        public void SetCommand(ICadCommand command)
        {
            _activeCommand?.Deactivate();
            _activeCommand = command;
            InputManager.SetCurrentPrompt(_activeCommand.LocalName);
            _activeCommand.Activate(this);            
            ActiveCommandChanged?.Invoke(_activeCommand.LocalName);
            Viewport.InvalidateVisual();
        }

        public void CancelCurrentCommand()
        {
            ClearKeywords();
            ClearSelection();
            SetCommand(new BaseCommand());
        }

        public void ClearKeywords()
        {
            InputManager.ClearKeywords();
        }

        public bool TryHandleKeyword(string text)
        {
            return InputManager.TryHandleKeyword(text);
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

        public void ApplyTheme(bool isLight)
        {
            IsLightTheme = isLight;
            Avalonia.Application.Current!.RequestedThemeVariant = isLight
                ? Avalonia.Styling.ThemeVariant.Light
                : Avalonia.Styling.ThemeVariant.Dark;
            Viewport.IsLightTheme = isLight;
            Viewport.InvalidateVisual();
        }
    }
}
