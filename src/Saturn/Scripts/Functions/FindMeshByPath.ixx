export module Saturn.Functions.FindMeshByPath;

import <duktape/duktape.h>;
import <string>;

export struct FFindMeshByPath {
	const static int MESH_SIG = 0xAAAB;
	static int MeshCount;

	static void encode(const std::string& path);
	// USkeletalMesh UFindMeshByPath("Path");
	static duk_ret_t dukFindMeshByPath(duk_context* ctx);
};