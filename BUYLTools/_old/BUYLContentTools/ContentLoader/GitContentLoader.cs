using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        const string fileDECHTemplate = "Template_Revit_MEP_2016CHDE.rte";

        public static void BUYLCloneOrUpdateRepository()
        {
            if (!CheckForContentRepositoryDirectory())
            {
                //prg.ShowDialog();
                Directory.CreateDirectory(pathToLocalContentRepository);

                CloneOptions op = new CloneOptions();
                
                Repository.Clone(urlContentRepository, pathToLocalContentRepository, op);
                //prg.Close();
            }
            else
            {
                using (var repo = new Repository(pathToLocalContentRepository))
                {
                    Remote remote = repo.Network.Remotes["origin"];
                    FetchOptions op = new FetchOptions();
                    repo.Network.Fetch(remote);
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
            return Path.Combine(pathToLocalContentRepository, pathToFamilies);
        }

        public static string GetDETemplateFile()
        {
            return Path.Combine(GetTemplatePath(), fileDECHTemplate);
        }
    }
}
