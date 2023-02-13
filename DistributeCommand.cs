// <copyright file="DistributeCommand.cs" company="Marvin Fuchs">
// Copyright (c) Marvin Fuchs. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Spectre.Console;
using Spectre.Console.Cli;
using Directory = System.IO.Directory;

namespace Distribute
{
    /// <summary>
    /// Class containing the distribute command that is responsible for distributing the images from the source directory.
    /// </summary>
    internal class DistributeCommand : Command<Options>
    {
        /// <inheritdoc/>
        public override int Execute([NotNull] CommandContext context, [NotNull] Options options)
        {
            AnsiConsole.Foreground = Color.Green;

            Queue<(string, DateTime)> images = new ();

            // Indexing images and load metadata
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new ElapsedTimeColumn(),
                    new TaskDescriptionColumn() { Alignment = Justify.Left },
                })
                .AutoClear(true)
                .HideCompleted(true)
                .Start(ctx => images = new (GetImagePaths(options.From, 0, options.Depth, ctx)));

            AnsiConsole.Write(new Rule($"Metadata loaded. [yellow]{images.Count}[/] files to distribute!")
            {
                Style = Style.Parse("green"),
            });

            if (images.Count == 0)
            {
                AnsiConsole.MarkupLine("No images found. Finished!");
                return 0;
            }

            // Start copy images
            var duplicates = CopyImages(images, options.To, options.Structure);

            if (duplicates.Count == 0)
            {
                AnsiConsole.MarkupLine("No duplicates. Finished!");
                return 0;
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
                    return 0;
                case "Overwrite":
                    CopyImages(duplicates, options.To, options.Structure, true);
                    return 0;
                default:
                    CopyImages(duplicates, options.To, options.Structure, false, "_copy");
                    return 0;
            }
        }

        private static Queue<(string, DateTime)> CopyImages(Queue<(string, DateTime)> images, string to, string structure, bool overwrite = false, string? filenameAppend = null)
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

            List<(string, DateTime)> images = new (files.Length);

            foreach (string image in files)
            {
                ExifSubIfdDirectory metadata;

                try
                {
                    metadata = ImageMetadataReader
                        .ReadMetadata(image)
                        .OfType<ExifSubIfdDirectory>()
                        .FirstOrDefault() !;
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
