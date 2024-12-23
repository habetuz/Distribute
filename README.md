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

## Features

- Specify a folder from wich files get loaded and a folder where the files should be sorted into using `distribute [From] [To]`.
- Use `-s|--structure` define a structure using [a custom date and time format](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).
- Delete copied files with `-d|--delete`.
- Media devices (like connected phones on Windows) are supported.

### Copy from or to media devices

When connecting a phone to a windows computer it can only be accessed by [MTP](https://en.wikipedia.org/wiki/Media_Transfer_Protocol) or [FTP](https://en.wikipedia.org/wiki/File_Transfer_Protocol).

To specify a media device to copy from or to use the following notation:

`Device Name:\Drive Name\Path`

where `Device Name` is the name listed in your explorer and `Drive Name` is the drive listed in the device.

## Installation

Goto the release page and fetch your fitting binary.

## Usage

``` txt
USAGE:
    distribute [From] [To] [OPTIONS]

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