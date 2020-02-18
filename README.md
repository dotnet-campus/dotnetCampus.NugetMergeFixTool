# dotnetCampus.NugetMergeFixTool

传说博哥的工具

可以用来修复 git 合并的时候将 csproj 合并坏了的问题，也可以用来快速升级 NuGet 库

## dotnet tool

在命令行输入下面代码安装

```
dotnet tool install -g NugetMergeFixTool
```

此后可以通过以下命令行启动

```csharp
dotnet nugetfix
```

启动之后设置 sln 所在路径，接下来看界面就会了

暂时只支持 Windows 平台

