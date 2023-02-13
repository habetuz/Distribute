# Distribute

> Easily distribute images from your camera or phone into a desired folder structure!

## Why?

I archive all my images on my hard drive by their date in the following structure:

``` txt
My images
├ 2017
│ ├ 01
│ ├ 02
│ ├ 03
│ ├ 04
│ └ ...
├ 2018
│ ├ 01
│ ├ 02
│ ├ 03
│ ├ 04
│ └ ...
└ ...
```

Because sorting my images by hand into this structure is tedious I build myself this little tool to do it for me.

## Installation

### Scoop

1. Add `HabetuzApps` bucket: <br>`scoop bucket add HabetuzApps https://github.com/habetuz/HabetuzApps`
2. Install:<br>`scoop install distribute`

### Manual installation

1. Download the zip file of the [latest release](https://github.com/habetuz/Distribute/releases/tag/v1.0.0) and unpack it in your desired location.

2. Optionally you can [add the program to your `PATH` environment variable](https://www.architectryan.com/2018/03/17/add-to-the-path-on-windows-10/) to run it from any directory you want by calling `distribute`.

## Usage

``` txt
USAGE:
    distribute [OPTIONS]

OPTIONS:
                       DEFAULT
    -h, --help                                   Prints help information
    -f, --from         The current directory.    The source directory the files should be distributed from
    -t, --to           The current directory.    The directory the files should be distributed to
    -s, --structure    yyyy\\MM\\                The folder structure the files should be sorted into.
                                                 See https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings for more information
    -d, --depth        5                         The maximum search depth for files in the source directory
    -r, --remove                                 Whether the distributed and copied files should be deleted in the source directory
```