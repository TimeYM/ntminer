﻿using Microsoft.Win32;
using System.Linq;

namespace NTMiner.Windows {
    public static class Registry {
        #region Registry
        public static object GetValue(RegistryKey root, string subkey, string valueName) {
            object registData = "";
            try {
                RegistryKey myKey = root.OpenSubKey(subkey, true);
                if (myKey != null) {
                    registData = myKey.GetValue(valueName);
                }

                return registData;
            }
            catch (System.Exception e) {
                Global.Logger.Error(e.Message, e);
                return registData;
            }
        }

        public static void SetValue(RegistryKey root, string subkey, string valueName, object value) {
            try {
                RegistryKey registryKey = root.CreateSubKey(subkey);
                registryKey.SetValue(valueName, value);
            }
            catch (System.Exception e) {
                Global.Logger.Error(e.Message, e);
            }
        }

        public static void DeleteValue(RegistryKey root, string subkey, string valueName) {
            try {
                RegistryKey myKey = root.OpenSubKey(subkey, true);
                if (myKey != null) {
                    myKey.DeleteValue(valueName, throwOnMissingValue: false);
                }
            }
            catch (System.Exception e) {
                Global.Logger.Error(e.Message, e);
            }
        }

        public static bool IsValueExist(RegistryKey root, string subkey, string valueName) {
            try {
                RegistryKey myKey = root.OpenSubKey(subkey, true);
                string[] valueNames = myKey.GetValueNames();
                return valueNames.Any(a => a == valueName);
            }
            catch (System.Exception e) {
                Global.Logger.Error(e.Message, e);
                return false;
            }
        }
        #endregion
    }
}
