import Saturn.Functions.PawnSetMasterMesh;

import Saturn.Functions.PlayerGetPawn;
import Saturn.Functions.FindMeshByPath;

import Saturn.FortniteFunctionLibrary;
import Saturn.WindowsFunctionLibrary;
import Saturn.Language.Opcodes;
import Saturn.Context;

import <duktape/duktape.h>;

import <Windows.h>;
import <string>;
import <vector>;

import <iostream>;

void FPawnSetMasterMesh::encode(int pawn, int mesh) {
	std::string message = std::to_string((int)EOpcodes::PAWNSETMASTERMESH) + ",,,,," + std::to_string(pawn) + ",,,,," + std::to_string(mesh);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPawnSetMasterMesh(Pawn, Mesh);
duk_ret_t FPawnSetMasterMesh::dukPawnSetMasterMesh(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 2) {
		MessageBoxW(nullptr, L"This function takes 2 arguments!", L"PawnSetMasterMesh", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	int64_t pawn = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	int64_t mesh = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 1));

	if (pawn % FPlayerGetPawn::PAWN_SIG != 0 || pawn == 0) {
		MessageBoxW(nullptr, L"Pawn pointer invalid!", L"PawnSetMasterMesh", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	if (mesh % FFindMeshByPath::MESH_SIG != 0 || mesh == 0) {
		MessageBoxW(nullptr, L"Mesh pointer invalid!", L"PawnSetMasterMesh", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(pawn / FPlayerGetPawn::PAWN_SIG, mesh / FFindMeshByPath::MESH_SIG);
	while (FContext::ResponseWaiting);

	return 0;
}