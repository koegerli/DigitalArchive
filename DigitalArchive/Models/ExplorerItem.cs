using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalArchive.Models;

public abstract class ExplorerItem
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsSelected { get; set; }
}
