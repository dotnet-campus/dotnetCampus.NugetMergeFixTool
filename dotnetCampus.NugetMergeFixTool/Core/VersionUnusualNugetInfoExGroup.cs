using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetMergeFixTool.Core
{
    public class VersionUnusualNugetInfoExGroup
    {
        public string NugetName { get; }

        public IEnumerable<NugetInfoEx> VersionUnusualNugetInfoExs { get; }

        public VersionUnusualNugetInfoExGroup(string nugetName, IEnumerable<NugetInfoEx> versionUnusualNugetInfoExs)
        {
            if (versionUnusualNugetInfoExs.Any(x => x.Name != nugetName))
                throw new InvalidDataException("传入的 Nuget 信息数组存在与声明的 Nuget 名称不匹配的项目");
            NugetName = nugetName;
            VersionUnusualNugetInfoExs = versionUnusualNugetInfoExs;
        }

        public VersionUnusualNugetInfoExGroup(IGrouping<string, NugetInfoEx> versionUnusualNugetInfoExs)
            : this(versionUnusualNugetInfoExs.Key, versionUnusualNugetInfoExs)
        {

        }

        public VersionUnusualNugetInfoExGroup(IEnumerable<NugetInfoEx> versionUnusualNugetInfoExs)
            : this(versionUnusualNugetInfoExs.FirstOrDefault()?.Name, versionUnusualNugetInfoExs)
        {

        }
    }
}
