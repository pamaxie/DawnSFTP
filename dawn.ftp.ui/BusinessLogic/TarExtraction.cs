using System.Formats.Tar;
using System.IO;
using System.IO.Compression;

namespace dawn.ftp.ui.BusinessLogic;

public class TarExtraction {
    /// <summary>
    /// Creates a <i>.tar.gz</i> archive from a directory.
    /// </summary>
    /// <param name="sourceDirectoryName">The path to the directory to be archived.</param>
    /// <param name="destinationFileName">The path of the archive file to be created.</param>
    /// <param name="includeBaseDirectory">True to include the base directory in the archive; otherwise, false.</param>
    public static void CreateTarGz(string sourceDirectoryName, string destinationFileName, bool includeBaseDirectory = false) {
        using var fs = File.Create(destinationFileName);
        using var gzip = new GZipStream(fs, CompressionMode.Compress);
        TarFile.CreateFromDirectory(sourceDirectoryName, gzip, includeBaseDirectory);
    }

    /// <summary>
    /// Creates a <i>.tar</i> archive from a directory.
    /// </summary>
    /// <param name="sourceDirectoryName">The path to the directory to be archived.</param>
    /// <param name="destinationFileName">The path of the archive file to be created.</param>
    /// <param name="includeBaseDirectory">True to include the base directory in the archive; otherwise, false.</param>
    public static void CreateTar(string sourceDirectoryName, string destinationFileName, bool includeBaseDirectory = false) {
        TarFile.CreateFromDirectory(sourceDirectoryName, destinationFileName, includeBaseDirectory);
    }

    /// <summary>
    /// Extracts a <i>.tar.gz</i> archive to the specified directory.
    /// </summary>
    /// <param name="filename">The <i>.tar.gz</i> to decompress and extract.</param>
    /// <param name="outputDir">Output directory to write the files.</param>
    /// <param name="overwriteFiles">True to overwrite existing files; otherwise, false.</param>
    public static void ExtractTarGz(string filename, string outputDir, bool overwriteFiles = false) {
        using var stream = File.OpenRead(filename);
        ExtractTarGz(stream, outputDir, overwriteFiles);
    }

    /// <summary>
    /// Extracts a <i>.tar.gz</i> archive stream to the specified directory.
    /// </summary>
    /// <param name="stream">The <i>.tar.gz</i> to decompress and extract.</param>
    /// <param name="outputDir">Output directory to write the files.</param>
    /// <param name="overwriteFiles">True to overwrite existing files; otherwise, false.</param>
    public static void ExtractTarGz(Stream stream, string outputDir, bool overwriteFiles = false) {
        using var gzip = new GZipStream(stream, CompressionMode.Decompress);
        TarFile.ExtractToDirectory(gzip, outputDir, overwriteFiles);
    }

    /// <summary>
    /// Extracts a <c>tar</c> archive to the specified directory.
    /// </summary>
    /// <param name="filename">The <i>.tar</i> to extract.</param>
    /// <param name="outputDir">Output directory to write the files.</param>
    /// <param name="overwriteFiles">True to overwrite existing files; otherwise, false.</param>
    public static void ExtractTar(string filename, string outputDir, bool overwriteFiles = false) {
        TarFile.ExtractToDirectory(filename, outputDir, overwriteFiles);
    }

    /// <summary>
    /// Extracts a <c>tar</c> archive to the specified directory.
    /// </summary>
    /// <param name="stream">The <i>.tar</i> to extract.</param>
    /// <param name="outputDir">Output directory to write the files.</param>
    /// <param name="overwriteFiles">True to overwrite existing files; otherwise, false.</param>
    public static void ExtractTar(Stream stream, string outputDir, bool overwriteFiles = false) {
        TarFile.ExtractToDirectory(stream, outputDir, overwriteFiles);
    }
}