using System.Collections.Generic;
using System.Data;
using System.Linq;
using UAssetAPI.IO;
using UAssetAPI.PropertyTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace UAssetAPI.ExportTypes
{
    /// <summary>
    /// A regular export, with no special serialization. Serialized as a None-terminated property list.
    /// </summary>
    public class NormalExport : Export
    {
        public List<UProperty> Data;

        /// <summary>
        /// Gets or sets the value associated with the specified key. This operation loops linearly, so it may not be suitable for high-performance environments.
        /// </summary>
        /// <param name="key">The key associated with the value to get or set.</param>
        public UProperty this[FName key]
        {
            get
            {
                for (int i = 0; i < Data.Count; i++)
                {
                    if (Data[i].Name == key.Value.Value) return Data[i];
                }
                return null;
            }
            set
            {
                for (int i = 0; i < Data.Count; i++)
                {
                    if (Data[i].Name == key.Value.Value)
                    {
                        Data[i] = value;
                        Data[i].Name = key.Value.Value;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key. This operation loops linearly, so it may not be suitable for high-performance environments.
        /// </summary>
        /// <param name="key">The key associated with the value to get or set.</param>
        public UProperty this[string key]
        {
            get
            {
                return this[FName.FromString(Asset, key)];
            }
            set
            {
                this[FName.FromString(Asset, key)] = value;
            }
        }


        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        public UProperty this[int index]
        {
            get
            {
                return Data[index];
            }
            set
            {
                Data[index] = value;
            }
        }

        public NormalExport(Export super)
        {
            Asset = super.Asset;
            Extras = super.Extras;
        }

        public NormalExport(UAsset asset, byte[] extras) : base(asset, extras)
        {

        }

        public NormalExport(List<UProperty> data, UAsset asset, byte[] extras) : base(asset, extras)
        {
            Data = data;
        }

        public NormalExport()
        {

        }

        public override void Read(AssetBinaryReader reader, int nextStarting = 0)
        {
            Data = new List<UProperty>();

            var unversionedHeader = new FUnversionedHeader(reader);
            var schema = reader.Asset.Mappings.Schemas.FirstOrDefault(x => x.Value.Name == reader.Asset.GlobalData.GetScriptName((ulong)ClassIndex.Index));
            if (string.IsNullOrWhiteSpace(schema.Key))
                throw new NoNullAllowedException($"Cannot find '{reader.Asset.GlobalData.GetScriptName((ulong)ClassIndex.Index)} in mappings!");

            int absoluteIdx = 0;
            int schemaIdx = 0;
            int zeroIdx = 0;

            while (unversionedHeader.Fragments.Count > 0)
            {
                var frag = unversionedHeader.Fragments.First();

                if (unversionedHeader.bHasNonZeroValues)
                {
                    schemaIdx += frag.SkipNum;
                }

                var remainingValues = frag.ValueNum;

                do
                {
                    var prop = schema.Value.Properties.ToList()
                        .Find(x => absoluteIdx + x.Value.SchemaIndex == schemaIdx);
                    while (prop.Value == null || string.IsNullOrEmpty(prop.Value.Name))
                    {
                        absoluteIdx += schema.Value.PropCount;
                        schema = reader.Asset.Mappings.Schemas.First(x => x.Value.Name == schema.Value.SuperType);
                        prop = schema.Value.Properties.ToList()
                            .Find(x => absoluteIdx + x.Value.SchemaIndex == schemaIdx);
                    }

                    var propType = prop.Value.PropertyData.Type.ToString();
                    var isNonZero = !frag.bHasAnyZeroes || !unversionedHeader.ZeroMask.Get(zeroIdx);

                    Data.Add(new UProperty()
                    {
                        Type = propType,
                        Name = prop.Value.Name,
                        IsZero = !isNonZero
                    });

                    if (frag.bHasAnyZeroes)
                    {
                        zeroIdx++;
                    }

                    schemaIdx++;
                    remainingValues--;
                } while (remainingValues > 0);
                
                unversionedHeader.Fragments.RemoveFirst();
            }
        }

        public override void ResolveAncestries(UnrealPackage asset, AncestryInfo ancestrySoFar)
        {
            var ancestryNew = (AncestryInfo)ancestrySoFar.Clone();
            ancestryNew.SetAsParent(GetClassTypeForAncestry(asset));

            if (Data != null)
            {
                
            }
            base.ResolveAncestries(asset, ancestrySoFar);
        }

        public override void Write(AssetBinaryWriter writer)
        {
            //MainSerializer.GenerateUnversionedHeader(ref Data, GetClassTypeForAncestry(writer.Asset), writer.Asset)?.Write(writer);
            for (int j = 0; j < Data.Count; j++)
            {
                //PropertyData current = Data[j];
                //MainSerializer.Write(current, writer, true);
            }
            if (!writer.Asset.HasUnversionedProperties) writer.Write(new FName(writer.Asset, "None"));
        }
    }
}
