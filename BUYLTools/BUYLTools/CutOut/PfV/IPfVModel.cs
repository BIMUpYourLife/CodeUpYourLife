using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.CutOut.PfV
{
    public interface IPfVModel
    {
        void ModelSave();
        PfVModelData ModelLoad();
        PfVModelData Model(string hostmodel);
    }
}
