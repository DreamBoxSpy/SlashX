using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Services.Interfaces
{
    public interface IFileSystem
    {
        public IFileProvider SlashXRootFiles { get; set; }

    }
}
