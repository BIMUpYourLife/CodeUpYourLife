using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BUYLRevit.Loader
{
    public static class ContentLoader
    {
        public static void BUYLSetContentLibraryPath(Autodesk.Revit.ApplicationServices.Application app)
        {
            if(BUYLTools.ContentLoader.GitContentLoader.CheckForContentRepositoryDirectory())
            {
                if(app != null)
                {
                    bool contained = false;

                    IDictionary<string, string> libs = app.GetLibraryPaths();
                    if(libs.ContainsKey(BUYLTools.ContentLoader.GitContentLoader.buylContent))
                    {
                        contained = true;
                    }
                    else
                    {
                        foreach (string item in libs.Keys)
                        {
                            if(libs[item] == BUYLTools.ContentLoader.GitContentLoader.pathToLocalContentRepository)
                            { contained = true;
                                break;
                            }
                        }
                    }

                    if(!contained)
                    {
                        libs.Add(BUYLTools.ContentLoader.GitContentLoader.buylContent, BUYLTools.ContentLoader.GitContentLoader.GetLibraryPath());
                        app.SetLibraryPaths(libs);
                    }
                }
            }
        }

        public static void BUYLStartNewProject(UIApplication app, BUYLTools.Utils.Countries country)
        {
            if (BUYLTools.ContentLoader.GitContentLoader.CheckForContentRepositoryDirectory())
            {
                if (app != null)
                {
                    Document doc = null;

                    switch (country)
                    {
                        case BUYLTools.Utils.Countries.DECH:
                            if (File.Exists(BUYLTools.ContentLoader.GitContentLoader.GetDECHTemplateFile(app.Application.VersionNumber)))
                                doc = app.Application.NewProjectDocument(BUYLTools.ContentLoader.GitContentLoader.GetDECHTemplateFile(app.Application.VersionNumber));
                            else
                                MessageBox.Show("Templatefile {0} not found!", BUYLTools.ContentLoader.GitContentLoader.GetDECHTemplateFile(app.Application.VersionNumber));
                            break;
                        case BUYLTools.Utils.Countries.DEDE:
                            if (File.Exists(BUYLTools.ContentLoader.GitContentLoader.GetDEDETemplateFile(app.Application.VersionNumber)))
                                doc = app.Application.NewProjectDocument(BUYLTools.ContentLoader.GitContentLoader.GetDEDETemplateFile(app.Application.VersionNumber));
                            else
                                MessageBox.Show("Templatefile {0} not found!", BUYLTools.ContentLoader.GitContentLoader.GetDEDETemplateFile(app.Application.VersionNumber));
                            break;
                        default:
                            doc = null;
                            break;
                    }
                        
                    if (doc != null)
                    {
                        SaveFileDialog dlg = new SaveFileDialog();
                        dlg.Filter = "Revit project files (*.rvt)|*.rvt";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            SaveAsOptions op = new SaveAsOptions() { OverwriteExistingFile = false };
                            if (File.Exists(dlg.FileName))
                            {
                                if (MessageBox.Show("The model alreaddy exists and will be overriden!", "Model Creation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    op.OverwriteExistingFile = true;
                                else
                                    op.OverwriteExistingFile = false;
                            }

                            doc.SaveAs(dlg.FileName, op);
                            app.OpenAndActivateDocument(dlg.FileName);
                        }
                    }
                }
            }
        }
    }
}
