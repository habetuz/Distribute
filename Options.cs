namespace Distribute
{
  using System.ComponentModel;
  using Spectre.Console;
  using Spectre.Console.Cli;

  public class Options : CommandSettings
  {
    public Options(string from, string to, string structure, int depth, bool remove)
    {
      From = from == "The current directory." ? Directory.GetCurrentDirectory() : from;
      To = to == "The current directory." ? Directory.GetCurrentDirectory() : to;
      Structure = structure ?? @"yyyy\\MM\\";
      Depth = depth;
      Remove = remove;
    }

    [Description("The source directory the files should be distributed from.")]
    [CommandOption("-f|--from")]
    [DefaultValue("The current directory.")]
    public string From { get; private set; }

    [Description("The directory the files should be distributed to.")]
    [CommandOption("-t|--to")]
    [DefaultValue("The current directory.")]
    public string To { get; private set; }

    [Description(
      "The folder structure the files should be sorted into.\nSee https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings for more information."
    )]
    [CommandOption("-s|--structure")]
    [DefaultValue(@"yyyy\\MM\\")]
    public string Structure { get; }

    [Description("The maximum search depth for files in the source directory.")]
    [CommandOption("-d|--depth")]
    [DefaultValue(5)]
    public int Depth { get; } = 5;

    [Description(
      "Whether the distributed and copied files should be deleted in the source directory."
    )]
    [CommandOption("-r|--remove")]
    public bool Remove { get; }

    public override ValidationResult Validate()
    {
      if (Depth < 1)
      {
        return ValidationResult.Error("Option [-d|--depth] must be at least 1.");
      }

      if (
        From == Directory.GetCurrentDirectory()
        && To == Directory.GetCurrentDirectory()
      )
      {
        return ValidationResult.Error(
          "Either option [-f|--from] or [-t|--to] has to be specified."
        );
      }

      try
      {
        From = Path.GetFullPath(From);
      }
      catch
      {
        return ValidationResult.Error("The option [-f|--from] is an invalid path.");
      }

      try
      {
        To = Path.GetFullPath(To);
      }
      catch
      {
        return ValidationResult.Error("The option [-t|--to] is an invalid path.");
      }

      return ValidationResult.Success();
    }
  }
}
