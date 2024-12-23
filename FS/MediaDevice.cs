#pragma warning disable CA1416 // Validate platform compatibility
using System.IO.Pipes;
using System.Runtime.ConstrainedExecution;
using Spectre.Console;

namespace Distribute.FS;

public class MediaDevice : FileSystem
{
  private string DeviceName { get; init; }
  private MediaDevices.MediaDevice Device { get; init; }

  public MediaDevice(string deviceName)
  {
    DeviceName = deviceName;
    var devices = MediaDevices.MediaDevice.GetDevices();
    Device = devices.First((device) => device.FriendlyName == deviceName);

    if (Device is null)
    {
      throw new Exception(
        $"Device {deviceName} is not connected. Connect the device and try again."
      );
    }

    Device.Connect();
  }

  private string TrimDeviceName(string path)
  {
    if (path.Contains(':'))
      return path[(DeviceName.Length + 1)..];
    else
      return path;
  }

  public override string[] GetDirectories(string path)
  {
    return Device.GetDirectories(TrimDeviceName(path));
  }

  public override string[] GetFiles(string path)
  {
    return Device.GetFiles(TrimDeviceName(path));
  }

  public override bool PathExists(string path)
  {
    return Device.FileExists(TrimDeviceName(path)) || Device.DirectoryExists(TrimDeviceName(path));
  }

  public override Stream OpenRead(string path)
  {
    var stream = new MemoryStream();
    Device.DownloadFile(TrimDeviceName(path), stream);
    stream.Position = 0;
    return stream;
  }

  public override Stream OpenWrite(string path)
  {
    var stream = new MemoryStream();
    Device.UploadFile(stream, TrimDeviceName(path));
    stream.Position = 0;
    return stream;
  }

  public override void CreateDirectory(string path)
  {
    Device.CreateDirectory(TrimDeviceName(path));
  }

  public override void DeleteFile(string path)
  {
    Device.DeleteFile(TrimDeviceName(path));
  }

  public override IEnumerable<string> EnumerateFiles(string path)
  {
    return Device.EnumerateFiles(path);
  }

  public override IEnumerable<string> EnumerateDirectories(string path)
  {
    return Device.EnumerateDirectories(path);
  }

  public override void Dispose()
  {
    GC.SuppressFinalize(this);
    Device.Disconnect();
    Device.Dispose();
  }
}
#pragma warning restore CA1416 // Validate platform compatibility
