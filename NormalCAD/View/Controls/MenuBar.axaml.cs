using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

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

    private static List<MenuEntry> GetMenuStructure() =>
    [
        new("Base", null, "avares://NormalCAD/Assets/normalcad.ico",
        [
            new("Options", "OPTIONS", null),
            new(null, null, null),
            new("Change Theme", "THEME", null)
        ]),
        new("File", null, null,
        [
            new("Open", null, null,
            [
                new("Open DXF...", "DXFIN", null),
                new("Open DWG...", "DWGIN", null)
            ]),
            new("Save", null, null,
            [
                new("Save DXF...", "DXFOUT", null),
                new("Save DWG...", "DWGOUT", null)
            ]),
            new(null, null, null),
            new("Exit", "QUIT", null)
        ]),
        new("Edit", null, null,
        [
            new("Undo", "UNDO", null),
            new("Redo", "REDO", null),
            new(null, null, null),
            new("Select", "SELECT", null),
            new("Erase", "ERASE", null),
            new("Clean All", "CLEANALL", null)
        ]),
        new("Draw", null, null,
        [
            new("Line", "LINE", null),
            new("Circle", "CIRCLE", null),
            new("Arc", "ARC", null),
            new("Polyline", "PLINE", null)
        ]),
        new("Help", null, null,
        [
            new("About", "ABOUT", null)
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
    string? CommandName,   // null = item pai (só abre submenu)
    string? Icon,          // null = sem ícone
    List<MenuEntry>? Children = null
)
{
    public bool IsSeparator => Header is null;
    public bool IsParent    => Children is { Count: > 0 };
}
