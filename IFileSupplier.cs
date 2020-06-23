using System;
using System.Collections.Generic;
using System.Text;

namespace App1
{
    public interface IFileSupplier
    {
        List<string> FilesWithExtension(string extension);

        string[] MusicFolders { get; }
    }
}
