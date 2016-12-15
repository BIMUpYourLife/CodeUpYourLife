using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.CutOut.PfV
{
    public interface IPfVModel
    {
        void ModelSave(string hostmodel);

        void ModelLoad(string hostmodel);

        PfVModelData ActualModel { get; }

        void UpdateModel(PfVModelData data, string hostmodell);
    }
}
