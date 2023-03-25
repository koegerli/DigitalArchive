using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalArchive.Models;

public class ExplorerItem
{
    public string Name { get; set; }
    public string Path { get; set; }
    public IEnumerable<ExplorerItem> Children { get; set; }
    public ExplorerItemType Type { get; set; }
}