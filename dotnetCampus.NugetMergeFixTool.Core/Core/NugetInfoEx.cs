﻿namespace dotnetCampus.NugetMergeFixTool.Core
{
    public class NugetInfoEx : NugetInfo
    {
        /// <summary>
        /// 构造一条 Nuget 包信息（拓展）
        /// </summary>
        /// <param name="nugetInfo">Nuget 包信息类</param>
        /// <param name="configPath">配置文件路径</param>
        public NugetInfoEx(NugetInfo nugetInfo, string configPath) : base(nugetInfo)
        {
            ConfigPath = configPath;
        }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public string ConfigPath { get; }
    }
}