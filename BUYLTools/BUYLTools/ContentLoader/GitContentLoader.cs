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
        
        //const string urlContentRepository = "https://github.com/BIMUpYourLife/ContentUpYourLife.git";
        const string urlContentRepository = "https://github.com/BIMUpYourLife/DynamoUpYourLife.git";
        const string bimUpYourLife = "BIMUpYourLife";
        public const string pathToLocalContentRepository = "c:\\BIMUpYourLife\\ContentUpYourLife";

        const string pathToFamilies = "_Content\\Revit\\_Families";
        public const string buylContent = "BIMUpYourLife Content";

        const string pathToTemplates = "_Content\\Revit\\_Templates";
        const string fileDECHTemplate = "Template_Revit_MEP_VERSIONCHDE.rte";
        const string fileDEDETemplate = "Template_Revit_MEP_VERSIONDEDE.rte";
        const string fileReplacer = "VERSION";

        // Path of the github repository. Addition of "/zipball/master" to the url allows us to download the zip directly.
        //const string urlContentRepository = "https://github.com/BIMUpYourLife/DynamoUpYourLife/zipball/master";
        // Path where the repository contents will be saved - will be wiped before download!
        public const string contentPath = "C:\\Users\\build\\Documents\\BimTestDownload\\";
        // Path where temporary download data will be stored - will be wiped after execution!
        public const string tempPath = "C:\\Users\\build\\Documents\\BimTestDownloadTemp\\";

        // Paths for temporary folders
        public const string contentPathTemp = tempPath + "temp_content";
        public const string zipPathTemp = tempPath + "temp_zip\\";

        // Name and path for the temporarily stored zip file
        public const string zipName = "repo.zip";
        public const string zipFullPath = zipPathTemp + zipName;

        public static async Task BUYLCloneOrUpdateRepository()
        {
            // Used to display debug messages during the function call

            // Setup folder structure for temporary files
            FolderSetup();

            using (var client = new WebClient())
            {
                // Security protocol needs to be changed to this or the zip download from github will fail
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                // Download file from github using async
                client.DownloadFile(urlContentRepository, zipFullPath);
                //client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            }

            UnpackAndMove();
            /*
            if (CheckForContentRepositoryDirectory())
                DeleteContentRepositoryLocally();

            if (!CheckForContentRepositoryDirectory())
            {
                //prg.ShowDialog();
                Directory.CreateDirectory(pathToLocalContentRepository);

                LibGit2Sharp.CloneOptions op = new LibGit2Sharp.CloneOptions();
                op.BranchName = "master";
                op.Checkout = true;
                op.IsBare = false;

                LibGit2Sharp.Repository.Clone(urlContentRepository, pathToLocalContentRepository, op);

                LogManager.ManagerLog.Current.WriteLogMessage(string.Format("Repository cloned to {0}", pathToLocalContentRepository), LogManager.LogState.Info);
                //prg.Close();
            }*/
            //else
            //{
            //    using (var repo = new LibGit2Sharp.Repository(pathToLocalContentRepository))
            //    {
            //        LibGit2Sharp.Remote remote = repo.Network.Remotes["origin"];
            //        LibGit2Sharp.FetchOptions op = new LibGit2Sharp.FetchOptions();
            //        repo.Network.Fetch(remote);
            //        LogManager.ManagerLog.Current.WriteLogMessage(string.Format("Repository update fetched to {0}", pathToLocalContentRepository), LogManager.LogState.Info);
            //    }
            //}
        }


        /// <summary>
        /// Called when the asynchronous download is completed.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        public static void Completed(object o, AsyncCompletedEventArgs args)
        {

            // Unpack the zip and move the files to the right folder
            UnpackAndMove();
        }

        /// <summary>
        /// Unpacks the zip file and moves all contents to the right folder
        /// </summary>
        public static void UnpackAndMove()
        {
            

            // Zip file from GitHub contains a folder containing the repository data
            // We don't need the folder, so all contents of that folder have to be moved in the parent directory
            try
            {
                ZipFile.ExtractToDirectory(zipFullPath, contentPathTemp);

                // Subdirectories contained in zip
                string[] directories = Directory.GetDirectories(contentPathTemp);

                if (directories != null && directories.Count() > 0)
                {
                    // The zip only contains one directory which contains all repository data
                    string dir = directories[0];

                    // Copy unpacked files to chosen location
                    Directory.Move(dir, contentPath);

                    // Clear temporary files and folders
                    Directory.Delete(tempPath, true);
                }
            }
            // When the zip file can't be accessed something with the download went wrong
            // Tested scenarios where this exception is thrown: No internet connection; Invalid repository url.
            catch (InvalidDataException e)
            {
                // Display error message
                // Cleanup unneeded temporary directories
                Directory.Delete(tempPath, true);
            }
        }

        /// <summary>
        /// Clears all old data from the content and temporary folder. Creates new temporary folders for the zip file and unpacked data.
        /// </summary>
        public static void FolderSetup()
        {
            // Clear directory for the zip contents to avoid exceptions while writing files
            if (Directory.Exists(contentPath))
            {
                // Directory at location contentPath will be wiped!
                Directory.Delete(contentPath, true);
            }
            if (Directory.Exists(tempPath))
            {
                // Directory at location contentPath will be wiped!
                Directory.Delete(tempPath, true);
            }

            // Directory for the zip file doesn't need to be cleared because the zip manager automatically overwrites it
            // Directory at location of contentPath will be created during the Directory.Move command
            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(zipPathTemp);
            Directory.CreateDirectory(contentPathTemp);
        }

        private static void DeleteContentRepositoryLocally()
        {
            System.Windows.Forms.MessageBox.Show("The local copy of your repository will be deleted!");
            DirectoryInfo dirContent = new DirectoryInfo(pathToLocalContentRepository);
            dirContent.Delete(true);
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
