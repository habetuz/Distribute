namespace Distribute.FS;

public abstract class FileSystem : IDisposable
{
  public abstract string[] GetFiles(string path);
  public abstract IEnumerable<string> EnumerateFiles(string path);
  public abstract string[] GetDirectories(string path);
  public abstract IEnumerable<string> EnumerateDirectories(string path);
  public abstract Stream OpenRead(string path);
  public abstract void OpenWrite(string path, Stream stream);
  public abstract bool PathExists(string path);
  public abstract void CreateDirectory(string path);
  public abstract void DeleteFile(string path);

  public abstract void Dispose();
}
