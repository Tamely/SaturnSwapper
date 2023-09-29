# Fortnite Files Documentation

## Skins
All skins have Character IDs. They can be found at `FortniteGame/Content/Athena/Items/Cosmetics/Characters/` and `FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics/Characters/`. In each ID, there is a `UObject[]` called `BaseCharacterParts`. This array stores all the character parts of the skin in question. `Character Parts` are just a container of components such as meshes, materials, and animation blueprints (parts of a character). We edit the components in these `Character Parts` in Saturn to the components of other items. To get the skin's icon to show ingame, Saturn edits the `HeroDefinition` in the ID to that of another skin.

## Backblings
All backblings have Backpack IDs. They can be found at `FortniteGame/Content/Athena/Items/Cosmetics/Backpacks/` and `FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics/Backpacks/`. In each ID, there is a `UObject[]` called `CharacterParts`. The rest of the process is the exact same as Skins.

## Pickaxes
All pickaxes have Pickaxe IDs. They can be found at `FortniteGame/Content/Athena/Items/Cosmetics/Pickaxes/` and `FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics/Pickaxes/`. In each ID, there is a `UObject` called `WeaponItemDefinition`. We edit these in Saturn to those of other items.

## Emotes
All emotes have Emote IDs. They can be found at `FortniteGame/Content/Athena/Items/Cosmetics/Dances/` and `FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics/Dances/`. We directly swap one ID to another.
    - NOTE: To do this, they must have the same Series

## Gliders
All gliders have Glider IDs. They can be found at `FortniteGame/Content/Athena/Items/Cosmetics/Gliders/` and `FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics/Gliders/`. We directly swap one ID to another.
    - NOTE: To do this, they must have the same Series and Mesh

## Fallbacks
If any asset becomes invalid for whatever reason, it will fallback to its fallback counterpart found in `/FortniteGame/Content/Balance/DefaultGameDataCosmetics.uasset`. This is leveraged by Saturn to bypass the mesh check on skins because you're not technically wearing the fallback skin.