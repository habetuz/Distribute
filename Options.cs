using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Distribute;

public class Options(string from, string to, string structure, int depth, bool remove)
  : CommandSettings
{
  [Description("The source directory the files should be distributed from.")]
  [CommandArgument(0, "[From]")]
  [DefaultValue("./")]
  public string From { get; private set; } = from;

  [Description("The directory the files should be distributed to.")]
  [CommandArgument(0, "[To]")]
  [DefaultValue("./")]
  public string To { get; private set; } = to;

  [Description(
    "The folder structure the files should be sorted into.\nSee https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings for more information."
  )]
  [CommandOption("-s|--structure")]
  [DefaultValue("yyyy/MM/")]
  public string Structure { get; private set; } = structure;

  [Description("The maximum search depth for files in the source directory.")]
  [CommandOption("-d|--depth")]
  [DefaultValue(5)]
  public int Depth { get; private set; } = depth;

  [Description(
    "Whether the distributed and copied files should be deleted in the source directory."
  )]
  [CommandOption("-r|--remove")]
  public bool Remove { get; private set; } = remove;

  public override ValidationResult Validate()
  {
    if (Depth < 1)
    {
      return ValidationResult.Error("Option [-d|--depth] must be at least 1.");
    }

    if (From == Directory.GetCurrentDirectory() && To == Directory.GetCurrentDirectory())
    {
      return ValidationResult.Error("Either option [-f|--from] or [-t|--to] has to be specified.");
    }

    if (!Path.IsPathFullyQualified(From))
    {
      From = Path.GetFullPath(From);
    }

    if (!Path.IsPathFullyQualified(To))
    {
      To = Path.GetFullPath(To);
    }

    return ValidationResult.Success();
  }
}
