﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class CoinPageViewModel : ViewModelBase {
        public static readonly CoinPageViewModel Current = new CoinPageViewModel();

        private string _coinKeyword;

        public ICommand Add { get; private set; }
        public ICommand ClearKeyword { get; private set; }

        private CoinPageViewModel() {
            this.Add = new DelegateCommand(() =>
            {
                int sortNumber = NTMinerRoot.Current.CoinSet.Count == 0 ? 1 : NTMinerRoot.Current.CoinSet.Max(a => a.SortNumber) + 1;
                new CoinViewModel(Guid.NewGuid()) {
                    SortNumber = sortNumber
                }.Edit.Execute(null);
            });
            this.ClearKeyword = new DelegateCommand(() =>
            {
                this.CoinKeyword = string.Empty;
            });
            CoinViewModels.Current.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == nameof(CoinViewModels.Current.AllCoins)) {
                    OnPropertyChanged(nameof(List));
                }
            };
        }

        public MinerProfileViewModel MinerProfile {
            get {
                return MinerProfileViewModel.Current;
            }
        }

        #region coin

        public string CoinKeyword {
            get => _coinKeyword;
            set {
                _coinKeyword = value;
                OnPropertyChanged(nameof(CoinKeyword));
                OnPropertyChanged(nameof(List));
            }
        }

        private CoinViewModel _currentCoin;
        public CoinViewModel CurrentCoin {
            get { return _currentCoin; }
            set {
                _currentCoin = value;
                OnPropertyChanged(nameof(CurrentCoin));
            }
        }

        public List<CoinViewModel> List {
            get {
                List<CoinViewModel> list;
                if (!string.IsNullOrEmpty(CoinKeyword)) {
                    string keyword = this.CoinKeyword.ToLower();
                    list = CoinViewModels.Current.AllCoins.
                        Where(a => (!string.IsNullOrEmpty(a.Code) && a.Code.ToLower().Contains(keyword))
                            || (!string.IsNullOrEmpty(a.Algo) && a.Algo.ToLower().Contains(keyword))
                            || (!string.IsNullOrEmpty(a.EnName) && a.EnName.ToLower().Contains(keyword))).ToList();
                }
                else {
                    list = CoinViewModels.Current.AllCoins;
                }
                if (list.Count == 1) {
                    CurrentCoin = list.FirstOrDefault();
                }
                if (CurrentCoin == null) {
                    CurrentCoin = list.FirstOrDefault();
                }
                else {
                    CurrentCoin = list.FirstOrDefault(a => a.Id == CurrentCoin.Id);
                }
                return list.ToList();
            }
        }
        #endregion

        private bool _isPoolTabSelected;
        private bool _isWalletTabSelected;
        private bool _isKernelTabSelected;
        public bool IsPoolTabSelected {
            get => _isPoolTabSelected;
            set {
                _isPoolTabSelected = value;
                OnPropertyChanged(nameof(IsPoolTabSelected));
            }
        }
        public bool IsWalletTabSelected {
            get => _isWalletTabSelected;
            set {
                _isWalletTabSelected = value;
                OnPropertyChanged(nameof(IsWalletTabSelected));
            }
        }

        public bool IsKernelTabSelected {
            get => _isKernelTabSelected;
            set {
                _isKernelTabSelected = value;
                OnPropertyChanged(nameof(IsKernelTabSelected));
            }
        }


        public void SwitchToPoolTab() {
            this.IsWalletTabSelected = false;
            this.IsKernelTabSelected = false;
            this.IsPoolTabSelected = true;
        }

        public void SwitchToWalletTab() {
            this.IsPoolTabSelected = false;
            this.IsKernelTabSelected = false;
            this.IsWalletTabSelected = true;
        }

        public void SwitchToKernelTab() {
            this.IsPoolTabSelected = false;
            this.IsWalletTabSelected = false;
            this.IsKernelTabSelected = true;
        }
    }
}
