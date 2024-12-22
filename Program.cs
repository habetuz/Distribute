using Distribute;
using Spectre.Console.Cli;

var app = new CommandApp<DistributeCommand>();

#if DEBUG
app.Configure(ctx => ctx.PropagateExceptions());
#endif

app.Run(args);
