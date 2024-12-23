#pragma warning disable CA1416 // Validate platform compatibility

using Spectre.Console;

namespace Distribute.FS;

public class MediaDevice : FileSystem
{
  private static Dictionary<string, MediaDevices.MediaDevice> Devices { get; } = [];

  private string DeviceName { get; init; }
  private MediaDevices.MediaDevice Device { get; init; }

  public MediaDevice(string deviceName)
  {
    DeviceName = deviceName;

    if (Devices.TryGetValue(deviceName, out var device))
    {
      Device = device;
      return;
    }

    var devices = MediaDevices.MediaDevice.GetDevices();

    if (!devices.Any((device) => device.FriendlyName == deviceName))
    {
      throw new Exception(
        $"Device {deviceName} is not connected. Connect the device and try again."
      );
    }

    Device = devices.First((device) => device.FriendlyName == deviceName);
    Devices[deviceName] = Device;
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
    lock (Device)
    {
      return Device.GetDirectories(TrimDeviceName(path));
    }
  }

  public override string[] GetFiles(string path)
  {
    lock (Device)
    {
      return Device.GetFiles(TrimDeviceName(path));
    }
  }

  public override bool PathExists(string path)
  {
    lock (Device)
    {
      return Device.FileExists(TrimDeviceName(path))
        || Device.DirectoryExists(TrimDeviceName(path));
    }
  }

  public override Stream OpenRead(string path)
  {
    lock (Device)
    {
      var stream = new MemoryStream();
      Device.DownloadFile(TrimDeviceName(path), stream);
      stream.Position = 0;
      return stream;
    }
  }

  public override void OpenWrite(string path, Stream stream)
  {
    lock (Device)
    {
      Device.UploadFile(stream, TrimDeviceName(path));
      stream.Close();
    }
  }

  public override void CreateDirectory(string path)
  {
    lock (Device)
    {
      Device.CreateDirectory(TrimDeviceName(path));
    }
  }

  public override void DeleteFile(string path)
  {
    lock (Device)
    {
      Device.DeleteFile(TrimDeviceName(path));
    }
  }

  public override IEnumerable<string> EnumerateFiles(string path)
  {
    lock (Device)
    {
      return Device.EnumerateFiles(TrimDeviceName(path));
    }
  }

  public override IEnumerable<string> EnumerateDirectories(string path)
  {
    lock (Device)
    {
      return Device.EnumerateDirectories(TrimDeviceName(path));
    }
  }

  public override void Dispose()
  {
    GC.SuppressFinalize(this);
    Device.Disconnect();
    Device.Dispose();
  }
}
#pragma warning restore CA1416 // Validate platform compatibility
