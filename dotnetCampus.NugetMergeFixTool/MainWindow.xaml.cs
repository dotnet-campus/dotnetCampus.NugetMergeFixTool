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

            _configs = ConfigurationFactory.FromFile(GetConfigFile().FullName).CreateAppConfigurator().Of<DefaultConfiguration>();
            Loaded += (sender, args) =>
            {
                TextBoxIdePath.Text = _configs["IdePath"] ?? "";
                SetSolutionFile();
            };
        }

        private static FileInfo GetConfigFile()
        {
            // 为什么不能放在应用程序运行文件夹，因为可以通过运行文件夹找到对应的 sln 如我在 C:\lindexi 运行软件，此时如果写入配置文件，那么我还需要从源代码工具去掉这个工具，或者我的代码也用了这个配置文件

            // 配置文件路径是 appdata\dotnet campus\NugetMergeFixTool\Configs.fkv
            return new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dotnet campus", "NugetMergeFixTool", "Configs.fkv"));
        }

        private void SetSolutionFile()
        {
            // 当前工作路径是否包含 sln 文件
            if (TryGetSlnFile(Environment.CurrentDirectory, out var slnFile))
            {
                TextBoxDirectory.Text = slnFile;
            }
            else
            {
                TextBoxDirectory.Text = _configs["SoluctionFile"] ?? "";
            }
        }

        private static bool TryGetSlnFile(string folder, out string slnFile)
        {
            slnFile = null;

            if (!Directory.Exists(folder))
            {
                return false;
            }

            var slnFileList = Directory.GetFiles(folder, "*.sln");
            if (slnFileList.Length > 0)
            // || Directory.GetFiles(Environment.CurrentDirectory, "*.csproj").Length > 0)
            {
                slnFile = slnFileList[0];
                return true;
            }

            return false;
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
                // 其实输入的可能是文件夹
                if (TryGetSlnFile(solutionFile, out var slnFile))
                {
                    solutionFile = slnFile;
                }
                else
                {
                    MessageBox.Show("找不到指定的解决方案，这是啥情况？？？");
                    return;
                }
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