namespace Distribute
{
    using System.IO;
    using System.Web;
    using System.Collections.Concurrent;
    using CommandLine;
    using Spectre.Console;
    using System.Collections.Generic;
    using System;
    using MetadataExtractor;
    using Directory = System.IO.Directory;
    using MetadataDirectory = MetadataExtractor.Directory;
    using System.Linq;
    using MetadataExtractor.Formats.Exif;
    using System.Globalization;
using XmpCore.Options;

    internal class Program
    {
        static void Main(string[] args)
        {
            var options = CommandLine.Parser.Default.ParseArguments<Options>(args).Value;

            AnsiConsole.Foreground = Spectre.Console.Color.Green;

            // Set missing "from" or "to" option
            if (options.From == null)
            {
                options.From = Directory.GetCurrentDirectory();
            }
            else if (options.To == null)
            {
                options.To = Directory.GetCurrentDirectory();
            }

            if (!Path.IsPathRooted(options.From)
                || Path.GetPathRoot(options.From).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
            }
            else
            {
                options.From = Path.GetFullPath(options.From);
            }
            if (!Path.IsPathRooted(options.To)
                || Path.GetPathRoot(options.To).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
            }
            else
            {
                options.To = Path.GetFullPath(options.To);
            }

            Queue<(string, DateTime)> images = null;

            // Indexing images and load metadata
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new ElapsedTimeColumn(),
                    new TaskDescriptionColumn()
                    {
                        Alignment = Justify.Left
                    },
                })
                .AutoClear(true)
                .HideCompleted(true)
                .Start(ctx =>
                {
                    images = new Queue<(string, DateTime)>(GetImagePaths(options.From, 0, options.Depth, ctx));
                });

            AnsiConsole.Write(new Rule($"Metadata loaded. [yellow]{images.Count}[/] files to distribute!")
            {
                Style = Style.Parse("green"),
            });

            if (images.Count == 0)
            {
                return;
            }

            // Start copy images
            var duplicates = CopyImages(images, options.To, options.Structure);
            

            if (duplicates.Count == 0)
            {
                AnsiConsole.MarkupLine("No duplicates. Finished!");
                return;
            }



            // Ask what should happen with duplicates.
            AnsiConsole.Write(new Rule($"[green]There are [yellow]{duplicates.Count}[/] duplicate files![/]")
            {
                Style = Style.Parse("green"),
            });

            AnsiConsole.WriteLine();

            var duplicatesOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[yellow]How would you like to proceed with the duplicate files?[/]")
                .AddChoices(new[]
                {
                    "Skip", 
                    "Overwrite", 
                    "Add \"_copy\" to filename",
                }));

            switch (duplicatesOption)
            {
                case "Skip":
                    return;
                case "Overwrite":
                    CopyImages(duplicates, options.To, options.Structure, true);
                    return;
                default:
                    CopyImages(duplicates, options.To, options.Structure, false, "_copy");
                    return;
            }
        }

        private static Queue<(string, DateTime)> CopyImages(Queue<(string, DateTime)> images, string to, string structure, bool overwrite = false, string filenameAppend = null)
        {
            var duplicates = new Queue<(string, DateTime)>();
            
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new ElapsedTimeColumn(),
                })
                .Start(ctx =>
                {
                    var task = ctx.AddTask("Copying files... ", maxValue: images.Count);

                    do
                    {
                        var image = images.Dequeue();  
                        var directory = to + @"\" + image.Item2.ToString(structure);
                        var path = directory + Path.GetFileName(image.Item1);
                        if (filenameAppend != null)
                        {
                            var split = path.Split('.');
                            split[0] += filenameAppend;
                            path = split[0] + "." + split[1];
                        }

                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        try
                        {
                            File.Copy(image.Item1, path, overwrite);
                        }
                        catch (IOException)
                        {
                            duplicates.Enqueue(image);
                        }

                        task.Increment(1);
                    }
                    while (images.Count > 0);

                });

            return duplicates;
        }

        private static List<(string, DateTime)> GetImagePaths(string directory, uint depth, int maxDepth, ProgressContext ctx)
        {
            string[] files = Directory.GetFiles(directory);

            var task = ctx.AddTask($"[grey][white]Loading images...         [/] {directory}[/]", maxValue: files.Length + 1);

            List<(string, DateTime)> images = new List<(string, DateTime)>(files.Length);

            foreach (string image in files)
            {
                ExifSubIfdDirectory metadata = null;

                try
                {
                    metadata = ImageMetadataReader
                        .ReadMetadata(image)
                        .OfType<ExifSubIfdDirectory>()
                        .FirstOrDefault();
                }
                catch (ImageProcessingException)
                {
                    task.Increment(1);
                    continue;
                }

                task.Increment(1);

                if (metadata == null)
                {
                    continue;
                }

                if (metadata.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime datetime))
                {
                    images.Add((image, datetime));
                }
            }

            var subdirectories = Directory.GetDirectories(directory);

            if (subdirectories.Length == 0)
            {
                task.Value = task.MaxValue;
                return images;
            }

            task.Description = $"[grey][white]Loading sub directories...[/] {directory}[/]";
            task.Value = 0;
            task.MaxValue = subdirectories.Length;

            if (depth < maxDepth)
            {
                foreach (string subdirectory in subdirectories)
                {
                    images.AddRange(GetImagePaths(subdirectory, depth + 1, maxDepth, ctx));

                    task.Increment(1);
                }
            }

            return images;
        }
    }
}
