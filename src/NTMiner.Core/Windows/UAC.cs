﻿using System;
using System.Linq;

namespace NTMiner.Windows {
    public static class UAC {
        public static bool DisableUAC() {
            try {
                const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
                string[] names = { "ConsentPromptBehaviorAdmin", "EnableLUA", "PromptOnSecureDesktop" };
                if (names.All(a => (int)Registry.GetValue(Microsoft.Win32.Registry.LocalMachine, subKey, a) == 0)) {
                    Global.DebugLine("UAC已经处于禁用状态，无需再次禁用", ConsoleColor.Green);
                    return true;
                }
                else {
                    if (!Windows.Role.IsAdministrator) {
                        Global.DebugLine("禁用UAC失败，因为不是管理员", ConsoleColor.Red);
                        return false;
                    }
                    foreach (var name in names) {
                        Registry.SetValue(Microsoft.Win32.Registry.LocalMachine, subKey, name, 0);
                    }
                    return true;
                }
            }
            catch (Exception e) {
                Global.Logger.Error("禁用UAC失败，因为异常", e);
                return false;
            }
        }
    }
}
