using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BUYLTools.CutOut.PfV
{
    public interface IPfVView
    {
        void SetPresenter(IPresenter presenter);
        DialogResult ShowPfvDlg();
        void HideDlg();
        void ShowDlg();
    }
}
