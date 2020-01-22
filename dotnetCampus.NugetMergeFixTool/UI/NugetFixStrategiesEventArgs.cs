using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using dotnetCampus.NugetMergeFixTool.Core;

namespace dotnetCampus.NugetMergeFixTool.UI
{
    public class NugetFixStrategiesEventArgs : EventArgs
    {
        public NugetFixStrategiesEventArgs([NotNull] IEnumerable<NugetFixStrategy> nugetFixStrategies)
        {
            NugetFixStrategies = nugetFixStrategies ?? throw new ArgumentNullException(nameof(nugetFixStrategies));
        }

        public IEnumerable<NugetFixStrategy> NugetFixStrategies { get; }
    }
}