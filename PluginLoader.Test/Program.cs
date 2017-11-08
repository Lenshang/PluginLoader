using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PluginLoader.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            PluginManager pm = new PluginManager();
            //var proxy = pm.CreateProxy();
            while (true)
            {
                
                Console.WriteLine("开始执行任务");
                FileInfo finfo = new System.IO.FileInfo("plugin/testplugin.dll");
                //var task = proxy.ReadPlugin<ITask>(finfo);
                var task = pm.ReadPluginAsync<ITask>(finfo, i => {
                    i.print("HelloWorld", () => { Console.WriteLine("回调执行完毕"); });

                });
                //task.print("HelloWorld", () =>
                //{
                //    Console.WriteLine("回调执行完毕");
                //});

                //foreach (var name in proxy.GetLoadPluginName())
                //{
                //    Console.WriteLine(name);
                //}
                //pm.UninstallProxy(proxy);//卸载APPDOMAIN释放资源
                Console.WriteLine("等待20秒继续");
                Thread.Sleep(20000);
            }
            
            //proxy.Version();
            //foreach(var item in proxy.GetAssembliesName())
            //{
            //    Console.WriteLine(item);
            //}

            //var obj = proxy.ReadPlugin<ITask>(new System.IO.FileInfo("plugin/testplugin.dll"), "testplugin.Class1");
            //obj.print("abc",()=> { Console.WriteLine("vvv"); });
            //foreach (var item in proxy.GetAssembliesName())
            //{
            //    Console.WriteLine(item);
            //}
            //pm.UninstallProxy(proxy);//卸载APPDOMAIN释放资源

            //foreach(var item in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    Console.WriteLine(item.FullName);
            //}
            Console.ReadLine();
        }
    }
}
