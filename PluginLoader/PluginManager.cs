using PluginLoader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoader
{
    public class PluginManager:MarshalByRefObject
    {
        private object locker = new object();
        private AppDomain runDomain { get; set; }
        public PluginProxy proxy { get; set; }
        /// <summary>
        /// 已加载的插件信息
        /// </summary>
        public List<PluginFileInfo> LoadedPlugin { get; set; } = new List<PluginFileInfo>();
        /// <summary>
        /// 存放AppDomain缓存
        /// </summary>
        private List<PluginProxy> Areas { get; set; } = new List<PluginProxy>();
        /// <summary>
        /// 缓存所在文件夹
        /// </summary>
        public DirectoryInfo RunCachePath { get; set; } = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

        public PluginManager()
        {
            if(Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginManager")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginManager"));
            }
        }

        /// <summary>
        /// 异步读取一个插件并运行
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task ReadPluginAsync<T>(FileInfo file,Action<T> action)
        {
            PluginFileInfo pinfo = new PluginFileInfo()
            {
                PluginName = file.Name,
                PluginPath = file.FullName,
                CreationTime = file.CreationTime,
                LastAccessTime = file.LastAccessTime,
                LastWriteTime = file.LastWriteTime
            };
            pinfo.MD5 = GetMD5HashFromFile(file.FullName);
            var _pinfo = this.LoadedPlugin.Where(i => i.PluginName == pinfo.PluginName).FirstOrDefault();
            if (_pinfo == null)//插件未读取过
            {
                this.LoadedPlugin.Add(pinfo);
                _pinfo = pinfo;
            }
            T obj;
            lock (locker)
            {
                if (_pinfo.MD5 == pinfo.MD5)//插件未更新过
                {
                    if (Areas.Count == 0)
                    {
                        Areas.Add(this.CreateProxy());
                    }
                }
                else//插件更新过了
                {
                    Areas.Add(this.CreateProxy());
                }

                var proxy = Areas.Last();
                try
                {
                    obj = proxy.ReadPlugin<T>(file);
                }
                catch(Exception ex)
                {
                    obj = default(T);
                    return;
                }
            }

            await Task.Run(() =>
            {
                action(obj);
                lock (locker)
                {
                    try
                    {
                        if (proxy.IsAllPluginUseOver())//如果代理全部运行完则销毁
                        {
                            Areas.Remove(proxy);
                            UninstallProxy(proxy);
                        }
                    }
                    catch
                    {
                        Areas.Remove(proxy);
                        UninstallProxy(proxy);

                    }
                }
            });
        }

        /// <summary>
        /// 创建一个插件读取代理
        /// </summary>
        /// <returns></returns>
        public PluginProxy CreateProxy()
        {
            try
            {
                AppDomainSetup setup = new AppDomainSetup();
                setup.ApplicationBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);//依赖项DLL目录
                setup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                setup.ShadowCopyFiles = "true";
                setup.ShadowCopyDirectories = RunCachePath.FullName;//影像副本目录
                setup.LoaderOptimization = LoaderOptimization.SingleDomain;
                setup.ApplicationName = "Dynamic";
                // Set up the Evidence
                Evidence baseEvidence = AppDomain.CurrentDomain.Evidence;
                Evidence evidence = new Evidence(baseEvidence);
                // Create the AppDomain     
                runDomain = AppDomain.CreateDomain("newDomain", evidence, setup);
                //创建代理类
                proxy = (PluginProxy)runDomain.CreateInstanceFromAndUnwrap(Assembly.GetAssembly(typeof(PluginManager)).Location, "PluginLoader.PluginProxy");
                proxy.SetCloseProxyAction(OnAreaEmpty);
                //proxy.ReadAssembly(new FileInfo(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase+ "PluginLoader.Test.exe"));
                return proxy;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 卸载一个插件代理
        /// </summary>
        /// <returns></returns>
        public void UninstallProxy(PluginProxy proxy)
        {
            AppDomain.Unload(proxy.GetAppDoamin());
        }

        public void OnAreaEmpty(PluginProxy proxy)
        {
            UninstallProxy(proxy);
        }

        /// <summary>
        /// 获得某个文件MD5
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail, error:" +ex.Message);
            }
        }
    }
}
