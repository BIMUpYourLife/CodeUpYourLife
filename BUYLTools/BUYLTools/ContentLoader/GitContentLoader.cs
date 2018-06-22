using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;

namespace BUYLTools.ContentLoader
{
    public static class GitContentLoader
    {
        public const string urlToRepositoryZip = "https://github.com/BIMUpYourLife/ContentUpYourLife/zipball/master";
        const string bimUpYourLife = "BIMUpYourLife";
        public const string pathToLocalContentRepository = "c:\\BIMUpYourLife\\";

        // Path where temporary download data will be stored - will be wiped after execution!
        public const string tempFilePath = "C:\\BimTestDownloadTemp\\";

        const string pathToFamilies = "_Content\\Revit\\_Families";
        public const string buylContent = "BIMUpYourLife Content";

        const string pathToTemplates = "_Content\\Revit\\_Templates";
        const string fileDECHTemplate = "Template_Revit_MEP_VERSIONCHDE.rte";
        const string fileDEDETemplate = "Template_Revit_MEP_VERSIONDEDE.rte";
        const string fileReplacer = "VERSION";

        public static bool CheckForContentRepositoryDirectory()
        {
            // check if file directory exists and is not empty
            return (Directory.Exists(pathToLocalContentRepository) && Directory.EnumerateFileSystemEntries(pathToLocalContentRepository).Any());
        }

        public static string GetLibraryPath()
        {
            return Path.Combine(pathToLocalContentRepository, pathToFamilies);
        }

        private static string GetTemplatePath()
        {
            return Path.Combine(pathToLocalContentRepository, pathToTemplates);
        }

        public static string GetDECHTemplateFile(string RvtVersion)
        {
            return Path.Combine(GetTemplatePath(), getDECHTemplateFileForVersion(RvtVersion));
        }

        private static string getDECHTemplateFileForVersion(string version)
        {
            return fileDECHTemplate.Replace(fileReplacer, version);
        }

        public static string GetDEDETemplateFile(string RvtVersion)
        {
            return Path.Combine(GetTemplatePath(), getDEDETemplateFileForVersion(RvtVersion));
        }
        private static string getDEDETemplateFileForVersion(string version)
        {
            return fileDEDETemplate.Replace(fileReplacer, version);
        }
    }
}
