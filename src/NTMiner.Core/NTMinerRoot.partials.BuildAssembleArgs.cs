﻿using NTMiner.Core;
using NTMiner.Core.Kernels;
using NTMiner.ServiceContracts.DataObjects;
using System;
using System.Collections.Generic;

namespace NTMiner {
    public partial class NTMinerRoot : INTMinerRoot {
        public string BuildAssembleArgs() {
            ICoin mainCoin;
            if (!CoinSet.TryGetCoin(this.MinerProfile.CoinId, out mainCoin)) {
                return string.Empty;
            }
            ICoinProfile coinProfile = this.CoinProfileSet.GetCoinProfile(mainCoin.GetId());
            IPool mainCoinPool;
            if (!PoolSet.TryGetPool(coinProfile.PoolId, out mainCoinPool)) {
                return string.Empty;
            }
            ICoinKernel coinKernel;
            if (!CoinKernelSet.TryGetKernel(coinProfile.CoinKernelId, out coinKernel)) {
                return string.Empty;
            }
            IKernel kernel;
            if (!KernelSet.TryGetKernel(coinKernel.KernelId, out kernel)) {
                return string.Empty;
            }
            if (!kernel.IsSupported()) {
                return string.Empty;
            }
            ICoinKernelProfile coinKernelProfile = this.CoinKernelProfileSet.GetCoinKernelProfile(coinProfile.CoinKernelId);
            string wallet = coinProfile.Wallet;
            string pool = mainCoinPool.Server;
            string kernelArgs = kernel.Args;
            string coinKernelArgs = coinKernel.Args;
            string customArgs = coinKernelProfile.CustomArgs;
            var argsDic = new Dictionary<string, string> {
                {"mainCoin", mainCoin.Code },
                {"mainAlgo", mainCoin.Algo },
                {"wallet", wallet },
                {"pool", pool },
                {"worker", this.MinerProfile.MinerName }
            };// 这里不要考虑{logfile}，{logfile}往后推迟
            if (coinKernel.DualCoinGroupId != Guid.Empty && coinKernelProfile.IsDualCoinEnabled) {
                ICoin dualCoin;
                if (this.CoinSet.TryGetCoin(coinKernelProfile.DualCoinId, out dualCoin)) {
                    ICoinProfile dualCoinProfile = this.CoinProfileSet.GetCoinProfile(dualCoin.GetId());
                    IPool dualCoinPool;
                    if (PoolSet.TryGetPool(dualCoinProfile.DualCoinPoolId, out dualCoinPool)) {
                        string dualWallet = dualCoinProfile.DualCoinWallet;
                        string dualPool = dualCoinPool.Server;
                        argsDic.Add("dualCoin", dualCoin.Code);
                        argsDic.Add("dualAlgo", dualCoin.Algo);
                        argsDic.Add("dualWeight", ((int)Math.Round(coinKernelProfile.DualCoinWeight)).ToString());
                        argsDic.Add("dualWallet", dualWallet);
                        argsDic.Add("dualPool", dualPool);

                        kernelArgs = kernel.DualFullArgs;
                        AssembleArgs(argsDic, ref kernelArgs, isDual: true);
                        AssembleArgs(argsDic, ref customArgs, isDual: true);

                        return $"{kernelArgs} {coinKernelArgs} {customArgs}";
                    }
                }
            }
            AssembleArgs(argsDic, ref kernelArgs, isDual: false);
            AssembleArgs(argsDic, ref coinKernelArgs, isDual: false);
            AssembleArgs(argsDic, ref customArgs, isDual: false);

            return $"{kernelArgs} {coinKernelArgs} {customArgs}";
        }

        private static void AssembleArgs(Dictionary<string, string> prms, ref string args, bool isDual) {
            if (string.IsNullOrEmpty(args)) {
                args = string.Empty;
                return;
            }
            args = args.Replace("{mainCoin}", prms["mainCoin"]);
            args = args.Replace("{mainAlgo}", prms["mainAlgo"]);
            args = args.Replace("{wallet}", prms["wallet"]);
            args = args.Replace("{pool}", prms["pool"]);
            args = args.Replace("{worker}", prms["worker"]);
            args = args.Replace("{wallet}", prms["wallet"]);
            if (isDual) {
                args = args.Replace("{dualCoin}", prms["dualCoin"]);
                args = args.Replace("{dualAlgo}", prms["dualAlgo"]);
                args = args.Replace("{dualWeight}", prms["dualWeight"]);
                args = args.Replace("{dualWallet}", prms["dualWallet"]);
                args = args.Replace("{dualPool}", prms["dualPool"]);
            }
            // 这里不要考虑{logfile}，{logfile}往后推迟
        }
    }
}
