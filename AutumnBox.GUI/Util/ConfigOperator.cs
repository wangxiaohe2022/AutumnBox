/* =============================================================================*\
*
* Filename: ConfigJson.cs
* Description: 
*
* Version: 1.0
* Created: 9/30/2017 18:32:35(UTC+8:00)
* Compiler: Visual Studio 2017
* 
* Author: zsh2401
* Company: I am free man
*
\* =============================================================================*/
using AutumnBox.Shared.CstmDebug;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace AutumnBox.GUI.Util
{
    [LogPropertyAttribute(Show = false)]
    public class ConfigOperator : IConfigOperator
    {
        public ConfigTemplate Data { get; private set; } = new ConfigTemplate();
        private static readonly string ConfigFileName = "autumnbox.json";
        /// <summary>
        /// 构造器
        /// </summary>
        public ConfigOperator()
        {
            Logger.D(this, "Start Check");
            if (HaveError() || HaveLost())
            {
                Logger.D(this, "Some error checked, init file");
                SaveToDisk();
            }
            Logger.D(this, "Finished Check");
            ReloadFromDisk();
        }
        /// <summary>
        /// 从硬盘重载数据
        /// </summary>
        public void ReloadFromDisk()
        {
            if (HaveError()) SaveToDisk();
            if (!File.Exists(ConfigFileName)) { SaveToDisk(); return; }
            Data = (ConfigTemplate)(JsonConvert.DeserializeObject(File.ReadAllText(ConfigFileName), Data.GetType()));
            Logger.D(this, "Is first launch? " + Data.IsFirstLaunch.ToString());
        }
        /// <summary>
        /// 将数据存储到硬盘
        /// </summary>
        public void SaveToDisk()
        {
            if (!File.Exists(ConfigFileName)) File.Create(ConfigFileName);
            using (StreamWriter sw = new StreamWriter(ConfigFileName, false))
            {
                string text = JsonConvert.SerializeObject(Data);
                Logger.D(this, text);
                sw.Write(text);
                sw.Flush();
            }
        }

        /// <summary>
        /// 检测硬盘上的数据是否有问题
        /// </summary>
        /// <returns>是否有问题</returns>
        private bool HaveError()
        {
            Logger.D(this, "enter error check");
            try
            {
                JObject jObj = JObject.Parse(File.ReadAllText(ConfigFileName)); return false;
            }
            catch (JsonReaderException) { return true; }
            catch (FileNotFoundException) { return true; }
        }
        /// <summary>
        /// 检测配置文件中的项是否有丢失
        /// </summary>
        /// <returns>项是否有丢失</returns>
        private bool HaveLost()
        {
            Logger.D(this, "enter lost check");
            JObject j = JObject.Parse(File.ReadAllText(ConfigFileName));
            Logger.D(this, "read finish");
            foreach (var prop in Data.GetType().GetProperties())
            {
                if (!(prop.IsDefined(typeof(JsonPropertyAttribute)))) continue;
                var attr = (JsonPropertyAttribute)prop.GetCustomAttribute(typeof(JsonPropertyAttribute));
                if (j[attr.PropertyName] == null) { Logger.D(this, "have lost"); return true; };
            }
            Logger.D(this, "no lost");
            return false;
        }
    }
}
