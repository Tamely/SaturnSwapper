using System.ComponentModel;

namespace Saturn.Backend.Data.Swapper.Assets;

public enum EAssetType
{
    [Description("Outfits")] Outfit,

    [Description("Back Blings")] Backpack,

    [Description("Pickaxes")] Pickaxe,

    [Description("Gliders")] Glider,

    [Description("Pets")] Pet,

    [Description("Weapons")] Weapon,

    [Description("Emotes")] Dance,

    [Description("Vehicles")] Vehicle,
    
    [Description("Galleries")] Gallery,

    [Description("Props")] Prop,

    [Description("Meshes")] Mesh,

    [Description("Music Packs")] Music,
    
    [Description("Toys")] Toy,
    
    [Description("Wildlife")] Wildlife,
}

public enum EFortCustomPartType : byte
{
    Head = 0,
    Body = 1,
    Hat = 2,
    Backpack = 3,
    MiscOrTail = 4,
    Face = 5,
    Gameplay = 6,
    NumTypes = 7
}

public enum ECustomHatType : byte
{
    HeadReplacement,
    Cap,
    Mask,
    Helmet,
    Hat,
    None
}

public enum ESortType
{
    [Description("Default")] Default,
    [Description("A-Z")] AZ,
    [Description("Season")] Season,
    [Description("Rarity")] Rarity,
    [Description("Series")] Series
}

public enum EFortCustomGender : byte
{
    Invalid = 0,
    Male = 1,
    Female = 2,
    Both = 3
}