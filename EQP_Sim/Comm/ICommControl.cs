using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQP_Sim.Comm
{
    public interface ICommControl
    {
        void Send(string msg);
    }
}
