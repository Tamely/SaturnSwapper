using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using SkiaSharp;

namespace Saturn.Backend.Data.Swapper.Styles;

public class StyleSelectorItem
{
    public FStructFallback OptionData;
    public string DisplayName { get; set; }
    public SKData IconSource { get; set; }

    public StyleSelectorItem(FStructFallback option, SKBitmap previewBitmap)
    {
        OptionData = option;
        DisplayName = option.GetOrDefault("VariantName", new FText("Unknown Style")).Text.ToLower().TitleCase();
        if (string.IsNullOrWhiteSpace(DisplayName)) DisplayName = "Unknown Style";
        IconSource = previewBitmap.Encode(SKEncodedImageFormat.Png, 100);
    }
}