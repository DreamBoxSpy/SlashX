using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SlashX.Services.Interfaces
{
    public interface ILibraryManager
    {
        public string LibrariesRoot { get; }
        public string PackagesRoot { get; }
        public Task FixDependencies(string packageId, string? version, CancellationToken cancellationToken = default);
        public Task FixDependencies(string packageId, CancellationToken cancellation = default);
        internal Task<IEnumerable<string>> ResolveLibraries(CancellationToken cancellationToken, params IEnumerable<string> packageId);
    }
}
