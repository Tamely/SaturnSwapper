using Saturn.Backend.Core.Enums;
using System.Collections.Generic;

namespace Saturn.Backend.Core.Models.Items
{
    public class MeshDefaultModel
    {
        public string HeadSkinColor { get; set; }
        public Dictionary<int, string> HeadMaterials { get; set; }
        public string HeadFX { get; set; }
        public string HeadPartModifierBP { get; set; }
        public string HeadHairColor { get; set; }
        public string HeadMesh { get; set; }
        public string HeadABP { get; set; }
        
        public string BodyABP { get; set; }
        public string BodyFX { get; set; }
        public string BodyPartModifierBP { get; set; }
        public string BodyMesh { get; set; }
        public Dictionary<int, string> BodyMaterials { get; set; }
        public string BodySkeleton { get; set; }

        public string FaceACCFX { get; set; }
        public string FaceACCPartModifierBP { get; set; }
        public string FaceACCABP { get; set; }
        public string FaceACCMesh { get; set; }
        public Dictionary<int, string> FaceACCMaterials { get; set; }
        public ECustomHatType HatType { get; set; }
    }
}