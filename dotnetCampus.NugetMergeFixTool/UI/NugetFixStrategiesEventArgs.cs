using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NugetMergeFixTool.Core;

namespace NugetMergeFixTool.UI
{
    public class NugetFixStrategiesEventArgs : EventArgs
    {
        public IEnumerable<NugetFixStrategy> NugetFixStrategies { get; }

        public NugetFixStrategiesEventArgs([NotNull] IEnumerable<NugetFixStrategy> nugetFixStrategies)
        {
            NugetFixStrategies = nugetFixStrategies ?? throw new ArgumentNullException(nameof(nugetFixStrategies));
        }
    }
}
