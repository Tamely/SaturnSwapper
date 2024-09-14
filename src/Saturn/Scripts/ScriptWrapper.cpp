#include <Crypt/skCrypter.h>
#include <duktape/duktape.h>

import Saturn.Scripts.ScriptWrapper;

import Saturn.Context;
import Saturn.Functions.Print;
import Saturn.Functions.PartHide;
import Saturn.Functions.PartShow;
import Saturn.Functions.WebClient;
import Saturn.Functions.DoNothing;
import Saturn.Functions.ClearMaps;
import Saturn.Functions.PartSetABP;
import Saturn.Functions.PawnAddPart;
import Saturn.Functions.PartSetMesh;
import Saturn.Functions.PawnGetPart;
import Saturn.Functions.WebClientGet;
import Saturn.Functions.WebClientPost;
import Saturn.Functions.PawnSetGender;
import Saturn.Functions.PlayerGetPawn;
import Saturn.Functions.FindABPByPath;
import Saturn.Functions.FindMeshByPath;
import Saturn.Functions.FindPartByPath;
import Saturn.Functions.GetLocalPlayer;
import Saturn.Functions.PawnStopMontage;
import Saturn.Functions.PawnPlayMontage;
import Saturn.Functions.PawnSetBodyType;
import Saturn.Functions.PartSetMaterial;
import Saturn.Functions.FindTextureByURL;
import Saturn.Functions.PawnHideAllParts;
import Saturn.Functions.FindTextureByPath;
import Saturn.Functions.PawnSetMasterMesh;
import Saturn.Functions.FindMontageByPath;
import Saturn.Functions.DownloadUEFNByZip;
import Saturn.Functions.ExecuteConsoleCommand;
import Saturn.Functions.MaterialSetIntParameter;
import Saturn.Functions.MaterialSetBoolParameter;
import Saturn.Functions.MaterialSetFloatParameter;
import Saturn.Functions.MaterialSetStringParameter;
import Saturn.Functions.MaterialSetVector4Parameter;
import Saturn.Functions.CreateDynamicMaterialInstance;
import Saturn.Functions.MaterialSetTextureParameterValue;

import <string>;
import <Windows.h>;

void error_handler(void* udata, const char* msg) {
	(void)udata;

	std::string message = "*** FATAL ERROR: ";
	message.append(msg ? msg : "NO MESSAGE");

	MessageBox(nullptr, message.c_str(), "ERROR", NULL);
}

void FScriptWrapper::InitBindings() {
	duk_context* ctx = duk_create_heap(NULL, NULL, NULL, NULL, error_handler);

	FContext::DukContext = ctx;

	duk_push_c_function(ctx, FExecuteConsoleCommand::dukExecuteConsoleCommand, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UExecuteConsoleCommand"));

	duk_push_c_function(ctx, FPrint::dukPrint, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPrint"));

	duk_push_c_function(ctx, FGetLocalPlayer::dukGetLocalPlayer, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UGetLocalPlayer"));

	duk_push_c_function(ctx, FWebClient::dukWebClient, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UWebClient"));

	duk_push_c_function(ctx, FWebClientPost::dukWebClientPost, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UWebClientPost"));

	duk_push_c_function(ctx, FWebClientGet::dukWebClientGet, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UWebClientGet"));

	duk_push_c_function(ctx, FCreateDynamicMaterialInstance::dukCreateDynamicMaterialInstance, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UCreateDynamicMaterialInstance"));

	duk_push_c_function(ctx, FPlayerGetPawn::dukPlayerGetPawn, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPlayerGetPawn"));

	duk_push_c_function(ctx, FMaterialSetTextureParameter::dukMaterialSetTextureParameter, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UMaterialSetTextureParameter"));

	duk_push_c_function(ctx, FFindTextureByURL::dukFindTextureByURL, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UFindTextureByURL"));

	duk_push_c_function(ctx, FPawnGetPart::dukPawnGetPart, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPawnGetPart"));

	duk_push_c_function(ctx, FPartSetMaterial::dukPartSetMaterial, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPartSetMaterial"));

	duk_push_c_function(ctx, FFindTextureByPath::dukFindTextureByPath, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UFindTextureByPath"));

	duk_push_c_function(ctx, FFindMeshByPath::dukFindMeshByPath, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UFindMeshByPath"));

	duk_push_c_function(ctx, FPartSetMesh::dukPartSetMesh, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPartSetMesh"));

	duk_push_c_function(ctx, FFindABPByPath::dukFindABPByPath, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UFindABPByPath"));

	duk_push_c_function(ctx, FPartSetABP::dukPartSetABP, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPartSetABP"));

	duk_push_c_function(ctx, FPartHide::dukPartHide, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPartHide"));

	duk_push_c_function(ctx, FPartShow::dukPartShow, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPartShow"));

	duk_push_c_function(ctx, FPawnSetMasterMesh::dukPawnSetMasterMesh, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPawnSetMasterMesh"));

	duk_push_c_function(ctx, FPawnSetGender::dukPawnSetGender, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPawnSetGender"));

	duk_push_c_function(ctx, FPawnSetBodyType::dukPawnSetBodyType, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPawnSetBodyType"));

	duk_push_c_function(ctx, FFindMontageByPath::dukFindMontageByPath, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UFindMontageByPath"));

	duk_push_c_function(ctx, FPawnPlayMontage::dukPawnPlayMontage, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPawnPlayMontage"));

	duk_push_c_function(ctx, FPawnStopMontage::dukPawnStopMontage, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPawnStopMontage"));

	duk_push_c_function(ctx, FClearMaps::dukClearMaps, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UClearMaps"));

	duk_push_c_function(ctx, FPawnHideAllParts::dukPawnHideAllParts, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPawnHideAllParts"));

	duk_push_c_function(ctx, FMaterialSetVector4Parameter::dukMaterialSetVector4Parameter, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UMaterialSetVector4Parameter"));

	duk_push_c_function(ctx, FMaterialSetStringParameter::dukMaterialSetStringParameter, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UMaterialSetStringParameter"));

	duk_push_c_function(ctx, FMaterialSetFloatParameter::dukMaterialSetFloatParameter, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UMaterialSetScalarParameter"));

	duk_push_c_function(ctx, FMaterialSetIntParameter::dukMaterialSetIntParameter, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UMaterialSetIntParameter"));

	duk_push_c_function(ctx, FMaterialSetBoolParameter::dukMaterialSetBoolParameter, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UMaterialSetBoolParameter"));

	duk_push_c_function(ctx, FFindPartByPath::dukFindPartByPath, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UFindPartByPath"));

	duk_push_c_function(ctx, FPawnAddPart::dukPawnAddPart, DUK_VARARGS);
	duk_put_global_string(ctx, skCrypt("UPawnAddPart"));

	// Fix UPawnHideAllParts
	// UPawnGetCurrentWeapon
	// UPawnGetCurrentPickaxe
	// UWeaponSetMesh
	// UWeaponSetMaterial
	// UWeaponSetABP
	// UGetPickaxeByPath
	// UPickaxeSetData

	FDoNothing::encode();
}

void FScriptWrapper::Eval(const std::string& code) {
	FFindABPByPath::ABPCount = 0;
	FFindMeshByPath::MeshCount = 0;
	FFindPartByPath::PartCount = 0;
	FGetLocalPlayer::PlayerCount = 0;
	FPawnGetPart::ComponentCount = 0;
	FPlayerGetPawn::PlayerPawnCount = 0;
	FFindTextureByURL::TextureCount = 0;
	FCreateDynamicMaterialInstance::MaterialInstanceCount = 0;

	duk_eval_string_noresult(FContext::DukContext, skCrypt("UClearMaps();"));
	duk_eval_string_noresult(FContext::DukContext, code.c_str());
}
