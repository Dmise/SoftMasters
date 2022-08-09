namespace WebApp.Utilities
{
    public static class PathCreator
    {
        public static string Canonicalize(this string path)
        {
            if (path.IsAbsolutePath())
                return Path.GetFullPath(path);
            var fakeRoot = Environment.CurrentDirectory; // Gives us a cross platform full path
            var combined = Path.Combine(fakeRoot, path);
            combined = Path.GetFullPath(combined);
            return combined.RelativeTo(fakeRoot);
        }
        private static bool IsAbsolutePath(this string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return
                Path.IsPathRooted(path)
                && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                && !Path.GetPathRoot(path).Equals(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        }
        private static string RelativeTo(this string filespec, string folder)
        {
            var pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString())) folder += Path.DirectorySeparatorChar;
            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString()
                .Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
