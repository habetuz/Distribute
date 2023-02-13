// <copyright file="Program.cs" company="Marvin Fuchs">
// Copyright (c) Marvin Fuchs. All rights reserved.
// </copyright>

using Distribute;
using Spectre.Console.Cli;

var app = new CommandApp<DistributeCommand>();

#if DEBUG
app.Configure(ctx => ctx.PropagateExceptions());
#endif

app.Run(args);