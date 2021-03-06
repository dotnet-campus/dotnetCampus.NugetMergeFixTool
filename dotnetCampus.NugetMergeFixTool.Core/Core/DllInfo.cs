﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace dotnetCampus.NugetMergeFixTool.Core
{
    public class NugetDllInfo
    {
        /// <summary>
        /// 构造 Nuget Dll 信息
        /// </summary>
        /// <param name="dllPath">Dll 绝对路径</param>
        /// <param name="dllFullName">Dll 完整名称</param>
        public NugetDllInfo([NotNull] string dllPath, [MaybeNull] string dllFullName)
        {
            DllPath = dllPath ?? throw new ArgumentNullException(nameof(dllPath));
            if (!File.Exists(DllPath))
            {
                DllFullName = dllFullName ?? throw new ArgumentNullException(nameof(dllFullName));
            }
            else
            {
                var dllFile = Assembly.LoadFile(DllPath);
                DllFullName =
                    $"{dllFile.FullName.Replace(", PublicKeyToken=null", string.Empty)}, processorArchitecture=MSIL";
            }
        }

        /// <summary>
        /// Dll 绝对路径
        /// </summary>
        public string DllPath { get; }

        /// <summary>
        /// Dll 完整名称
        /// </summary>
        public string DllFullName { get; }
    }
}