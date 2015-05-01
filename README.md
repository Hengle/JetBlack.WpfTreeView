# JetBlack.WpfTreeView

An implementation of a viewmodel for a WPF TreeView.

The view model optionally supports lazy loading. The provided example shows
how to do this by browsing the file system.

The key class is the lazy loader.

```cs
public class LoaderResult
{
    public LoaderResult(object child, Func<object, IEnumerable<LoaderResult>> lazyLoader = null)
    {
        Child = child;
        LazyLoader = lazyLoader;
    }

    public object Child { get; private set; }
    public Func<object, IEnumerable<LoaderResult>> LazyLoader { get; private set; }
}
```

The TreeViewModel takes the following parameter in it's constructor `Func<object, IEnumerable<LoaderResult>> lazyLoader`.

For the example this is implemented in the following way:

```cs
private static IEnumerable<LoaderResult> EnumerateChildren(object parent)
{
    var results = new List<LoaderResult>();

    var directoryInfo = parent as DirectoryInfo;
    if (directoryInfo == null)
        return results;

    results.AddRange(
        directoryInfo.EnumerateDirectories()
            .Select(child => new LoaderResult(child, EnumerateChildren)));

    results.AddRange(
        directoryInfo.EnumerateFiles()
            .Select(file => new LoaderResult(file)));

    return results;
}
```

Note that the files have no children, so the lazy loader parameter is null.

The TreeView is bound to the root of the model, and the style is applied:

```xaml
<TreeView
    BorderThickness="0"
    ItemContainerStyle="{StaticResource ExpandableTreeViewItemStyle}"
    ItemsSource="{Binding Path=ViewModel.Root}"
    VirtualizingStackPanel.IsVirtualizing="True" VirtualizingStackPanel.VirtualizationMode="Standard" />
```

Finally the data templates are defined.

```xaml
<DataTemplate DataType="{x:Type io:DirectoryInfo}">
  <TextBlock Text="{Binding Path=Name}" ToolTip="{Binding Path=FullName}" />
</DataTemplate>

<DataTemplate DataType="{x:Type io:FileInfo}">
  <TextBlock Text="{Binding Path=Name}" ToolTip="{Binding Path=FullName}" />
</DataTemplate>

<HierarchicalDataTemplate DataType="{x:Type viewModels:TreeViewModel}" ItemsSource="{Binding Children}">
  <StackPanel Orientation="Horizontal">
    <CheckBox Focusable="False" IsChecked="{Binding Path=IsChecked}" VerticalAlignment="Center" Margin="0,0,5,0" />
    <ContentPresenter Content="{Binding Path=Value}" />
  </StackPanel>
</HierarchicalDataTemplate>
```