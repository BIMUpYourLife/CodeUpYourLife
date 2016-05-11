using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static void BUYLStartNewDECHProject(Autodesk.Revit.ApplicationServices.Application app)
        {
            if (BUYLTools.ContentLoader.GitContentLoader.CheckForContentRepositoryDirectory())
            {
                if (app != null)
                {
                    bool contained = false;

                    if (!contained)
                    {
                        if (File.Exists(BUYLTools.ContentLoader.GitContentLoader.GetDETemplateFile()))
                            app.NewProjectDocument(BUYLTools.ContentLoader.GitContentLoader.GetDETemplateFile());
                    }
                }
            }
        }
    }
}
