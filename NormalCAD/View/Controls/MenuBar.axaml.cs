using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace NormalCAD.View.Controls;

public partial class MenuBar : UserControl
{
    private readonly Controller.CmdManager _cmdManager = new(null!); // Será injetado depois
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
        // MenuEntry(Header, CommandName, Icon, Children)
        new("Base", null, "avares://NormalCAD/Assets/normalcad.ico",
        [
            new("Options", "options", null),
            new(null, null, null), // separator
            new("Change Theme", "change_theme", null)
        ]),
        new("File", null, null,
        [
            new("Open", "file.open", null),
            new("Save", "file.save", null),
            new(null, null, null), // separator
            new("Exit", "file.exit", null)
        ]),
        new("Edit", null, null,
        [
            new("Undo", "edit.undo", null),
            new("Redo", "edit.redo", null),
            new(null, null, null), // separator
            new("Select", "edit.select", null),
            new("Erase", "edit.erase", null),
            new("Clean All", "edit.clean_all", null)
        ]),
        new("Draw", null, null,
        [
            new("Line", "draw.line", null),
            new("Circle", "draw.circle", null)
        ]),
        new("Help", null, null,
        [
            new("About", "help.about", null)
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
                    _cmdManager.ExecuteCommand(cmdName);
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
