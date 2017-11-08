using PluginLoader.Attribute;
using PluginLoader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoader
{
    public class PluginProxy : MarshalByRefObject
    {
        private object locker = new object();
        private Action<PluginProxy> CloseProxy { get; set; }
        /// <summary>
        /// 依赖项DLL所在文件夹
        /// </summary>
        public DirectoryInfo DependentDllPath { get; set; }
        /// <summary>
        /// 使用中的数量
        /// </summary>
        public int UseCount { get; set; } = 0;
        /// <summary>
        /// 已加载的插件信息
        /// </summary>
        public List<PluginFileInfo> LoadedPlugin { get; set; } = new List<PluginFileInfo>();

        public void SetCloseProxyAction(Action<PluginProxy> act)
        {
            CloseProxy = act;
        }
        /// <summary>
        /// 读取一个插件文件 并尝试实例化，读取失败返回null
        /// </summary>
        /// <param name="file"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public T ReadPlugin<T>(FileInfo file)
        {
            PluginFileInfo pinfo = new PluginFileInfo()
            {
                PluginName = file.Name,
                PluginPath = file.FullName,
                CreationTime = file.CreationTime,
                LastAccessTime = file.LastAccessTime,
                LastWriteTime = file.LastWriteTime
            };
            try
            {
                var _pinfo = this.LoadedPlugin.Where(i => i.PluginName == pinfo.PluginName).FirstOrDefault();
                if (_pinfo != null)
                {
                    lock (locker)
                    {
                        UseCount++;
                    }
                    return (T)GetPluginClass(_pinfo.Assembly);
                }

                byte[] b = File.ReadAllBytes(file.FullName);
                Assembly asm = Assembly.Load(b);
                var obj = (T)GetPluginClass(asm);
                pinfo.Assembly = asm;
                this.LoadedPlugin.Add(pinfo);
                lock (locker)
                {
                    UseCount++;
                }
                return (T)obj;
            }
            catch(Exception ex)
            {
                return default(T);
            }
        }
        /// <summary>
        /// 当插件使用完成后,代理中是否还有运行中的插件？是则为True否则位False
        /// </summary>
        /// <returns></returns>
        public bool IsAllPluginUseOver()
        {
            lock (locker)
            {
                UseCount--;
            }
            if (UseCount <= 0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获得拥有指定特性的类
        /// </summary>
        /// <returns></returns>
        private Object GetPluginClass(Assembly ass)
        {
            foreach(var type in ass.GetTypes())
            {
                if (type.GetCustomAttribute<PluginAttribute>() != null)//默认返回第一个
                {
                    return ass.CreateInstance(type.FullName);
                }
            }
            return null;
        }
        /// <summary>
        /// 获得已加载的程序集信息
        /// </summary>
        /// <returns></returns>
        public string[] GetAssembliesName()
        {
            List<string> result = new List<string>();
            foreach(var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                result.Add(item.FullName);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 获得已加载插件文件信息
        /// </summary>
        /// <returns></returns>
        public string[] GetLoadPluginName()
        {
            return LoadedPlugin.Select(i => i.PluginName).ToArray();
        }

        /// <summary>
        /// 获得最后一个创建的插件时间
        /// </summary>
        /// <returns></returns>
        public DateTime? GetLastCreatePluginDate()
        {
            return LoadedPlugin.OrderByDescending(i => i.CreationTime).FirstOrDefault()?.CreationTime;
        }

        internal AppDomain GetAppDoamin()
        {
            return AppDomain.CurrentDomain;
        }

        public void Version()
        {
            Console.WriteLine("1.0");
        }
    }
}
