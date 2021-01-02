using System.Runtime.CompilerServices;
using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.All)]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]