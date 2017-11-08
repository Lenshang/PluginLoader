using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoader.Test
{
    public interface ITask
    {
        void print(string msg,Action dosth);
    }
}
