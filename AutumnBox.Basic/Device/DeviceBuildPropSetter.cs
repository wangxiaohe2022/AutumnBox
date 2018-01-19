﻿/********************************************************************************
** auth： zsh2401@163.com
** date： 2018/1/12 23:45:05
** filename: DeviceBuildPropSetter.cs
** compiler: Visual Studio 2017
** desc： ...
*********************************************************************************/
using AutumnBox.Basic.Executer;
using AutumnBox.Support.CstmDebug;
using AutumnBox.Support.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutumnBox.Basic.Device
{
    public class DeviceBuildPropSetter : IDisposable, ISetableShell
    {
        private static readonly CommandExecuter executer = new CommandExecuter();
        private AndroidShell shellAsSu;
        public DeviceSerial DeviceSerial { get; private set; }
        public bool AutoSave { get; set; } = true;
        public string CurrentString { get; private set; }
        public AndroidShell ShellAsSu
        {
            private get
            {
                if (shellAsSu == null)
                {
                    shellAsSu = new AndroidShell(DeviceSerial);
                }
                return shellAsSu;
            }
            set
            {
                if (shellAsSu == null)
                {
                    shellAsSu = value;
                }
            }
        }

        private const string buildPropPath = "/system/build.prop";
        private const string buildPropFileNameOnTempFloder = "now_build.prop.tmp";
        private const string buildPropFileNameOnDeviceTemp = "/sdcard/atmb_buildprop_temp";
        public DeviceBuildPropSetter(DeviceSerial serial)
        {
            this.DeviceSerial = serial;
            shellAsSu = new AndroidShell(serial);
            shellAsSu.Connect();
            if (!shellAsSu.Switch2Su())
            {
                throw new DeviceHaveNoRootException();
            }
            ReloadFromDevice();
        }
        /// <summary>
        /// 从设备加载数据
        /// </summary>
        public void ReloadFromDevice()
        {
            CurrentString = shellAsSu.SafetyInput($"cat {buildPropPath}").All.ToString();
        }
        /// <summary>
        /// 将所做的改动保存到设备
        /// </summary>
        public void SaveToDevice()
        {
            using (FileStream fs = TemporaryFilesHelper.GetTempFileStream(buildPropFileNameOnTempFloder))
            {
                using (TextWriter writer = new StreamWriter(fs))
                {
                    writer.Write(CurrentString);
                }
            }
            //push local temp file to device sdcard
            var result= executer.Execute(Command.MakeForAdb(DeviceSerial, $"push {TemporaryFilesHelper.TemporaryFloderName}/{buildPropFileNameOnTempFloder} {buildPropFileNameOnDeviceTemp}"));
            //remount device /system rw
            shellAsSu.SafetyInput($"mount -o rw,remount /system");
            //move temp file on device to build.prop
            ShellAsSu.SafetyInput($"mv {buildPropFileNameOnDeviceTemp} {buildPropPath}");
        }
        public void Dispose()
        {
            shellAsSu.Dispose();
        }
        /// <summary>
        /// 设置Value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            if (HaveThisProperty(key)) {
                CurrentString = Regex.Replace(CurrentString, $"^{key}=.+$", $"{key}={value}", RegexOptions.Multiline);
            }
            {
                CurrentString += $"\n{key}={value}";
            }
            if (AutoSave)
            {
                SaveToDevice();
            }
        }

        public bool HaveThisProperty(string key) {
            return Regex.IsMatch(CurrentString,$"^{key}=.+$",RegexOptions.Multiline);
        }
    }
}
