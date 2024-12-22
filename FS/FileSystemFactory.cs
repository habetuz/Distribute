using System.Runtime.InteropServices;

namespace Distribute.FS;

public static class FileSystemFactory
{
  public static FileSystem From(string path)
  {
    FileSystem? fileSystem = null;

    if (!path.Contains(':'))
    {
      path = Path.GetFullPath(path);
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      var drive = path.Split(':')[0];

      if (drive.Length > 1)
      {
        // MTP or FTP
        fileSystem = new MediaDevice(drive);
      }
    }

    return fileSystem ?? new Normal();
  }
}
