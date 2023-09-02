using System.IO.Compression;
using UnityEngine;

namespace PatchManager.Core.Cache;

/// <summary>
/// Wrapper for <see cref="ZipArchive"/> for use with the caching system.
/// </summary>
public class Archive : IDisposable
{
    private readonly string _path;
    private readonly MemoryStream _stream;
    private bool _isDisposed;

    /// <summary>
    /// Loads an archive from the given path into memory.
    /// </summary>
    /// <param name="path">Path to the archive file.</param>
    /// <param name="createNew">Whether to create a new archive if it does not exist.</param>
    public Archive(string path, bool createNew = false)
    {
        _path = path;
        _stream = new MemoryStream();

        if (!createNew)
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            fileStream.CopyTo(_stream);
        }

        _stream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Disposes the archive object.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _stream.Dispose();
        _isDisposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Reads the content of the given file in the archive.
    /// </summary>
    /// <param name="filePath">Path of the file relative to the root of the archive.</param>
    /// <returns>The full text of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist in the archive.</exception>
    public string ReadFile(string filePath)
    {
        AssertNotDisposed();

        using var archive = new ZipArchive(_stream, ZipArchiveMode.Read, true);
        var entry = archive.GetEntry(filePath);
        if (entry == null)
        {
            throw new FileNotFoundException($"File {filePath} does not exist in archive {_path}!");
        }

        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Adds a file to the archive.
    /// </summary>
    /// <param name="filePath">Path of the file relative to the root of the archive.</param>
    /// <param name="content">Content of the file.</param>
    /// <exception cref="ArgumentException">Thrown when the file already exists in the archive.</exception>
    public void AddFile(string filePath, string content)
    {
        AssertNotDisposed();

        using var archive = new ZipArchive(_stream, ZipArchiveMode.Update, true);

        if (archive.GetEntry(filePath) != null)
        {
            throw new ArgumentException($"File {filePath} already exists in archive {_path}!");
        }

        var entry = archive.CreateEntry(filePath);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream);
        writer.Write(content);
    }

    /// <summary>
    /// Saves the archive to disk.
    /// </summary>
    public void Save()
    {
        AssertNotDisposed();
        using var fileStream = new FileStream(_path, FileMode.Create);
        _stream.Seek(0, SeekOrigin.Begin);
        _stream.CopyTo(fileStream);
    }

    private void AssertNotDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(Archive), "The Archive object has been disposed.");
        }
    }
}