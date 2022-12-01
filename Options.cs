namespace Distribute
{
    using CommandLine;

    internal class Options
    {
        [Option('f', "from", Group = "Paths", HelpText = "The path to the files that sould be distributed or empty for the current path.")]
        public string From { get; set; }

        [Option('t', "to", Group = "Paths", HelpText = "The path where the files should be distributed to or empty for the current path.")]
        public string To { get; set; }

        [Option('s', "structure", HelpText = "The folder structure the files should be sorted into.", Default = "yyyy\\\\MM\\\\")]
        public string Structure { get; set; }

        [Option('d', "depth", HelpText = "The search depth.", Default = 5)]
        public int Depth { get; set; }

        [Option('r', "remove", HelpText = "Wether copied images should be deleted in the source.")]
        public bool Remove { get; set; }
    }
}
