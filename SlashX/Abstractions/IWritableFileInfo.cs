using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Abstractions
{
    public interface IWritableFileInfo : IFileInfo
    {
        public Stream CreateWriteStream();
    }
}
