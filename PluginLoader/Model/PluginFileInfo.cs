using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoader.Model
{
    public class PluginFileInfo
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        public string PluginName { get; set; }
        /// <summary>
        /// 插件所在路径
        /// </summary>
        public string PluginPath { get; set; }
        /// <summary>
        /// 插件文件创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// 插件文件最后一次写入时间
        /// </summary>
        public DateTime LastWriteTime { get; set; }
        /// <summary>
        /// 插件文件最后一次访问时间
        /// </summary>
        public DateTime LastAccessTime { get; set; }
        /// <summary>
        /// 插件的MD5特征，用来判断插件是否更新过
        /// </summary>
        public string MD5 { get; set; }
        /// <summary>
        /// 插件程序集对象
        /// </summary>
        public Assembly Assembly { get; set; }
    }
}
