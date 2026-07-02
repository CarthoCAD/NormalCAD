using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NormalCAD.Resources;

namespace NormalCAD.View.Controls;

public partial class MenuBar : UserControl
{
    public Controller.CadController? Controller { get; set; }


    public MenuBar()
    {
        InitializeComponent();
        PopulateMenu(GetMenuStructure());
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private static string MenuBase => PanelResources.Get("MENUBAR.MENU.BASE");
    private static string MenuFile => PanelResources.Get("MENUBAR.MENU.FILE");
    private static string MenuEdit => PanelResources.Get("MENUBAR.MENU.EDIT");
    private static string MenuDraw => PanelResources.Get("MENUBAR.MENU.DRAW");
    private static string MenuHelp => PanelResources.Get("MENUBAR.MENU.HELP");
    private static string ItemBaseOptions => PanelResources.Get("MENUBAR.ITEM.BASE.OPTIONS");
    private static string ItemBaseChangeTheme => PanelResources.Get("MENUBAR.ITEM.BASE.CHANGETHEME");
    private static string ItemFileOpen => PanelResources.Get("MENUBAR.ITEM.FILE.OPEN");
    private static string ItemFileSave => PanelResources.Get("MENUBAR.ITEM.FILE.SAVE");
    private static string ItemFileSaveAs => PanelResources.Get("MENUBAR.ITEM.FILE.SAVEAS");
    private static string ItemFileExit => PanelResources.Get("MENUBAR.ITEM.FILE.EXIT");
    private static string ItemEditUndo => PanelResources.Get("MENUBAR.ITEM.EDIT.UNDO");
    private static string ItemEditRedo => PanelResources.Get("MENUBAR.ITEM.EDIT.REDO");
    private static string ItemEditSelect => PanelResources.Get("MENUBAR.ITEM.EDIT.SELECT");
    private static string ItemEditErase => PanelResources.Get("MENUBAR.ITEM.EDIT.ERASE");
    private static string ItemEditCleanAll => PanelResources.Get("MENUBAR.ITEM.EDIT.CLEANALL");
    private static string ItemDrawLine => PanelResources.Get("MENUBAR.ITEM.DRAW.LINE");
    private static string ItemDrawCircle => PanelResources.Get("MENUBAR.ITEM.DRAW.CIRCLE");
    private static string ItemDrawArc => PanelResources.Get("MENUBAR.ITEM.DRAW.ARC");
    private static string ItemDrawPolyline => PanelResources.Get("MENUBAR.ITEM.DRAW.POLYLINE");
    private static string ItemHelpAbout => PanelResources.Get("MENUBAR.ITEM.HELP.ABOUT");

    private static List<MenuEntry> GetMenuStructure() =>
    [
        new(MenuBase, null, "avares://NormalCAD/Assets/normalcad.ico",
        [
            new(ItemBaseOptions, "_.OPTIONS", null),
            new(null, null, null),
            new(ItemBaseChangeTheme, "_.THEME", null)
        ]),
        new(MenuFile, null, null,
        [
            new(ItemFileOpen, "_.OPEN", null),
            new(ItemFileSave, "_.SAVE", null),
            new(ItemFileSaveAs, "_.SAVEAS", null),
            new(null, null, null),
            new(ItemFileExit, "_.QUIT", null)
        ]),
        new(MenuEdit, null, null,
        [
            new(ItemEditUndo, "_.UNDO", null),
            new(ItemEditRedo, "_.REDO", null),
            new(null, null, null),
            new(ItemEditSelect, "_.SELECT", null),
            new(ItemEditErase, "_.ERASE", null),
            new(ItemEditCleanAll, "_.CLEANALL", null)
        ]),
        new(MenuDraw, null, null,
        [
            new(ItemDrawLine, "_.LINE", null),
            new(ItemDrawCircle, "_.CIRCLE", null),
            new(ItemDrawArc, "_.ARC", null),
            new(ItemDrawPolyline, "_.PLINE", null)
        ]),
        new(MenuHelp, null, null,
        [
            new(ItemHelpAbout, "_.ABOUT", null)
        ])
    ];

    private void PopulateMenu(List<MenuEntry> structure)
    {
        var menuControl = this.FindControl<Menu>("TopMenu")!;
        menuControl.Items.Clear();

        foreach (var entry in structure)
            menuControl.Items.Add(BuildMenuItem(entry));
    }

    private Control BuildMenuItem(MenuEntry entry)
    {
        if (entry.IsSeparator) return new Separator();

        var item = new MenuItem { Header = entry.Header };

        if (entry.Icon is not null)
        {
            using var stream = AssetLoader.Open(new Uri(entry.Icon));
            var source = new Bitmap(stream);
            item.Header = new Image
            {
                Source = source,
                Width = 20,
                Height = 20
            };
        }

        if (entry.IsParent)
        {
            foreach (var child in entry.Children!)
                item.Items.Add(BuildMenuItem(child));
        }
        else if (entry.CommandName is not null)
        {
            item.Tag = entry.CommandName;
            item.Click += (sender, e) =>
            {
                var item = sender as MenuItem;
                var cmdName = item?.Tag as string;
                if (cmdName is not null)
                {
                    _ = Controller!.CmdManager.ExecuteCommand(cmdName);
                }
                else
                {
                    Controller?.CancelCurrentCommand();
                }
            };
        }

        return item;
    }
    
}

public record MenuEntry(
    string? Header,        // null = separador
    string? CommandName,   // null = parent item (opens submenu only)
    string? Icon,          // null = no icon
    List<MenuEntry>? Children = null
)
{
    public bool IsSeparator => Header is null;
    public bool IsParent    => Children is { Count: > 0 };
}
