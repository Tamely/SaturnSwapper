import Saturn.Functions.PartSetMesh;

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
import Saturn.Functions.PawnGetPart;

void FPartSetMesh::encode(int part, int mesh) {
	std::string message = std::to_string((int)EOpcodes::PARTSETMESH) + ",,,,," + std::to_string(part) + ",,,,," + std::to_string(mesh);
	WindowsFunctionLibrary::StringToImage(WindowsFunctionLibrary::ws2s(FortniteFunctionLibrary::GetFortniteLocalPath() + L"instruction.png"), message);
}

// UPartSetMesh(Part, Mesh);
duk_ret_t FPartSetMesh::dukPartSetMesh(duk_context* ctx) {
	int ArgsLength = duk_get_top(ctx);
	if (ArgsLength != 2) {
		MessageBoxW(nullptr, L"This function takes 2 arguments!", L"PartSetMesh", NULL);
		return DUK_RET_TYPE_ERROR;
	}


	int64_t part = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 0));
	int64_t mesh = reinterpret_cast<int64_t>(duk_get_pointer(ctx, 1));

	if (part % FPawnGetPart::COMPONENT_SIG != 0 || part == 0) {
		MessageBoxW(nullptr, L"Part pointer invalid!", L"PartSetMesh", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	if (mesh % FFindMeshByPath::MESH_SIG != 0 || mesh == 0) {
		MessageBoxW(nullptr, L"Mesh pointer invalid!", L"PartSetMesh", NULL);
		return DUK_RET_TYPE_ERROR;
	}

	FContext::ResponseWaiting = true;
	encode(part / FPawnGetPart::COMPONENT_SIG, mesh / FFindMeshByPath::MESH_SIG);
	while (FContext::ResponseWaiting);

	return 0;
}