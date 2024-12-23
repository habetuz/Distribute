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

Goto the release page and fetch your fitting binary.

## Usage

``` txt
USAGE:
    Distribute.dll [From] [To] [OPTIONS]

ARGUMENTS:
    [From]    The source directory the files should be distributed from
    [To]      The directory the files should be distributed to

OPTIONS:
                       DEFAULT
    -h, --help                         Shows help an                                                                       
    -s, --structure    yyyy'/'MM'/'    The folder structure the files should be sorted into.
                                       See
                                       https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-for
                                       mat-strings for more information
    -d, --depth        5               The maximum search depth for files in the source directory
    -r, --remove                       Whether the distributed and copied files should be deleted in the source directory  
```