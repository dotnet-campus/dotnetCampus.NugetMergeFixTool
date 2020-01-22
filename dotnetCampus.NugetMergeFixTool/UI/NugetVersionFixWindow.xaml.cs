using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using dotnetCampus.NugetMergeFixTool.Core;
using dotnetCampus.NugetMergeFixTool.Utils;

namespace dotnetCampus.NugetMergeFixTool.UI
{
    /// <summary>
    /// NugetVersionFixWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NugetVersionFixWindow : Window
    {
        public NugetVersionFixWindow(IEnumerable<VersionUnusualNugetInfoExGroup> mismatchVersionNugetInfoExs)
        {
            InitializeComponent();
            _mismatchVersionNugetInfoExs = mismatchVersionNugetInfoExs;
            foreach (var mismatchVersionNugetInfoEx in _mismatchVersionNugetInfoExs)
            {
                var nugetName = mismatchVersionNugetInfoEx.NugetName;
                var repeatNugetVersions = mismatchVersionNugetInfoEx.VersionUnusualNugetInfoExs.Select(x => x.Version)
                    .Distinct();
                var nugetVersionSelectorUserControl =
                    new NugetVersionSelectorUserControl(nugetName, repeatNugetVersions);
                PanelNugetVersionSelectors.Children.Add(nugetVersionSelectorUserControl);
            }
        }

        public event EventHandler<NugetFixStrategiesEventArgs> NugetFixStrategiesSelected;

        private readonly IEnumerable<VersionUnusualNugetInfoExGroup> _mismatchVersionNugetInfoExs;

        private readonly List<NugetFixStrategy> _nugetFixStrategyList = new List<NugetFixStrategy>();

        private void ButtonFix_OnClick(object sender, RoutedEventArgs e)
        {
            _nugetFixStrategyList.Clear();
            foreach (var child in PanelNugetVersionSelectors.Children)
            {
                if (!(child is NugetVersionSelectorUserControl nugetVersionSelectorUserControl))
                {
                    continue;
                }

                var nugetName = nugetVersionSelectorUserControl.NugetName;
                var selectedVersion = nugetVersionSelectorUserControl.SelectedVersion;
                var versionUnusualNugetInfoExGroup = _mismatchVersionNugetInfoExs.First(x => x.NugetName == nugetName);
                var selectedVersionNugetInfoExs =
                    versionUnusualNugetInfoExGroup.VersionUnusualNugetInfoExs.Where(x => x.Version == selectedVersion);
                var targetFrameworks = selectedVersionNugetInfoExs.Where(x => x.TargetFramework != null)
                    .Select(x => x.TargetFramework).Distinct().ToList();
                targetFrameworks.Sort();
                targetFrameworks.Reverse();
                var nugetDllInfos = selectedVersionNugetInfoExs.Where(x => x.NugetDllInfo != null)
                    .Select(x => x.NugetDllInfo).Distinct();
                var dllPaths = nugetDllInfos.Select(x => x.DllPath).Distinct();
                if (dllPaths.Count() > 1)
                {
                    var errorMessage = "指定的修复策略存在多个 Dll 路径，修复工具无法确定应该使用哪一个。请保留现场并联系开发者。";
                    var dllPathMessage = string.Empty;
                    foreach (var dllPath in dllPaths)
                    {
                        dllPathMessage = StringSplicer.SpliceWithNewLine(dllPathMessage, dllPath);
                    }

                    errorMessage = StringSplicer.SpliceWithDoubleNewLine(errorMessage, dllPathMessage);
                    MessageBox.Show(errorMessage);
                    continue;
                }

                var nugetDllInfo = nugetDllInfos.FirstOrDefault();
                if (nugetDllInfo != null)
                {
                    _nugetFixStrategyList.Add(new NugetFixStrategy(nugetName, selectedVersion, nugetDllInfo));
                }
                else
                {
                    _nugetFixStrategyList.Add(
                        new NugetFixStrategy(nugetName, selectedVersion, targetFrameworks.First()));
                }
            }

            if (!_nugetFixStrategyList.Any())
            {
                return;
            }

            NugetFixStrategiesSelected?.Invoke(this, new NugetFixStrategiesEventArgs(_nugetFixStrategyList));
        }
    }
}