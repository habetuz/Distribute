using System.Diagnostics.CodeAnalysis;
using Distribute.FS;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Spectre.Console;
using Spectre.Console.Cli;
using Directory = System.IO.Directory;

namespace Distribute;

public class DistributeCommand : Command<Options>
{
  public override int Execute([NotNull] CommandContext context, [NotNull] Options options)
  {
    AnsiConsole.Foreground = Color.Green;

    Queue<(string Path, DateTime Date)> images = new();

    FileSystem fsFrom = FileSystemFactory.From(options.From);
    FileSystem fsTo = FileSystemFactory.From(options.To);

    // Indexing images and load metadata
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
      .AutoClear(true)
      .HideCompleted(true)
      .Start(ctx => images = new(GetMediaPaths(fsFrom, options.From, 0, options.Depth, ctx)));

    AnsiConsole.Write(
      new Rule($"Metadata loaded. [yellow]{images.Count}[/] files to distribute!")
      {
        Style = Style.Parse("green"),
      }
    );

    if (images.Count == 0)
    {
      AnsiConsole.MarkupLine("No images found. Finished!");
      return 0;
    }

    // Start copy images
    var duplicates = CopyImages(images, fsFrom, fsTo, options.To, options.Structure);

    if (duplicates.Count > 0)
    {
      // Ask what should happen with duplicates.
      AnsiConsole.Write(
        new Rule($"[green]There are [yellow]{duplicates.Count}[/] duplicate files![/]")
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
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new RemainingTimeColumn(),
            new ElapsedTimeColumn(),
          ]
        )
        .Start(ctx =>
        {
          var task = ctx.AddTask("Removing files...", maxValue: images.Count);

          do
          {
            var path = images.Dequeue().Path;

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
          } while (images.Count > 0);
        });
    }

    AnsiConsole.Write(
      new Rule("[blue]All jobs done successfully![/]") { Style = Style.Parse("blue") }
    );

    return 0;
  }

  private static Queue<(string, DateTime)> CopyImages(
    Queue<(string Path, DateTime Date)> images,
    FileSystem fsFrom,
    FileSystem fsTo,
    string to,
    string structure,
    bool overwrite = false,
    string filenameAppend = ""
  )
  {
    var duplicates = new Queue<(string, DateTime)>();

    AnsiConsole
      .Progress()
      .Columns(
        [
          new TaskDescriptionColumn(),
          new ProgressBarColumn(),
          new PercentageColumn(),
          new RemainingTimeColumn(),
          new ElapsedTimeColumn(),
        ]
      )
      .Start(ctx =>
      {
        var task = ctx.AddTask("Copying files... ", maxValue: images.Count);

        do
        {
          var image = images.Dequeue();
          var directory = to + Path.DirectorySeparatorChar + image.Date.ToString(structure);
          var path =
            directory
            + Path.GetFileNameWithoutExtension(image.Path)
            + filenameAppend
            + Path.GetExtension(image.Path);

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
              duplicates.Enqueue(image);
            }
          }

          try
          {
            using var streamTo = fsTo.OpenWrite(path);
            using var streamFrom = fsFrom.OpenRead(image.Path);

            streamFrom.CopyTo(streamTo);
          }
          catch (IOException)
          {
            duplicates.Enqueue(image);
          }
          catch (UnauthorizedAccessException)
          {
            AnsiConsole.MarkupLine(
              $"[yellow]Missing permission copy image:[/] [gray]{image.Path}[/][white] to [/][gray]{path}[/]"
            );
          }

          task.Increment(1);
        } while (images.Count > 0);
      });

    return duplicates;
  }

  private static List<(string, DateTime)> GetMediaPaths(
    FileSystem fileSystem,
    string directory,
    uint depth,
    int maxDepth,
    ProgressContext ctx
  )
  {
    string[] files = fileSystem.GetFiles(directory);

    var task = ctx.AddTask(
      $"[gray][white]Loading images...         [/] {directory}[/]",
      maxValue: files.Length + 1
    );

    List<(string, DateTime)> images = new(files.Length);

    foreach (string file in files)
    {
      ExifSubIfdDirectory? metadata = null;

      try
      {
        metadata = ImageMetadataReader
          .ReadMetadata(fileSystem.OpenRead(file))
          .OfType<ExifSubIfdDirectory>()
          .FirstOrDefault()!;
      }
      catch (ImageProcessingException)
      {
        AnsiConsole.MarkupLine($"[yellow]Could not process file:[/] [gray]{file}[/]");
      }

      task.Increment(1);

      if (metadata == null)
      {
        continue;
      }

      if (metadata.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime datetime))
      {
        images.Add((file, datetime));
      } else {
        AnsiConsole.MarkupLine($"[yellow]No date time data for file:[/] [gray]{file}[/]");
      }
    }

    var subdirectories = fileSystem.GetDirectories(directory);

    if (subdirectories.Length == 0)
    {
      task.Value = task.MaxValue;
      return images;
    }

    task.Description = $"[gray][white]Loading sub directories...[/] {directory}[/]";
    task.Value = 0;
    task.MaxValue = subdirectories.Length;

    if (depth < maxDepth)
    {
      foreach (string subdirectory in subdirectories)
      {
        images.AddRange(GetMediaPaths(fileSystem, subdirectory, depth + 1, maxDepth, ctx));

        task.Increment(1);
      }
    }

    return images;
  }
}
