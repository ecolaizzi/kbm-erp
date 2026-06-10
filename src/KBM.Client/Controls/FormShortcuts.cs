using System;
using System.Windows;
using System.Windows.Input;

namespace KBM.Client.Controls;

/// <summary>
/// Scorciatoie standard delle maschere documento (stile NTS Business Cube):
/// F9 = Salva, F6 = Stampa, ESC = Esci/Annulla. Vedi help pc05 "Le maschere in Business".
/// </summary>
public static class FormShortcuts
{
    public static void Apply(UIElement target, Action? save = null, Action? print = null, Action? exit = null)
    {
        if (save is not null) Add(target, Key.F9, save);
        if (print is not null) Add(target, Key.F6, print);
        if (exit is not null) Add(target, Key.Escape, exit);
    }

    private static void Add(UIElement target, Key key, Action action) =>
        target.InputBindings.Add(new KeyBinding(new RelayCommand(action), key, ModifierKeys.None));
}
