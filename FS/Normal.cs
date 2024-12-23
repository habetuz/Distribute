namespace Distribute.FS;

public class Normal : FileSystem
{
  public override string[] GetDirectories(string path)
  {
    return Directory.GetDirectories(path);
  }

  public override string[] GetFiles(string path)
  {
    return Directory.GetFiles(path);
  }

  public override bool PathExists(string path)
  {
    return Path.Exists(path);
  }

  public override Stream OpenRead(string path)
  {
    return File.OpenRead(path);
  }

  public override Stream OpenWrite(string path)
  {
    return File.OpenWrite(path);
  }

  public override void CreateDirectory(string path)
  {
    Directory.CreateDirectory(path);
  }

  public override void DeleteFile(string path)
  {
    File.Delete(path);
  }

  public override IEnumerable<string> EnumerateFiles(string path)
  {
    return Directory.EnumerateFiles(path);
  }

  public override IEnumerable<string> EnumerateDirectories(string path)
  {
    return Directory.EnumerateDirectories(path);
  }

  public override void Dispose()
  {
    GC.SuppressFinalize(this);
  }
}
