using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BUYLRevitAddin.Loader
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

        public static void BUYLStartNewDECHProject(UIApplication app)
        {
            if (BUYLTools.ContentLoader.GitContentLoader.CheckForContentRepositoryDirectory())
            {
                if (app != null)
                {
                    if (File.Exists(BUYLTools.ContentLoader.GitContentLoader.GetDECHTemplateFile()))
                    {
                        Document doc = app.Application.NewProjectDocument(BUYLTools.ContentLoader.GitContentLoader.GetDECHTemplateFile());
                        if(doc != null)
                        {
                            SaveFileDialog dlg = new SaveFileDialog();
                            dlg.Filter = "Revit project files (*.rvt)|*.rvt";
                            if(dlg.ShowDialog() == DialogResult.OK)
                            {
                                doc.SaveAs(dlg.FileName);
                                app.OpenAndActivateDocument(dlg.FileName);
                            }
                        }
                    }
                    else
                        MessageBox.Show("Templatefile {0} not found!", BUYLTools.ContentLoader.GitContentLoader.GetDECHTemplateFile());
                }
            }
        }
    }
}
