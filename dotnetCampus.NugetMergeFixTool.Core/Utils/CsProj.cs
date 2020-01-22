using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using dotnetCampus.NugetMergeFixTool.Core;

namespace dotnetCampus.NugetMergeFixTool.Utils
{
    public class CsProj
    {
        private static readonly Regex _includeValueRegex =
            new Regex(@".+,\s*Version=.+,\s*Culture=.+,\s*processorArchitecture=.+");

        private static readonly Regex _nugetNameRegex = new Regex(@".+(?=,\s*Version)");

        private static readonly Regex _nugetVersionRegex = new Regex(@"(?<=Version=).+(?=,\s*Culture)");

        private static readonly Regex _nugetTargetFrameworkRegex = new Regex(@"(?<=lib\\).*(?=\\)");

        public static IEnumerable<XElement> GetPackageReferences([NotNull] XDocument xDocument)
        {
            return GetXElementsByNameInItemGroups(xDocument, PackageReferenceName);
        }

        public static IEnumerable<XElement> GetReferences([NotNull] XDocument xDocument)
        {
            return GetXElementsByNameInItemGroups(xDocument, ReferenceName);
        }

        public static bool IsNugetInfoReference([NotNull] XElement xElement)
        {
            if (xElement == null)
            {
                throw new ArgumentNullException(nameof(xElement));
            }

            if (xElement.Name.LocalName != ReferenceName)
            {
                throw new InvalidOperationException($"传入的键不是 {ReferenceName}，详情：{xElement}");
            }

            if (xElement.Attribute(IncludeAttribute) == null)
            {
                return false;
            }

            var hintPathChildElements = xElement.Elements().Where(x => x.Name.LocalName == HintPathElementName);
            if (!hintPathChildElements.Any())
            {
                return false;
            }

            if (!hintPathChildElements.Any(x => x.Value.Contains(@"\packages\")))
            {
                return false;
            }

            var includeValue = xElement.Attribute(IncludeAttribute).Value;
            return _includeValueRegex.IsMatch(includeValue);
        }

        public static IEnumerable<XElement> GetNugetInfoReferences(XDocument xDocument)
        {
            return GetReferences(xDocument).Where(x => IsNugetInfoReference(x));
        }

        public static NugetInfo GetNugetInfoFromNugetInfoReference([NotNull] XElement xElement,
            [MaybeNull] string sourceFilePath = null)
        {
            if (xElement == null)
            {
                throw new ArgumentNullException(nameof(xElement));
            }

            if (!IsNugetInfoReference(xElement))
            {
                throw new InvalidOperationException($"传入的键不含 Nuget 信息，详情：{xElement}");
            }

            var includeValue = xElement.Attribute(IncludeAttribute).Value;
            var nugetName = _nugetNameRegex.Match(includeValue).Value;
            var nugetVersion = _nugetVersionRegex.Match(includeValue).Value;
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                return new NugetInfo(nugetName, nugetVersion);
            }

            var dllPath = GetDllPath(xElement, Path.GetDirectoryName(sourceFilePath));
            var nugetDllInfo = new NugetDllInfo(dllPath, includeValue);
            return new NugetInfo(nugetName, nugetVersion, nugetDllInfo);
        }

        public static string GetTargetFrameworkOfDll(string dllFilePath)
        {
            var matchCollection = _nugetTargetFrameworkRegex.Matches(dllFilePath);
            return matchCollection[matchCollection.Count - 1].Value;
        }

        public const string RootName = "Project";

        public const string ItemGroupName = "ItemGroup";

        public const string PackageReferenceName = "PackageReference";

        public const string ReferenceName = "Reference";

        public const string IncludeAttribute = "Include";

        public const string UpdateAttribute = "Update";

        public const string VersionAttribute = "Version";

        public const string VersionElementName = VersionAttribute;

        public const string HintPathElementName = "HintPath";

        private static IEnumerable<XElement> GetXElementsByNameInItemGroups([NotNull] XDocument xDocument,
            [NotNull] string xElementName)
        {
            if (xDocument == null)
            {
                throw new ArgumentNullException(nameof(xDocument));
            }

            if (xElementName == null)
            {
                throw new ArgumentNullException(nameof(xElementName));
            }

            var xElementList = new List<XElement>();
            var itemGroupElements = xDocument.Root.Elements().Where(x => x.Name.LocalName == ItemGroupName);
            foreach (var itemGroupElement in itemGroupElements)
            {
                xElementList.AddRange(itemGroupElement.Elements().Where(x => x.Name.LocalName == xElementName));
            }

            return xElementList;
        }

        private static string GetDllPath([NotNull] XElement xElement, [NotNull] string csProjDirectory)
        {
            if (xElement == null)
            {
                throw new ArgumentNullException(nameof(xElement));
            }

            if (csProjDirectory == null)
            {
                throw new ArgumentNullException(nameof(csProjDirectory));
            }

            if (!IsNugetInfoReference(xElement))
            {
                throw new InvalidOperationException($"传入的键不含 Nuget 信息，详情：{xElement}");
            }

            if (!Directory.Exists(csProjDirectory))
            {
                throw new DirectoryNotFoundException(csProjDirectory);
            }

            var dllRelativePath = xElement.Elements().First(x => x.Name.LocalName == HintPathElementName).Value;
            var dllAbsolutePath = Path.GetFullPath(Path.Combine(csProjDirectory, dllRelativePath));
            return dllAbsolutePath;
        }
    }
}