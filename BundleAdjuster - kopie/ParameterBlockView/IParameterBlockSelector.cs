using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ceresdotnet;

namespace BundleAdjuster.ParameterBlockView
{
    public delegate void ParamblockSelected(ICeresParameterblock block);
    public interface IParameterBlockSelector {
        event ParamblockSelected ParameterblockSelected;
    }
}
