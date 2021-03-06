﻿namespace NTMiner.Vms {
    public class MinerProfileIndexViewModel : ViewModelBase {
        public static readonly MinerProfileIndexViewModel Current = new MinerProfileIndexViewModel();

        private MinerProfileIndexViewModel() {
            if (NTMinerRoot.IsInDesignMode) {
                return;
            }
        }

        public MinerProfileViewModel MinerProfile {
            get {
                return MinerProfileViewModel.Current;
            }
        }
    }
}
