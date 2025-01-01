import Saturn.Asset.DependencyBundleEntry;

import Saturn.Readers.FArchive;

FArchive& operator<<(FArchive& Ar, FDependencyBundleEntry& DependencyBundleEntry) {
	Ar << DependencyBundleEntry.LocalImportOrExportIndex;

	return Ar;
}