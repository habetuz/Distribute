using System.Diagnostics.CodeAnalysis;
using Distribute.FS;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Distribute;

public class DistributeCommand : Command<Options>
{
  const string VERSION = "2.0.0";

  public override int Execute(CommandContext context, Options options)
  {
    AnsiConsole.Foreground = Color.Green;

    AnsiConsole.Write(new FigletText("Distribute").LeftJustified().Color(Color.Green));
    AnsiConsole.WriteLine("Distribute images by date taken.");
    AnsiConsole.MarkupLine($"[yellow]Version:[/] [gray]{VERSION}[/]");
    AnsiConsole.WriteLine();

    FileSystem fsFrom = FileSystemFactory.From(options.From);
    FileSystem fsTo = FileSystemFactory.From(options.To);

    AnsiConsole.Write(new Rule($"[green]Loading image paths[/]") { Style = Style.Parse("green"), });

    var files = AnsiConsole
      .Status()
      .Start(
        "Loading file names...",
        (ctx) => EnumerateFilePaths(fsFrom, options.From, 1, options.Depth, ctx)
      );

    AnsiConsole.Write(
      new Rule($"Files loaded. [yellow]{files.Count()}[/] files to distribute!")
      {
        Style = Style.Parse("green"),
      }
    );

    if (!files.Any())
    {
      AnsiConsole.MarkupLine("No images found. Finished!");
      return 0;
    }

    // Start copy images
    var duplicates = CopyImages(files, fsFrom, fsTo, options.To, options.Structure);

    if (duplicates.Any())
    {
      // Ask what should happen with duplicates.
      AnsiConsole.Write(
        new Rule($"[green]There are [yellow]{duplicates.Count()}[/] duplicate files![/]")
        {
          Style = Style.Parse("green"),
        }
      );

      AnsiConsole.WriteLine();

      var duplicatesOption = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
          .Title("[yellow]How would you like to proceed with the duplicate files?[/]")
          .AddChoices(["Skip", "Overwrite", "Add \"_copy\" to filename"])
      );

      switch (duplicatesOption)
      {
        case "Skip":
          break;
        case "Overwrite":
          CopyImages(duplicates, fsFrom, fsTo, options.To, options.Structure, true);
          break;
        default:
          CopyImages(duplicates, fsFrom, fsTo, options.To, options.Structure, false, "_copy");
          break;
      }
    }

    // Remove images in source.
    if (options.Remove)
    {
      AnsiConsole.Write(
        new Rule("[yellow]Removing files in source directory![/]") { Style = Style.Parse("yellow") }
      );
      AnsiConsole
        .Progress()
        .Columns(
          [
            new ProgressBarColumn(),
            new PercentageColumn(),
            new RemainingTimeColumn(),
            new ElapsedTimeColumn(),
            new TaskDescriptionColumn() { Alignment = Justify.Left },
          ]
        )
        .Start(ctx =>
        {
          var task = ctx.AddTask("", maxValue: files.Count());

          files
            .AsParallel()
            .ForAll(
              (path) =>
              {
                task.Description = $"[gray]{path}[/]";

                try
                {
                  fsFrom.DeleteFile(path);
                }
                catch (IOException)
                {
                  AnsiConsole.MarkupLine($"[yellow]Image is in use:[/] [gray]{path}[/]");
                }
                catch (UnauthorizedAccessException)
                {
                  AnsiConsole.MarkupLine(
                    $"[yellow]Missing permission to delete image:[/] [gray]{path}[/]"
                  );
                }

                task.Increment(1);
              }
            );
        });
    }

    AnsiConsole.Write(
      new Rule("[blue]All jobs done successfully![/]") { Style = Style.Parse("blue") }
    );

    return 0;
  }

  private static IEnumerable<string> CopyImages(
    IEnumerable<string> files,
    FileSystem fsFrom,
    FileSystem fsTo,
    string to,
    string structure,
    bool overwrite = false,
    string filenameAppend = ""
  )
  {
    return AnsiConsole
      .Progress()
      .Columns(
        [
          new ProgressBarColumn(),
          new PercentageColumn(),
          new RemainingTimeColumn(),
          new ElapsedTimeColumn(),
          new TaskDescriptionColumn(),
        ]
      )
      .Start<IEnumerable<string>>(ctx =>
      {
        var task = ctx.AddTask("Copying files... ", maxValue: files.Count());

        return files
          .AsParallel()
          .Select<string, (string Path, DateTime Date, Stream Stream)?>(
            (file) =>
            {
              task.Description = $"[gray]{file}[/]";
              var readStream = fsFrom.OpenRead(file);

              try
              {
                ExifSubIfdDirectory? metadata = ImageMetadataReader
                  .ReadMetadata(readStream)
                  .OfType<ExifSubIfdDirectory>()
                  .FirstOrDefault();

                if (
                  metadata is not null
                  && metadata.TryGetDateTime(
                    ExifDirectoryBase.TagDateTimeOriginal,
                    out DateTime datetime
                  )
                )
                {
                  return (file, datetime, readStream);
                }
                else
                {
                  AnsiConsole.MarkupLine($"[yellow]No date time data for file:[/] [gray]{file}[/]");
                }
              }
              catch (ImageProcessingException e)
              {
                AnsiConsole.MarkupLine($"[yellow]Could not process file:[/] [gray]{file}[/]");
                AnsiConsole.WriteException(e);
              }

              readStream.Close();
              return null;
            }
          )
          .Where(
            (file) =>
            {
              if (file.HasValue)
              {
                file.Value.Stream.Position = 0;
                return true;
              }
              else
              {
                task.Increment(1);
                //progress++;
                return false;
              }
            }
          )
          .Select((file) => file!.Value)
          .Where(
            (file) =>
            {
              var directory = to + Path.DirectorySeparatorChar + file.Date.ToString(structure);
              var path =
                directory
                + Path.DirectorySeparatorChar
                + Path.GetFileNameWithoutExtension(file.Path)
                + filenameAppend
                + Path.GetExtension(file.Path);

              if (!fsTo.PathExists(directory))
              {
                fsTo.CreateDirectory(directory);
              }

              if (fsTo.PathExists(path))
              {
                if (overwrite)
                {
                  fsTo.DeleteFile(path);
                }
                else
                {
                  // Duplicate that cannot be overwritten
                  task.Increment(1);
                  file.Stream.Close();
                  return true;
                }
              }

              try
              {
                fsTo.OpenWrite(path, fsFrom.OpenRead(file.Path));
              }
              catch (UnauthorizedAccessException)
              {
                AnsiConsole.MarkupLine(
                  $"[yellow]Missing permission copy image:[/] [gray]{file.Path}[/][white] to [/][gray]{path}[/]"
                );
              }

              task.Increment(1);
              file.Stream.Close();
              return false;
            }
          )
          .Select(
            (file) =>
            {
              file.Stream.Close();
              return file.Path;
            }
          )
          .ToList();
      });
  }

  private static IEnumerable<string> EnumerateFilePaths(
    FileSystem fileSystem,
    string directory,
    uint depth,
    int maxDepth,
    StatusContext ctx
  )
  {
    ctx.Status(directory);

    var files = fileSystem.EnumerateFiles(directory).ToList();
    var subdirectories = fileSystem.EnumerateDirectories(directory);

    return files.Concat(
      subdirectories.Aggregate(
        Enumerable.Empty<string>(),
        (acc, curr) => acc.Concat(EnumerateFilePaths(fileSystem, curr, depth + 1, maxDepth, ctx))
      )
    );
  }
}
