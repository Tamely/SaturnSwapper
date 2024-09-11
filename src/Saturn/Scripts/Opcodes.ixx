export module Saturn.Language.Opcodes;

export enum class EOpcodes {
	NONE,
	EXECUTECONSOLECOMMAND,
	PRINT,
	GETLOCALPLAYER,
	CREATEMATERIALINSTANCE,
	GETPAWNFROMPLAYER,
	MATERIALSETTEXTUREPARAMETER,
	GETTEXTUREBYURL,
	PAWNGETPART,
	PARTSETMATERIAL,
	GETTEXTUREBYPATH,
	GETMESHBYPATH,
	PARTSETMESH,
	GETABPBYPATH,
	PARTSETABP,
	PARTHIDE,
	PARTSHOW,
	PAWNSETMASTERMESH,
	PAWNSETGENDER,
	PAWNSETBODYTYPE,
	GETMONTAGEBYPATH,
	PAWNPLAYMONTAGE,
	PAWNSTOPMONTAGE,
	CLEARMAPS,
	PAWNHIDEALLPARTS,
	MATERIALSETVEC4PARAMETER,
	MATERIALSETSCALARPARAMETER,
	MATERIALSETSTRINGPARAMETER,
	MATERIALSETINTPARAMETER,
	MATERIALSETBOOLPARAMETER,
	GETPARTBYPATH,
	PAWNADDPART
};