// <copyright file="Options.cs" company="Marvin Fuchs">
// Copyright (c) Marvin Fuchs. All rights reserved.
// </copyright>

namespace Distribute
{
    using System.ComponentModel;
    using Spectre.Console;
    using Spectre.Console.Cli;

    /// <summary>
    /// Class containing the command line options.
    /// </summary>
    public class Options : CommandSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Options"/> class.
        /// </summary>
        public Options(
            string from,
            string to,
            string structure,
            int depth,
            bool remove)
        {
            this.From = from == "The current directory." ? Directory.GetCurrentDirectory() : from;
            this.To = to == "The current directory." ? Directory.GetCurrentDirectory() : to;
            this.Structure = structure ?? @"yyyy\\MM\\";
            this.Depth = depth;
            this.Remove = remove;
        }

        /// <summary>
        /// Gets the source directory the files should be distributed from.
        /// </summary>
        [Description("The source directory the files should be distributed from.")]
        [CommandOption("-f|--from")]
        [DefaultValue("The current directory.")]
        public string From { get; private set;}

        /// <summary>
        /// Gets the source directory the files should be distributed from.
        /// </summary>
        [Description("The directory the files should be distributed to.")]
        [CommandOption("-t|--to")]
        [DefaultValue("The current directory.")]
        public string To { get; private set; }

        /// <summary>
        /// Gets the folder structure the files should be sorted into.
        /// </summary>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings">Date and time format strings.</seealso>
        [Description("The folder structure the files should be sorted into.\nSee https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings for more information.")]
        [CommandOption("-s|--structure")]
        [DefaultValue(@"yyyy\\MM\\")]
        public string Structure { get; }

        /// <summary>
        /// Gets the maximum search depth for files in the <see cref="Distribute.Options.From"/> directory.
        /// </summary>
        [Description("The maximum search depth for files in the source directory.")]
        [CommandOption("-d|--depth")]
        [DefaultValue(5)]
        public int Depth { get; } = 5;

        /// <summary>
        /// Gets a value indicating whether the distributed and copied files should be deleted in the source directory.
        /// </summary>
        [Description("Whether the distributed and copied files should be deleted in the source directory.")]
        [CommandOption("-r|--remove")]
        public bool Remove { get; }

        /// <inheritdoc/>
        public override ValidationResult Validate()
        {
            if (this.Depth < 1)
            {
                return ValidationResult.Error("Option [-d|--depth] must be at least 1.");
            }

            if (this.From == Directory.GetCurrentDirectory() && this.To == Directory.GetCurrentDirectory())
            {
                return ValidationResult.Error("Either option [-f|--from] or [-t|--to] has to be specified.");
            }

            try
            {
                this.From = Path.GetFullPath(this.From);
            }
            catch
            {
                return ValidationResult.Error("The option [-f|--from] is an invalid path.");
            }

            try
            {
                this.To = Path.GetFullPath(this.To);
            }
            catch
            {
                return ValidationResult.Error("The option [-t|--to] is an invalid path.");
            }

            return ValidationResult.Success();
        }
    }
}
