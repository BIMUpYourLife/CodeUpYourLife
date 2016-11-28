using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.CutOut.PfV
{
    public interface IPresenter
    {
        void ConnectView(IPfVView view);
        void ZoomToPfV(PfVElementData pfvData);
    }
}
