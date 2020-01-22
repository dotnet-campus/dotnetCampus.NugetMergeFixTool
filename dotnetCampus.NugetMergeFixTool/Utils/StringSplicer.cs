using System;
using System.Diagnostics.CodeAnalysis;

namespace dotnetCampus.NugetMergeFixTool.Utils
{
    /// <summary>
    /// 字符串拼接器
    /// </summary>
    public class StringSplicer
    {
        public static string SpliceWithNewLine(string sourceString, [NotNull] string newString, int indentTab = 0)
        {
            newString = AddTabToHeader(newString, indentTab);
            return string.IsNullOrEmpty(sourceString) ? newString : sourceString + Environment.NewLine + newString;
        }

        public static string SpliceWithDoubleNewLine(string sourceString, [NotNull] string newString, int indentTab = 0)
        {
            newString = AddTabToHeader(newString, indentTab);
            return string.IsNullOrEmpty(sourceString)
                ? newString
                : sourceString + Environment.NewLine + Environment.NewLine + newString;
        }

        public static string SpliceWithComma(string sourceString, [NotNull] string newString)
        {
            return string.IsNullOrEmpty(sourceString) ? newString : $"{sourceString}, {newString}";
        }

        public static string AddTabToHeader(string sourceString, int tabCount = 1)
        {
            for (var i = 0; i < tabCount; i++)
            {
                sourceString = "\t" + sourceString;
            }

            return sourceString;
        }
    }
}