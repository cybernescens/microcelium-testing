using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

[assembly: Parallelizable(ParallelScope.All)]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]


