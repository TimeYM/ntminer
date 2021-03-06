﻿using NTMiner.Core;
using NTMiner.ServiceContracts.DataObjects;
using NTMiner.Views;
using NTMiner.Views.Ucs;
using System;
using System.Linq;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class MinerGroupViewModel : ViewModelBase, IMinerGroup {
        public static readonly MinerGroupViewModel PleaseSelect = new MinerGroupViewModel(Guid.Empty) {
            _name = "请选择"
        };

        private Guid _id;
        private string _name;
        private string _description;

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }

        public MinerGroupViewModel() {
            if (!NTMinerRoot.IsInDesignMode) {
                throw new InvalidProgramException();
            }
        }

        public MinerGroupViewModel(Guid id) {
            _id = id;
            this.Edit = new DelegateCommand(() => {
                MinerGroupEdit.ShowEditWindow(this);
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                DialogWindow.ShowDialog(message: $"您确定删除{this.Name}吗？", title: "确认", onYes: () => {
                    Global.Execute(new RemoveMinerGroupCommand(this.Id));
                }, icon: "Icon_Confirm");
            });
        }

        public MinerGroupViewModel(IMinerGroup data) : this(data.GetId()) {
            _name = data.Name;
            _description = data.Description;
        }

        public Guid GetId() {
            return this.Id;
        }

        public Guid Id {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string Name {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    if (this == PleaseSelect) {
                        return;
                    }
                    OnPropertyChanged(nameof(Name));
                    if (string.IsNullOrEmpty(value)) {
                        throw new ValidationException("名称是必须的");
                    }
                    if (MinerGroupViewModels.Current.List.Any(a => a.Name == value && a.Id != this.Id)) {
                        throw new ValidationException("名称重复");
                    }
                }
            }
        }

        public string Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }
}
