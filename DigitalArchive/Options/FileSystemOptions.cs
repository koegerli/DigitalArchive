using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalArchive.Options;

public class FileSystemOptions
{
    public static string SectionName => "FileSystem";

    public string InputPath { get; set; }
    public string OutputPath { get; set; }
}
