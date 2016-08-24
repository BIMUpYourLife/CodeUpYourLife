using System.IO;
using System.Threading.Tasks;

namespace BUYLTools.ContentLoader
{
    public static class GitContentLoader
    {
        const string urlContentRepository = "https://github.com/BIMUpYourLife/ContentUpYourLife.git";
        const string bimUpYourLife = "BIMUpYourLife";
        public const string pathToLocalContentRepository = "c:\\BIMUpYourLife\\ContentUpYourLife";

        const string pathToFamilies = "_Content\\Revit\\_Families";
        public const string buylContent = "BIMUpYourLife Content";

        const string pathToTemplates = "_Content\\Revit\\_Templates";
        const string fileDECHTemplate = "Template_Revit_MEP_VERSIONCHDE.rte";
        const string fileDEDETemplate = "Template_Revit_MEP_VERSIONDEDE.rte";
        const string fileReplacer = "VERSION";

        public static async Task BUYLCloneOrUpdateRepository()
        {
            if (!CheckForContentRepositoryDirectory())
            {
                //prg.ShowDialog();
                Directory.CreateDirectory(pathToLocalContentRepository);

                LibGit2Sharp.CloneOptions op = new LibGit2Sharp.CloneOptions();

                LibGit2Sharp.Repository.Clone(urlContentRepository, pathToLocalContentRepository, op);

                LogManager.FileLogService.Instance.Info(string.Format("Repository cloned to {0}", pathToLocalContentRepository), typeof(GitContentLoader));
                //prg.Close();
            }
            else
            {
                using (var repo = new LibGit2Sharp.Repository(pathToLocalContentRepository))
                {
                    LibGit2Sharp.Remote remote = repo.Network.Remotes["origin"];
                    LibGit2Sharp.FetchOptions op = new LibGit2Sharp.FetchOptions();
                    repo.Network.Fetch(remote);
                    LogManager.FileLogService.Instance.Info(string.Format("Repository update fetched to {0}", pathToLocalContentRepository), typeof(GitContentLoader));
                }
            }
        }

        public static bool CheckForContentRepositoryDirectory()
        {
            if (Directory.Exists(pathToLocalContentRepository))
            {
                if (CheckForGitSubDir(pathToLocalContentRepository))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public static bool CheckForGitSubDir(string pathToRep)
        {
            if (Directory.Exists(Path.Combine(pathToRep, ".git")))
                return true;
            else
                return false;
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
