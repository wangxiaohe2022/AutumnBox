﻿/* =============================================================================*\
*
* Filename: MainWindow.FlowFinishedHandling
* Description: 
*
* Version: 1.0
* Created: 2017/11/25 1:29:42 (UTC+8:00)
* Compiler: Visual Studio 2017
* 
* Author: zsh2401
* Company: I am free man
*
\* =============================================================================*/
using AutumnBox.Basic.Flows;
using AutumnBox.GUI.Helper;
using AutumnBox.GUI.Windows;
using AutumnBox.Basic.Flows.Result;
using AutumnBox.Basic.FlowFramework;
using System.Threading.Tasks;
using AutumnBox.Support.CstmDebug;

namespace AutumnBox.GUI
{
    partial class MainWindow
    {
        public void FlowFinished(object sender, FinishedEventArgs<FlowResult> e)
        {
            this.Dispatcher.Invoke(() =>
            {
                switch (sender.GetType().Name)
                {
                    case nameof(IslandActivator):
                    case nameof(IceBoxActivator):
                    case nameof(AirForzenActivator):
                        DevicesOwnerSetted((DeviceOwnerSetter)sender, (DeviceOwnerSetterResult)e.Result);
                        break;
                    case nameof(FilePusher):
                        PushFinished((AdvanceResult)e.Result);
                        break;
                    default:
                        new FlowResultWindow(e.Result).ShowDialog();
                        break;
                }
            });
        }
        public void PushFinished(AdvanceResult result)
        {
            Logger.D("Enter the Push Finish Handler in the GUI");
            if (result.ResultType == ResultType.Successful)
            {
                BoxHelper.ShowMessageDialog("Notice", "msgPushOK");
            }
            else
            {
                new FlowResultWindow(result).ShowDialog();
            }
        }
        private void DevicesOwnerSetted(DeviceOwnerSetter tor, DeviceOwnerSetterResult result)
        {
            string message = null;
            string advise = null;
            switch (result.ErrorType)
            {
                case Basic.Flows.States.DeviceOwnerSetterErrType.None:
                    break;
                case Basic.Flows.States.DeviceOwnerSetterErrType.DeviceOwnerIsAlreadySet:
                    message = UIHelper.GetString("rmsgDeviceOwnerIsAlreadySet");
                    advise = UIHelper.GetString("advsDeviceOwnerIsAlreadySet");
                    break;
                case Basic.Flows.States.DeviceOwnerSetterErrType.ServalAccountsOnTheDevice:
                    message = UIHelper.GetString("rmsgAlreadyServalAccountsOnTheDevice");
                    advise = UIHelper.GetString("advsAlreadyServalAccountsOnTheDevice");
                    break;
                case Basic.Flows.States.DeviceOwnerSetterErrType.ServalUserOnTheDevice:
                    message = UIHelper.GetString("rmsgAlreadyServalUsersOnTheDevice");
                    advise = UIHelper.GetString("advsAlreadyServalUsersOnTheDevice");
                    break;
                case Basic.Flows.States.DeviceOwnerSetterErrType.UnknowAdmin:
                    message = UIHelper.GetString("rmsgAppHaveNoInstalled");
                    advise = UIHelper.GetString("advsAppHaveNoInstalled");
                    break;
                default:
                    message = UIHelper.GetString("rmsgUnknowError");
                    advise = UIHelper.GetString("advsUnknowError");
                    break;
            }
            new FlowResultWindow(result, message, advise).ShowDialog();
        }
    }
}
