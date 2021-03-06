﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.Vms {
    public class VirtualMemory {
        public static readonly VirtualMemory Empty = new VirtualMemory {
            DriveName = string.Empty,
            MinSizeMb = 0,
            MaxSizeMb = 0
        };
        private static readonly Dictionary<string, VirtualMemory> _initialVms = new Dictionary<string, VirtualMemory>();
        public static readonly Dictionary<string, VirtualMemory> VirtualMemories = new Dictionary<string, VirtualMemory>(StringComparer.OrdinalIgnoreCase);

        static VirtualMemory() {
            foreach (var item in GetPagingFiles()) {
                _initialVms.Add(item.DriveName, item);
            }
            foreach (var item in GetPagingFiles()) {
                VirtualMemories.Add(item.DriveName, item);
            }
        }

        public static void RefreshVirtualMemories() {
            VirtualMemories.Clear();
            foreach (var item in GetPagingFiles()) {
                VirtualMemories.Add(item.DriveName, item);
            }
        }

        public static bool IsStateChanged {
            get {
                foreach (var item in _initialVms) {
                    if (!VirtualMemories.ContainsKey(item.Key)) {
                        return true;
                    }
                }
                foreach (var item in VirtualMemories) {
                    if (!_initialVms.ContainsKey(item.Key)) {
                        return true;
                    }
                }
                foreach (var item in _initialVms) {
                    if (VirtualMemories[item.Key].MaxSizeB != item.Value.MaxSizeB) {
                        return true;
                    }
                }
                return false;
            }
        }

        private VirtualMemory() {

        }

        public string DriveName { get; private set; }
        public int MinSizeMb { get; private set; }
        public int MaxSizeMb { get; private set; }

        public long MaxSizeB {
            get {
                return MaxSizeMb * ((long)1024 * 1024);
            }
        }

        public int MaxSizeGb {
            get {
                return MaxSizeMb / 1024;
            }
        }

        public string MaxSizeGbText {
            get {
                return MaxSizeGb + " G"; ;
            }
        }

        public bool IsNotZero {
            get {
                return MaxSizeMb > 0;
            }
        }

        public override string ToString() {
            return $"{DriveName}pagefile.sys  {MinSizeMb} {MaxSizeMb}";
        }

        private const string MemoryManagementSubKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
        public static void SetVirtualMemoryOfDrive(Drive drive, int sizeMb) {
            List<VirtualMemory> list = GetPagingFiles();
            VirtualMemory exist = list.FirstOrDefault(a => a.DriveName == drive.Name);
            if (exist != null) {
                exist.MinSizeMb = sizeMb;
                exist.MaxSizeMb = sizeMb;
            }
            else {
                list.Add(new VirtualMemory() {
                    DriveName = drive.Name,
                    MinSizeMb = sizeMb,
                    MaxSizeMb = sizeMb
                });
            }
            string[] value = list.Select(a => a.ToString()).ToArray();

            Windows.Registry.SetValue(Microsoft.Win32.Registry.LocalMachine, MemoryManagementSubKey, "PagingFiles", value);
            DriveSet.Current.Refresh();
            DrivesViewModel.Current.IsNeedRestartWindows = VirtualMemory.IsStateChanged;
            drive.OptionalVirtualMemories.OnPropertyChanged(nameof(drive.OptionalVirtualMemories.List));
        }

        public static void ClearVirtualMemory(Drive drive) {
            List<VirtualMemory> list = GetPagingFiles();
            VirtualMemory exist = list.FirstOrDefault(a => a.DriveName == drive.Name);
            if (exist == null) {
                return;
            }
            list.Remove(exist);
            string[] value = list.Select(a => a.ToString()).ToArray();

            Windows.Registry.SetValue(Microsoft.Win32.Registry.LocalMachine, MemoryManagementSubKey, "PagingFiles", value);
            DriveSet.Current.Refresh();
            DrivesViewModel.Current.IsNeedRestartWindows = VirtualMemory.IsStateChanged;
            drive.OptionalVirtualMemories.OnPropertyChanged(nameof(drive.OptionalVirtualMemories.List));
        }

        private static List<VirtualMemory> GetPagingFiles() {
            object value = Windows.Registry.GetValue(Microsoft.Win32.Registry.LocalMachine, MemoryManagementSubKey, "PagingFiles");
            // REG_SZ or REG_MULTI_SZ
            List<VirtualMemory> list;
            if (value is string[]) {
                list = Parse((string[])value);
            }
            else {
                list = new List<VirtualMemory>();
            }
            return list;
        }

        private static VirtualMemory Parse(string vmReg) {
            string driveName;
            int minsize = 0;
            int maxsize = 0;
            try {
                string[] strarr = vmReg.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (strarr.Length == 3) {
                    driveName = strarr[0].Substring(0, 3);
                    minsize = Convert.ToInt32(strarr[1]);
                    maxsize = Convert.ToInt32(strarr[2]);
                    return new VirtualMemory {
                        DriveName = driveName,
                        MinSizeMb = minsize,
                        MaxSizeMb = maxsize
                    };
                }
                return null;
            }
            catch (Exception e) {
                Global.Logger.Error(e);
                return null;
            }
        }

        private static List<VirtualMemory> Parse(string[] vmReg) {
            List<VirtualMemory> list = new List<VirtualMemory>();
            try {
                foreach (string item in vmReg) {
                    VirtualMemory vm = Parse(item);
                    if (vm != null) {
                        list.Add(vm);
                    }
                }
            }
            catch (Exception e) {
                Global.Logger.Error(e);
            }
            return list;
        }
    }
}
