using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using dotnetCampus.Configurations;
using dotnetCampus.Configurations.Core;
using dotnetCampus.NugetMergeFixTool.Core;
using dotnetCampus.NugetMergeFixTool.UI;
using dotnetCampus.NugetMergeFixTool.Utils;

namespace dotnetCampus.NugetMergeFixTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _configs = ConfigurationFactory.FromFile("Configs.fkv").CreateAppConfigurator().Of<DefaultConfiguration>();
            Loaded += (sender, args) =>
            {
                TextBoxIdePath.Text = _configs["IdePath"] ?? "";
                SetSolutionFile();
            };
        }

        private void SetSolutionFile()
        {
            // 当前工作路径是否包含 sln 文件
            if (Directory.GetFiles(Environment.CurrentDirectory, "*.sln").Length > 0
                || Directory.GetFiles(Environment.CurrentDirectory, "*.csproj").Length > 0)
            {
                TextBoxDirectory.Text = Environment.CurrentDirectory;
            }
            else
            {
                TextBoxDirectory.Text = _configs["SoluctionFile"] ?? "";
            }
        }

        [NotNull] private readonly DefaultConfiguration _configs;

        private NugetVersionChecker _nugetVersionChecker;

        private void Check()
        {
            TextBoxIdePath.Text = TextBoxIdePath.Text.Trim('"');
            TextBoxDirectory.Text = TextBoxDirectory.Text.Trim('"');
            var idePath = TextBoxIdePath.Text;
            var solutionFile = TextBoxDirectory.Text;
            if (string.IsNullOrWhiteSpace(solutionFile))
            {
                MessageBox.Show("源代码路径不能为空…… 心急吃不了热豆腐……");
                return;
            }

            if (!File.Exists(solutionFile))
            {
                MessageBox.Show("找不到指定的解决方案，这是啥情况？？？");
                return;
            }

            _configs["IdePath"] = idePath;
            _configs["SoluctionFile"] = solutionFile;
            _nugetVersionChecker = new NugetVersionChecker(solutionFile);
            TextBoxErrorMessage.Text = _nugetVersionChecker.Message;
            ButtonFixFormat.IsEnabled = _nugetVersionChecker.ErrorFormatNugetConfigs.Any();
            ButtonFixVersion.IsEnabled = _nugetVersionChecker.MismatchVersionNugetInfoExs.Any() &&
                                         !_nugetVersionChecker.ErrorFormatNugetConfigs.Any();
        }

        private void ButtonCheck_OnClick(object sender, RoutedEventArgs e)
        {
            Check();
        }

        private void ButtonFixFormat_OnClick(object sender, RoutedEventArgs e)
        {
            var idePath = TextBoxIdePath.Text;
            if (string.IsNullOrWhiteSpace(idePath))
            {
                MessageBox.Show("大佬，IDE 路径都还没配置，你这样我很难帮你办事啊……");
                return;
            }

            if (!File.Exists(idePath))
            {
                MessageBox.Show("找不到配置的 IDE，可能离家出走了吧……");
                return;
            }

            OpenFilesByIde(idePath, _nugetVersionChecker.ErrorFormatNugetConfigs.Select(x => x.FilePath).Distinct());
        }

        private void OpenFilesByIde(string idePath, IEnumerable<string> filePaths)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            try
            {
                process.Start();
                foreach (var filePath in filePaths)
                {
                    process.StandardInput.WriteLine($"\"{idePath}\" \"{filePath}\"");
                }
            }
            finally
            {
                process.Close();
            }
        }

        private void ButtonFix_OnClick(object sender, RoutedEventArgs e)
        {
            var nugetVersionFixWindow = new NugetVersionFixWindow(_nugetVersionChecker.MismatchVersionNugetInfoExs)
            {
                Owner = this
            };
            nugetVersionFixWindow.NugetFixStrategiesSelected += (o, args) =>
            {
                var nugetFixStrategies = args.NugetFixStrategies;
                if (nugetFixStrategies == null || !nugetFixStrategies.Any())
                {
                    return;
                }

                var repairLog = string.Empty;
                foreach (var mismatchVersionNugetInfoEx in _nugetVersionChecker.MismatchVersionNugetInfoExs)
                {
                    foreach (var nugetInfoEx in mismatchVersionNugetInfoEx.VersionUnusualNugetInfoExs)
                    {
                        var nugetConfigRepairer = new NugetConfigRepairer(nugetInfoEx.ConfigPath, nugetFixStrategies);
                        nugetConfigRepairer.Repair();
                        repairLog = StringSplicer.SpliceWithDoubleNewLine(repairLog, nugetConfigRepairer.Log);
                    }
                }

                TextBoxErrorMessage.Text = repairLog;
                ButtonFixVersion.IsEnabled = false;
                nugetVersionFixWindow.Close();
            };
            nugetVersionFixWindow.ShowDialog();
        }
    }
}