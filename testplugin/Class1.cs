using PluginLoader.Attribute;
using PluginLoader.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace testplugin
{
    [Plugin]
    public class Class1:MarshalByRefObject,ITask
    {
        public void print(string msg,Action dosth)
        {
            Console.WriteLine("插件1号开始执行");
            Console.WriteLine(msg);
            Console.WriteLine();
            for (int i = 0; i < 15; i++)
            {
                Console.Write("x");
                Thread.Sleep(1000);
            }
            Console.WriteLine("执行回调");
            dosth();
            Console.WriteLine("执行完毕");
        }
    }
}
