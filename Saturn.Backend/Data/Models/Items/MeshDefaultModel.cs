using Saturn.Backend.Data.Enums;
using System.Collections.Generic;

namespace Saturn.Backend.Data.Models.Items
{
    public struct MeshDefaultModel
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

        public MeshDefaultModel(MeshDefaultModel model = new MeshDefaultModel())
        {
            if (Equals(model, new MeshDefaultModel()))
            {
                HeadSkinColor = "";
                HeadMaterials = new Dictionary<int, string>();
                HeadFX = "";
                HeadPartModifierBP = "";
                HeadHairColor = "";
                HeadMesh = "";
                HeadABP = "";

                BodyABP = "";
                BodyFX = "";
                BodyPartModifierBP = "";
                BodyMesh = "";
                BodyMaterials = new Dictionary<int, string>();
                BodySkeleton = "";

                FaceACCFX = "";
                FaceACCPartModifierBP = "";
                FaceACCABP = "";
                FaceACCMesh = "";
                FaceACCMaterials = new Dictionary<int, string>();
                HatType = ECustomHatType.ECustomHatType_None;
            }
            else
            {
                HeadSkinColor = model.HeadSkinColor;
                HeadMaterials = model.HeadMaterials;
                HeadFX = model.HeadFX;
                HeadPartModifierBP = model.HeadPartModifierBP;
                HeadHairColor = model.HeadHairColor;
                HeadMesh = model.HeadMesh;
                HeadABP = model.HeadABP;

                BodyABP = model.BodyABP;
                BodyFX = model.BodyFX;
                BodyPartModifierBP = model.BodyPartModifierBP;
                BodyMesh = model.BodyMesh;
                BodyMaterials = model.BodyMaterials;
                BodySkeleton = model.BodySkeleton;

                FaceACCFX = model.FaceACCFX;
                FaceACCPartModifierBP = model.FaceACCPartModifierBP;
                FaceACCABP = model.FaceACCABP;
                FaceACCMesh = model.FaceACCMesh;
                FaceACCMaterials = model.FaceACCMaterials;
                HatType = model.HatType;
            }
        }
    }
}