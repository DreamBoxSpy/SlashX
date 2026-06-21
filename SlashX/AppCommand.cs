using Microsoft.Extensions.DependencyInjection;
using SlashX.CLI;
using SlashX.Services.Interfaces;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX
{
    internal class AppCommand : Command<SlashXCLISettings>
    {
        protected override int Execute(CommandContext context, SlashXCLISettings settings, CancellationToken cancellationToken)
        {
            var services = (IServiceProvider) context.Data!;

            var app = services.GetRequiredService<IApplicationDefault>();
            return app.Run();
        }
    }
}
