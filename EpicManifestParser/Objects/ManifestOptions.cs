using System;
using System.IO;

namespace EpicManifestParser.Objects
{
	public class ManifestOptions
	{
		public Uri ChunkBaseUri { get; set; }
		public DirectoryInfo ChunkCacheDirectory { get; set; }
	}
}