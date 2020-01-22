using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using dotnetCampus.NugetMergeFixTool.Utils;

namespace dotnetCampus.NugetMergeFixTool.Core
{
    public class NugetVersionChecker
    {
        #region 构造函数

        /// <summary>
        /// 构造一个 Nuget 版本检查器
        /// </summary>
        /// <param name="solutionFilePath">解决方案路径</param>
        public NugetVersionChecker([NotNull] string solutionFilePath)
        {
            if (solutionFilePath == null)
            {
                throw new ArgumentNullException(nameof(solutionFilePath));
            }

            if (!File.Exists(solutionFilePath))
            {
                throw new FileNotFoundException(solutionFilePath);
            }

            _solutionFilePath = solutionFilePath;
            CheckNugetVersion();
        }

        #endregion

        #region 私有变量

        private readonly string _solutionFilePath;

        #endregion

        #region 公共字段

        /// <summary>
        /// 异常 Nuget 配置文件列表
        /// </summary>
        public IEnumerable<NugetConfigReader> ErrorFormatNugetConfigs { get; private set; }

        public IEnumerable<VersionUnusualNugetInfoExGroup> MismatchVersionNugetInfoExs { get; private set; }

        /// <summary>
        /// 检测信息
        /// </summary>
        public string Message { get; private set; }

        #endregion

        #region 私有方法

        private void CheckNugetVersion()
        {
            var projectFiles = GetProjectFilesFromSolutionFile(_solutionFilePath);
            var projectDirectories = projectFiles.Select(Path.GetDirectoryName);
            var nugetConfigFiles = new List<string>();
            foreach (var projectDirectory in projectDirectories)
            {
                nugetConfigFiles.AddRange(GetNugetConfigFiles(projectDirectory));
            }

            var badFormatNugetConfigList = new List<NugetConfigReader>();
            var goodFormatNugetInfoExList = new List<NugetInfoEx>();
            foreach (var nugetConfigFile in nugetConfigFiles)
            {
                var nugetConfigReader = new NugetConfigReader(nugetConfigFile);
                if (nugetConfigReader.IsGoodFormat())
                {
                    goodFormatNugetInfoExList.AddRange(nugetConfigReader.PackageInfoExs);
                }
                else
                {
                    badFormatNugetConfigList.Add(nugetConfigReader);
                }
            }

            ErrorFormatNugetConfigs = badFormatNugetConfigList;
            MismatchVersionNugetInfoExs = GetMismatchVersionNugets(goodFormatNugetInfoExList);
            var nugetMismatchVersionMessage = CreateNugetMismatchVersionMessage(MismatchVersionNugetInfoExs);
            foreach (var errorFormatNugetConfig in ErrorFormatNugetConfigs)
            {
                Message = StringSplicer.SpliceWithDoubleNewLine(Message, errorFormatNugetConfig.ErrorMessage);
            }

            Message = StringSplicer.SpliceWithDoubleNewLine(Message, nugetMismatchVersionMessage);
            if (string.IsNullOrEmpty(Message))
            {
                Message = "完美无瑕！";
            }
        }

        /// <summary>
        /// 获取目录下所有的 Nuget 配置文件
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetNugetConfigFiles(string directoryPath)
        {
#if TEST
            const string packagesConfigSearchPattern = "*.config";
            const string csProjSearchPattern = "*.csproj";
#else
            const string packagesConfigSearchPattern = "packages.config";
            const string csProjSearchPattern = "*.csproj";
#endif
            var packagesConfigs = GetFilesFromDirectory(directoryPath, packagesConfigSearchPattern);
            var csProjs = GetFilesFromDirectory(directoryPath, csProjSearchPattern);
            return packagesConfigs.Concat(csProjs);
        }

        /// <summary>
        /// 从指定目录中获取所有文件（含子文件夹）
        /// </summary>
        /// <param name="directoryPath">待遍历的目录</param>
        /// <param name="searchPattern">搜索字符串</param>
        /// <returns>获取到的文件路径列表</returns>
        private IEnumerable<string> GetFilesFromDirectory(string directoryPath, string searchPattern = null)
        {
            if (searchPattern == null)
            {
                searchPattern = "*";
            }

            var files = Directory.EnumerateFiles(directoryPath, searchPattern);
            foreach (var directory in Directory.GetDirectories(directoryPath))
            {
                files = files.Concat(GetFilesFromDirectory(directory, searchPattern));
            }

            return files;
        }

        private IEnumerable<VersionUnusualNugetInfoExGroup> GetMismatchVersionNugets(
            [NotNull] IEnumerable<NugetInfoEx> nugetPackageInfoExs)
        {
            var mismatchVersionNugetGroupList = new List<VersionUnusualNugetInfoExGroup>();
            var nugetPackageInfoGroups = nugetPackageInfoExs.GroupBy(x => x.Name);
            foreach (var nugetPackageInfoGroup in nugetPackageInfoGroups)
            {
                var groupByConfigPath = nugetPackageInfoGroup.GroupBy(x => x.ConfigPath);
                if (groupByConfigPath.All(x => x.Count() == 1) &&
                    nugetPackageInfoGroup.Select(x => x.Version).Distinct().Count() == 1)
                {
                    continue;
                }

                mismatchVersionNugetGroupList.Add(new VersionUnusualNugetInfoExGroup(nugetPackageInfoGroup));
            }

            return mismatchVersionNugetGroupList;
        }

        private string CreateNugetMismatchVersionMessage(
            [NotNull] IEnumerable<VersionUnusualNugetInfoExGroup> mismatchVersionNugetInfoExs)
        {
            var nugetMismatchVersionMessage = string.Empty;
            foreach (var mismatchVersionNugetInfoEx in mismatchVersionNugetInfoExs)
            {
                var headMessage = $"{mismatchVersionNugetInfoEx.NugetName} 存在版本异常：";
                var detailMessage = string.Empty;
                foreach (var nugetPackageInfo in mismatchVersionNugetInfoEx.VersionUnusualNugetInfoExs)
                {
                    var mainDetailMessage = $"  {nugetPackageInfo.Version}，{nugetPackageInfo.ConfigPath}";
                    detailMessage = StringSplicer.SpliceWithNewLine(detailMessage, mainDetailMessage);
                }

                var singleNugetMismatchVersionMessage = StringSplicer.SpliceWithNewLine(headMessage, detailMessage);
                nugetMismatchVersionMessage = StringSplicer.SpliceWithDoubleNewLine(nugetMismatchVersionMessage,
                    singleNugetMismatchVersionMessage);
            }

            return nugetMismatchVersionMessage;
        }

        private IEnumerable<string> GetProjectFilesFromSolutionFile(string soluctionFile)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(soluctionFile));
            var text = File.ReadAllText(soluctionFile);
            var regex = new Regex(
                @"Project\(""{[\w-]+}""\)\s*=\s*""[\w\.]+"",\s*""(?<csprojPath>.+\.csproj)"",\s*""{[\w-]+}""");
            return FindProjectFiles();

            IEnumerable<string> FindProjectFiles()
            {
                var matches = regex.Matches(text);
                foreach (Match match in matches)
                {
                    var csprojPath = match.Groups["csprojPath"].Value;
                    var path = Path.Combine(directory, csprojPath);
                    yield return path;
                }
            }
        }

        #endregion
    }
}