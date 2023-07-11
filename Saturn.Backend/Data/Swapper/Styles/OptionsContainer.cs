using System.Collections.Generic;

namespace Saturn.Backend.Data.Swapper.Styles;

public class OptionsContainer
{
    public List<StyleSelectorItem> Items { get; set; } = new();
    public int SelectedIndex { get; set; }

    public bool HasItems => Items.Count != 0;
}