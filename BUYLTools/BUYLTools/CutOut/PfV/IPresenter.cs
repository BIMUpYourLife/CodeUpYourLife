using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.CutOut.PfV
{
    public interface IPresenter
    {
        PfVModelData CurrentModel { get; }

        string CurrentHostDocument { get; set; }

        void ConnectView(IPfVView view);

        void PfVPrevious();

        void PfVNext();

        void PfVZoomToDummy();

        void PfVPlaceCurrent();

        void CurrentPfVSet(string linkedFile, int idLinked);

        PfVElementData CurrentPfVGet();

        void CurrentLinkSet(string linkedFile);
    }
}
