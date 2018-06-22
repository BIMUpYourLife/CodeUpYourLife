using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Octokit;
using System.IO.Compression;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // Path of the github repository. Addition of "/zipball/master" to the url allows us to download the zip directly.
        const string urlContentRepository = "https://github.com/BIMUpYourLife/DynamoUpYourLife/zipball/master";
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

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Used to display debug messages during the function call
            logMessages.Clear();

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
            using (var client = new WebClient())
            {
                // Security protocol needs to be changed to this or the zip download from github will fail
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                
                logMessages.Text += "Downloading file...\n";

                // Download file from github
                client.DownloadFile(urlContentRepository, zipFullPath);

                logMessages.Text += "Download complete.\n";
            }
            logMessages.Text += "Unzipping file...\n";

            // Zip file from GitHub contains a folder containing the repository data
            // We don't need the folder, so all contents of that folder have to be moved in the parent directory
            ZipFile.ExtractToDirectory(zipFullPath, contentPathTemp);

            // Subdirectories contained in zip
            string[] directories = Directory.GetDirectories(contentPathTemp);
            logMessages.Text += "Unzip complete.\n";

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
    }
}
