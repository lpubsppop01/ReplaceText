# Replace Text

[![Build status](https://ci.appveyor.com/api/projects/status/9naynk0k7xlnl84q?svg=true)](https://ci.appveyor.com/project/lpubsppop01/replacetext)

A CLI tool to replace part of file contents, file names and directory names.

## Features

```powershell
$rt = /path/to/lpubsppop01.ReplaceText.exe

# "s" command like sed that targets filename and file content
&$rt 's/hoge/piyo/g' hoge.txt    # Report only
&$rt 's/hoge/piyo/g' hoge.txt -r # Replace existing file
&$rt 's/hoge/piyo/g' hoge.txt -g # Generate new file "piyo.txt"

# Process directory recursively
&$rt 's/hoge/piyo/g' hoge_dir

# Multi commands and multi targets
&$rt 's/hoge/piyo/g' 's/piyo/fuga/g' hoge_dir piyo.txt

# Regex pattern like sed
&$rt 's/index-\([0-9]+\)/number-\1/g' hoge.txt

# 日本語の文字コードを考慮しています
&$rt 's/ほげ/ぴよ/g' ほげ.txt
```

## Download

Latest Build:
- [ReplaceText-win-x64.zip](https://ci.appveyor.com/api/projects/lpubsppop01/replacetext/artifacts/ReplaceText-win-x64.zip)
- [ReplaceText-linux-x64.zip](https://ci.appveyor.com/api/projects/lpubsppop01/replacetext/artifacts/ReplaceText-linux-x64.zip)

## Author

[lpubsppop01](https://github.com/lpubsppop01)

## License

[zlib License](https://github.com/lpubsppop01/ReplaceText/raw/master/LICENSE.txt)

This software uses the following NuGet packages:
- [ReadJEnc 1.3.1.2](https://www.nuget.org/packages/ReadJEnc/)  
  Copyright (c) 2017 hnx8  
  Released under the [MIT License](https://github.com/hnx8/ReadJEnc/blob/master/LICENSE)
- [System.Text.Encoding.CodePages 4.5.0](https://www.nuget.org/packages/System.Text.Encoding.CodePages/)  
  Copyright (c) .NET Foundation and Contributors  
  Released under the [MIT License](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT)
