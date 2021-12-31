using Saturn.Backend.Data.Enums;

namespace Saturn.Backend.Data.Models.Items
{
    public struct MeshDefaultModel
    {
        public string HeadSkinColor { get; set; }
        public string HeadMaterial { get; set; }
        public string HairMaterial { get; set; }
        public string HeadFX { get; set; }
        public string HeadPartModifierBP { get; set; }
        public string HeadHairColor { get; set; }
        public string HeadMesh { get; set; }
        public string HeadABP { get; set; }
        
        public string BodyABP { get; set; }
        public string BodyFX { get; set; }
        public string BodyPartModifierBP { get; set; }
        public string BodyMesh { get; set; }
        public string BodyMaterial { get; set; }
        public string BodySkeleton { get; set; }

        public string FaceACCFX { get; set; }
        public string FaceACCPartModifierBP { get; set; }
        public string FaceACCABP { get; set; }
        public string FaceACCMesh { get; set; }
        public string FaceACCMaterial { get; set; }
        public string FaceACCMaterial2 { get; set; }
        public string FaceACCMaterial3 { get; set; }
        public ECustomHatType HatType { get; set; }
    }
}