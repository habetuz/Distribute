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
  -f, --from         (Group: Paths) The path to the files that sould be distributed or empty for the current path.

  -t, --to           (Group: Paths) The path where the files should be distributed to or empty for the current path.

  -s, --structure    (Default: yyyy\\MM\\) The folder structure the files should be sorted into.

  -d, --depth        (Default: 5) The search depth.

  -r, --remove       Wether copied images should be deleted in the source.

  --help             Display this help screen.

  --version          Display version information.
```