﻿using System.Collections.Generic;

namespace dotnetCampus.NugetMergeFixTool.Core.NugetConfigParser
{
    /// <summary>
    /// Nuget 配置解析器接口
    /// </summary>
    internal interface INugetConfigParser
    {
        /// <summary>
        /// 解析时的异常信息
        /// </summary>
        string ExceptionMessage { get; }

        /// <summary>
        /// 是否格式正常
        /// </summary>
        /// <returns>是否格式正常</returns>
        bool IsGoodFormat();

        /// <summary>
        /// 获取 Nuget 包信息列表
        /// </summary>
        /// <returns>Nuget 包信息列表</returns>
        IEnumerable<NugetInfo> GetNugetInfos();
    }
}