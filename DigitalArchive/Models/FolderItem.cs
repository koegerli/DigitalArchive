using System.Collections.Generic;

namespace DigitalArchive.Models;

public class FolderItem : ExplorerItem
{
    public IEnumerable<ExplorerItem> Children { get; set; }
}
