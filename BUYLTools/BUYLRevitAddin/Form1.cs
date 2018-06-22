using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.IO.Compression;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // Path of the github repository. Addition of "/zipball/master" to the url allows us to download the zip directly.
        const string urlContentRepository = BUYLTools.ContentLoader.GitContentLoader.urlToRepositoryZip;
        // Path where the repository contents will be saved - will be wiped before download!
        public const string contentPath = BUYLTools.ContentLoader.GitContentLoader.pathToLocalContentRepository;
        // Path where temporary download data will be stored - will be wiped after execution!
        public const string tempPath = BUYLTools.ContentLoader.GitContentLoader.tempFilePath;

        // Paths for temporary folders
        public const string contentPathTemp = tempPath + "temp_content";
        public const string zipPathTemp = tempPath + "temp_zip\\";

        // Name and path for the temporarily stored zip file
        public const string zipName = "repo.zip";
        public const string zipFullPath = zipPathTemp + zipName;

        // Helper for updating the progress bar
        int oldProgress = 0;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when the download button is clicked. Starts the download process from github and unpacks and stores the repository files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // Used to display debug messages during the function call
            logMessages.Clear();

            // Setup folder structure for temporary files
            FolderSetup();
            
            using (var client = new WebClient())
            {
                // Security protocol needs to be changed to this or the zip download from github will fail
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                
                logMessages.Text += "Downloading file...\n";

                // Reset download progress counter
                oldProgress = 0;

                // Download file from github using async
                client.DownloadFileAsync(new Uri(urlContentRepository), zipFullPath);
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            }
        }

        /// <summary>
        /// Updates the download progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            // check if the progress has changes to avoid overloading the ui with too many updates
            if (e.ProgressPercentage != oldProgress)
            {
                downloadProgress.Value = e.ProgressPercentage;
                oldProgress = e.ProgressPercentage;
            }
        }

        /// <summary>
        /// Called when the asynchronous download is completed.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        public void Completed(object o, AsyncCompletedEventArgs args)
        {
            logMessages.Text += "Download complete.\n";

            // Unpack the zip and move the files to the right folder
            UnpackAndMove();
        }

        /// <summary>
        /// Unpacks the zip file and moves all contents to the right folder
        /// </summary>
        public void UnpackAndMove()
        {
            logMessages.Text += "Unzipping file...\n";

            // Zip file from GitHub contains a folder containing the repository data
            // We don't need the folder, so all contents of that folder have to be moved in the parent directory
            try
            {
                ZipFile.ExtractToDirectory(zipFullPath, contentPathTemp);

                // Subdirectories contained in zip
                string[] directories = Directory.GetDirectories(contentPathTemp);
                logMessages.Text += "Unzip complete.\n";

                logMessages.Text += "Cleanup of temporary files.\n";
                if (directories != null && directories.Count() > 0)
                {
                    // The zip only contains one directory which contains all repository data
                    string dir = directories[0];

                    // Copy unpacked files to chosen location
                    Directory.Move(dir, contentPath);

                    // Clear temporary files and folders
                    Directory.Delete(tempPath, true);
                }
                logMessages.Text += "Done.\n";
            }
            // When the zip file can't be accessed something with the download went wrong
            // Tested scenarios where this exception is thrown: No internet connection; Invalid repository url.
            catch (InvalidDataException e)
            {
                // Display error message
                logMessages.Text += "Error while downloading or accessing the file. Please check  your internet connection or try again later.";
                // Cleanup unneeded temporary directories
                Directory.Delete(tempPath, true);
            }
        }

        /// <summary>
        /// Clears all old data from the content and temporary folder. Creates new temporary folders for the zip file and unpacked data.
        /// </summary>
        public void FolderSetup()
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
    }
}
