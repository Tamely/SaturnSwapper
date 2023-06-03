using System;
using System.Collections.Generic;
using System.Text;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.i18N;
using Newtonsoft.Json;
using Saturn.Backend.Data;

namespace CUE4Parse.UE4.Assets.Objects
{
    [JsonConverter(typeof(TextPropertyConverter))]
    public class TextProperty : FPropertyTagType<FText>
    {
        public TextProperty(FAssetArchive Ar, ReadType type)
        {
            Value = type switch
            {
                ReadType.ZERO => new FText(0, ETextHistoryType.None, new FTextHistory.None()),
                _ => new FText(Ar)
            };
        }

        public override void Serialize(List<byte> Ar)
        {
            Ar.AddRange(BitConverter.GetBytes(Value.Flags));
            Ar.Add((byte)Value.HistoryType);
            switch (Value.TextHistory)
            {
                case FTextHistory.Base Base:
                    Ar.AddRange(BitConverter.GetBytes(Base.Namespace.Length + 1));
                    Ar.AddRange(Encoding.UTF8.GetBytes(Base.Namespace + "\0"));
                    
                    Ar.AddRange(BitConverter.GetBytes(Base.Key.Length + 1));
                    Ar.AddRange(Encoding.UTF8.GetBytes(Base.Key + "\0"));
                    
                    Ar.AddRange(BitConverter.GetBytes(Base.SourceString.Length + 1));
                    Ar.AddRange(Encoding.UTF8.GetBytes(Base.SourceString + "\0"));
                    break;
                case FTextHistory.NamedFormat Named:
                    Ar.AddRange(BitConverter.GetBytes(Named.SourceFmt.Flags));
                    Ar.Add((byte)Named.SourceFmt.HistoryType);
                    switch (Named.SourceFmt.TextHistory)
                    {
                        case FTextHistory.Base Base:
                            Ar.AddRange(BitConverter.GetBytes(Base.Namespace.Length + 1));
                            Ar.AddRange(Encoding.UTF8.GetBytes(Base.Namespace + "\0"));
                    
                            Ar.AddRange(BitConverter.GetBytes(Base.Key.Length + 1));
                            Ar.AddRange(Encoding.UTF8.GetBytes(Base.Key + "\0"));
                    
                            Ar.AddRange(BitConverter.GetBytes(Base.SourceString.Length + 1));
                            Ar.AddRange(Encoding.UTF8.GetBytes(Base.SourceString + "\0"));
                            break;
                    }

                    Ar.AddRange(BitConverter.GetBytes(Named.Arguments.Count));
                    foreach (var argument in Named.Arguments)
                    {
                        Ar.AddRange(BitConverter.GetBytes(argument.Key.Length + 1));
                        Ar.AddRange(Encoding.UTF8.GetBytes(argument.Key + "\0"));
                        
                        Ar.Add((byte)argument.Value.Type);
                        switch (argument.Value.Value)
                        {
                            case FText text:
                                Ar.AddRange(BitConverter.GetBytes(text.Flags));
                                Ar.Add((byte)text.HistoryType);
                                switch (text.TextHistory)
                                {
                                    case FTextHistory.Base Base:
                                        Ar.AddRange(BitConverter.GetBytes(Base.Namespace.Length + 1));
                                        Ar.AddRange(Encoding.UTF8.GetBytes(Base.Namespace + "\0"));
                    
                                        Ar.AddRange(BitConverter.GetBytes(Base.Key.Length + 1));
                                        Ar.AddRange(Encoding.UTF8.GetBytes(Base.Key + "\0"));
                    
                                        Ar.AddRange(BitConverter.GetBytes(Base.SourceString.Length + 1));
                                        Ar.AddRange(Encoding.UTF8.GetBytes(Base.SourceString + "\0"));
                                        break;
                                }
                                break;
                            case long l:
                                Ar.AddRange(BitConverter.GetBytes(l));
                                break;
                            case ulong ul:
                                Ar.AddRange(BitConverter.GetBytes(ul));
                                break;
                            case double d:
                                Ar.AddRange(BitConverter.GetBytes(d));
                                break;
                            case float f:
                                Ar.AddRange(BitConverter.GetBytes(f));
                                break;
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException("TextProperty: " + Value.HistoryType + " is not implemented yet!");
                    break;
            }
        }
    }
    
    public class TextPropertyConverter : JsonConverter<TextProperty>
    {
        public override void WriteJson(JsonWriter writer, TextProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override TextProperty ReadJson(JsonReader reader, Type objectType, TextProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}