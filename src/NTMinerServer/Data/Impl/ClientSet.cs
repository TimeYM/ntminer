﻿using LiteDB;
using NTMiner.ServiceContracts.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NTMiner.Data.Impl {
    public class ClientSet : IClientSet {
        // 内存中保留20分钟内活跃的客户端
        private readonly Dictionary<Guid, ClientData> _dicById = new Dictionary<Guid, ClientData>();

        private readonly IHostRoot _root;
        internal ClientSet(IHostRoot root) {
            _root = root;
            Global.Access<Per10SecondEvent>(
                Guid.Parse("ea795e07-7f4b-4284-aa72-aa00c17c89d8"),
                "周期性将内存中的ClientData列表刷入磁盘",
                LogEnum.Console,
                action: message => {
                    InitOnece();
                    lock (_locker) {
                        DateTime time = message.Timestamp.AddMinutes(-20);
                        List<Guid> toRemoves = _dicById.Where(a => a.Value.ModifiedOn < time).Select(a => a.Key).ToList();
                        foreach (var clientId in toRemoves) {
                            _dicById.Remove(clientId);
                        }
                        using (LiteDatabase db = HostRoot.CreateReportDb()) {
                            var col = db.GetCollection<ClientData>();
                            col.Upsert(_dicById.Values);
                        }
                    }
                });
        }

        private bool _isInited = false;
        private object _locker = new object();

        private void InitOnece() {
            if (_isInited) {
                return;
            }
            Init();
        }

        private void Init() {
            lock (_locker) {
                if (!_isInited) {
                    using (LiteDatabase db = HostRoot.CreateReportDb()) {
                        var col = db.GetCollection<ClientData>();
                        DateTime time = DateTime.Now.AddMinutes(-20);
                        foreach (var item in col.Find(Query.GT(nameof(ClientData.ModifiedOn), time))) {
                            _dicById.Add(item.Id, item);
                        }
                    }
                    _isInited = true;
                }
            }
        }

        public int OnlineCount {
            get {
                InitOnece();
                lock (_locker) {
                    DateTime time = DateTime.Now.AddSeconds(-140);
                    return _dicById.Values.Count(a => a.ModifiedOn > time);
                }
            }
        }

        public int MiningCount {
            get {
                InitOnece();
                lock (_locker) {
                    DateTime time = DateTime.Now.AddSeconds(-140);
                    return _dicById.Values.Count(a => a.ModifiedOn > time && a.IsMining);
                }
            }
        }

        public int CountMainCoinOnline(string coinCode) {
            InitOnece();
            lock (_locker) {
                DateTime time = DateTime.Now.AddSeconds(-140);
                return _dicById.Values.Where(a => a.MainCoinCode == coinCode).Count(a => a.ModifiedOn > time);
            }
        }

        public int CountMainCoinMining(string coinCode) {
            InitOnece();
            lock (_locker) {
                DateTime time = DateTime.Now.AddSeconds(-140);
                return _dicById.Values.Where(a => a.MainCoinCode == coinCode).Count(a => a.ModifiedOn > time && a.IsMining);
            }
        }

        public int CountDualCoinOnline(string coinCode) {
            InitOnece();
            lock (_locker) {
                DateTime time = DateTime.Now.AddSeconds(-140);
                return _dicById.Values.Where(a => a.DualCoinCode == coinCode).Count(a => a.ModifiedOn > time);
            }
        }

        public int CountDualCoinMining(string coinCode) {
            InitOnece();
            lock (_locker) {
                DateTime time = DateTime.Now.AddSeconds(-140);
                return _dicById.Values.Where(a => a.DualCoinCode == coinCode).Count(a => a.ModifiedOn > time && a.IsMining);
            }
        }

        public void Add(ClientData clientData) {
            InitOnece();
            if (!_dicById.ContainsKey(clientData.Id)) {
                lock (_locker) {
                    if (!_dicById.ContainsKey(clientData.Id)) {
                        _dicById.Add(clientData.Id, clientData);
                    }
                }
            }
        }

        public List<ClientData> QueryClients(
            int pageIndex,
            int pageSize,
            Guid? mineWorkId,
            string minerIp,
            string minerName,
            MineStatus mineState,
            string mainCoin,
            string mainCoinPool,
            string mainCoinWallet,
            string dualCoin,
            string dualCoinPool,
            string dualCoinWallet,
            out int total) {
            InitOnece();
            lock (_locker) {
                IQueryable<ClientData> query = _dicById.Values.AsQueryable();
                if (mineWorkId != null && mineWorkId.Value != Guid.Empty) {
                    query = query.Where(a => a.WorkId == mineWorkId.Value);
                }
                else {
                    if (mineWorkId != null) {
                        query = query.Where(a => a.WorkId == mineWorkId.Value);
                    }
                    if (!string.IsNullOrEmpty(mainCoin)) {
                        query = query.Where(a => a.MainCoinCode == mainCoin);
                    }
                    if (!string.IsNullOrEmpty(mainCoinPool)) {
                        query = query.Where(a => a.MainCoinPool == mainCoinPool);
                    }
                    if (!string.IsNullOrEmpty(dualCoin)) {
                        if (dualCoin == "*") {
                            query = query.Where(a => a.IsDualCoinEnabled);
                        }
                        else {
                            query = query.Where(a => a.DualCoinCode == dualCoin);
                        }
                    }
                    if (!string.IsNullOrEmpty(dualCoinPool)) {
                        query = query.Where(a => a.DualCoinPool == dualCoinPool);
                    }
                    if (!string.IsNullOrEmpty(mainCoinWallet)) {
                        query = query.Where(a => a.MainCoinWallet == mainCoinWallet);
                    }
                    if (!string.IsNullOrEmpty(dualCoinWallet)) {
                        query = query.Where(a => a.DualCoinWallet == dualCoinWallet);
                    }
                }
                if (!string.IsNullOrEmpty(minerIp)) {
                    query = query.Where(a => a.MinerIp == minerIp);
                }
                if (!string.IsNullOrEmpty(minerName)) {
                    query = query.Where(a => a.MinerName.IndexOf(minerName, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                if (mineState != MineStatus.All) {
                    if (mineState == MineStatus.Mining) {
                        query = query.Where(a => a.IsMining == true);
                    }
                    else {
                        query = query.Where(a => a.IsMining == false);
                    }
                }
                total = query.Count();
                return query.OrderBy(a => a.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        public List<ClientData> LoadClients(IEnumerable<Guid> clientIds) {
            InitOnece();
            List<ClientData> results = new List<ClientData>();
            foreach (var clientId in clientIds) {
                ClientData clientData = LoadClient(clientId);
                if (clientData != null) {
                    results.Add(clientData);
                }
            }
            return results;
        }

        public ClientData LoadClient(Guid clientId) {
            InitOnece();
            ClientData clientData = null;
            lock (_locker) {
                if (_dicById.TryGetValue(clientId, out clientData)) {
                    return clientData;
                }
            }
            using (LiteDatabase db = HostRoot.CreateReportDb()) {
                var col = db.GetCollection<ClientData>();
                clientData = col.FindById(clientId);
                if (clientData != null) {
                    Add(clientData);
                }
                return clientData;
            }
        }

        public void UpdateClient(Guid clientId, string propertyName, object value) {
            InitOnece();
            ClientData clientData = LoadClient(clientId);
            if (clientData != null) {
                PropertyInfo propertyInfo = typeof(ClientData).GetProperty(propertyName);
                if (propertyInfo != null) {
                    if (propertyInfo.PropertyType == typeof(Guid)) {
                        value = DictionaryExtensions.ConvertToGuid(value);
                    }
                    propertyInfo.SetValue(clientData, value, null);
                    clientData.ModifiedOn = DateTime.Now;
                }
            }
        }
    }
}
