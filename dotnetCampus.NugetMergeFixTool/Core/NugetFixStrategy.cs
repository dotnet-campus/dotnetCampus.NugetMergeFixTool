using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NugetMergeFixTool.Utils;

namespace NugetMergeFixTool.Core
{
    /// <summary>
    /// Nuget 包修复策略
    /// </summary>
    public class NugetFixStrategy
    {
        /// <summary>
        /// Nuget 名称
        /// </summary>
        public string NugetName { get; }

        /// <summary>
        /// Nuget 版本号
        /// </summary>
        public string NugetVersion { get; }

        /// <summary>
        /// Nuget 目标框架
        /// </summary>
        public string TargetFramework { get; }

        /// <summary>
        /// Dll 信息
        /// </summary>
        public NugetDllInfo NugetDllInfo { get; }

        /// <summary>
        /// 构造一条 Nuget 包修复策略
        /// </summary>
        /// <param name="nugetName">名称</param>
        /// <param name="nugetVersion">版本号</param>
        public NugetFixStrategy(string nugetName, string nugetVersion, string targetFramework) : this(nugetName, nugetVersion)
        {
            TargetFramework = targetFramework;
            var userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dllFilePath = Path.Combine(userProfileFolder, ".nuget", "packages", nugetName, nugetVersion, "lib", TargetFramework, $"{nugetName}.dll");
            if (!File.Exists(dllFilePath))
            {
                MessageBox.Show($"找不到 {dllFilePath}，无法进行修复。要不您老人家先试着编译一下，还原下 Nuget 包，然后再来看看？");
                return;
            }
            NugetDllInfo = new NugetDllInfo(dllFilePath, null);
        }

        /// <summary>
        /// 构造一条 Nuget 包修复策略
        /// </summary>
        /// <param name="nugetName">名称</param>
        /// <param name="nugetVersion">版本号</param>
        /// <param name="nugetDllInfo">Dll 信息</param>
        public NugetFixStrategy(string nugetName, string nugetVersion, [NotNull] NugetDllInfo nugetDllInfo) : this(nugetName, nugetVersion)
        {
            NugetDllInfo = nugetDllInfo ?? throw new ArgumentNullException(nameof(nugetDllInfo));
            TargetFramework = CsProj.GetTargetFrameworkOfDll(nugetDllInfo.DllPath);
        }

        private NugetFixStrategy(string nugetName, string nugetVersion)
        {
            NugetName = nugetName;
            NugetVersion = nugetVersion;
        }
    }
}
