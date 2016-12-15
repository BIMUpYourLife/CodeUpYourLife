using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.CutOut.PfV
{
    public interface IPresenter
    {
        PfVModelData GetCurrentData { get; }

        void ConnectView(IPfVView view);

        void PfVZoomToCurrent();

        void PfVPlaceCurrent();

        void CurrentPfVSet(string linkedFile, int idLinked);

        PfVElementData CurrentPfVGet();
    }
}
