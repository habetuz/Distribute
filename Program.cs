using Distribute;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<DistributeCommand>();

#if DEBUG
app.Configure(ctx => ctx.PropagateExceptions());
#endif

try
{
  app.Run(args);
}
catch (Exception e)
{
  AnsiConsole.WriteException(e);
  return 1;
}

return 0;
