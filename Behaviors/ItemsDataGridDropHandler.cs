using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using RelinkModOrganizer.ViewModels;

namespace RelinkModOrganizer.Behaviors;

public sealed class ItemsDataGridDropHandler : BaseDataGridDropHandler<ModItemViewModel>
{
    protected override ModItemViewModel MakeCopy(ObservableCollection<ModItemViewModel> parentCollection, ModItemViewModel item)
    {
        throw new System.NotImplementedException();
    }

    protected override bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not ModItemViewModel sourceItem
         || targetContext is not ModListViewModel vm
         || dg.GetVisualAt(e.GetPosition(dg)) is not Control targetControl
         || targetControl.DataContext is not ModItemViewModel targetItem)
        {
            return false;
        }

        var items = vm.ModItems;
        return RunDropAction(dg, e, bExecute, sourceItem, targetItem, items);
    }

    public override async void Drop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        base.Drop(sender, e, sourceContext, targetContext);

        if (targetContext is ModListViewModel vm)
            await vm.ReOrderAsync();
    }
}