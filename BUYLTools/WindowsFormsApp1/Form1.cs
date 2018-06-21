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
        const string urlContentRepository = "https://github.com/BIMUpYourLife/DynamoUpYourLife/zipball/master";
        public const string zipDirectory = "C:\\Users\\build\\Documents\\BimTestDownload\\temp\\";
        public const string contentPath = "C:\\Users\\build\\Documents\\BimTestDownload\\content\\";
        //public const string contentTempPath = "C:\\Users\\build\\Documents\\BimTestDownload\\tempcontent";
        public const string zipName = "repo.zip";
        public const string zipFullPath = zipDirectory + zipName;

        public Form1()
        {
            InitializeComponent();
            System.IO.Directory.CreateDirectory(zipDirectory);
            System.IO.Directory.CreateDirectory(contentPath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var client = new WebClient())
            {
                // used to display debug messages during the function call
                logMessages.Clear();
           
                // clear directory for the zip contents to avoid exceptions while writing files
                if (Directory.Exists(contentPath))
                {   
                    // directory at location contentPath will be wiped!
                    Directory.Delete(contentPath, true);
                }
                
                // directory for the zip file doesn't need to be cleared because the zip manager automatically overwrites it
                System.IO.Directory.CreateDirectory(zipDirectory);
                System.IO.Directory.CreateDirectory(contentPath);

                // Security protocol needs to be changed to this or the zip download from github will fail
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                logMessages.Text += "Downloading file...\n";

                // Download file from github
                client.DownloadFile(urlContentRepository, zipFullPath);

                logMessages.Text += "Download complete.\n";
                logMessages.Text += "Unzipping file...\n";

                // Zip file from GitHub contains a folder containing the repository data
                // We don't need the folder, so all contents of that folder have to be moved in the parent directory
                ZipFile.ExtractToDirectory(zipFullPath, contentPath);
                
                // subdirectory contained in zip
                string[] directories = Directory.GetDirectories(contentPath);

                if(directories != null && directories.Count() > 0)
                {
                    // The zip only contains one directory which contains all repository data
                    string dir = directories[0];

                    // Get all files and directories contained in the repository
                    string[] files = Directory.GetFiles(dir);
                    string[] subdirectories = Directory.GetDirectories(dir);

                    // Move all subdirectories
                    foreach (string subdirectory in subdirectories)
                    {
                        string[] path = subdirectory.Split('\\');
                        string dirName = path.Last();
                        Directory.Move(subdirectory, contentPath + "//" + dirName);
                    }
                  
                    // Move all files
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        File.Move(file, contentPath + fileName);
                    }

                    // Delete now empty directory
                    Directory.Delete(dir);
                }

                logMessages.Text += "Unzip complete.\n";
            }
        }
    }
}
