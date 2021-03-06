﻿using NTMiner.Core;
using NTMiner.Core.Kernels;
using NTMiner.Views;
using NTMiner.Views.Ucs;
using System;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class KernelOutputFilterViewModel : ViewModelBase, IKernelOutputFilter {
        private Guid _kernelId;
        private string _regexPattern;
        private Guid _id;

        public Guid GetId() {
            return this.Id;
        }

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }

        public KernelOutputFilterViewModel(IKernelOutputFilter data) : this(data.GetId()) {
            _kernelId = data.KernelId;
            _regexPattern = data.RegexPattern;
            _id = data.GetId();
        }

        public KernelOutputFilterViewModel(Guid id) {
            _id = id;
            this.Edit = new DelegateCommand(() => {
                KernelOutputFilterEdit.ShowEditWindow(this);
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                DialogWindow.ShowDialog(message: $"您确定删除{this.RegexPattern}内核输出过滤器吗？", title: "确认", onYes: () => {
                    Global.Execute(new RemoveKernelOutputFilterCommand(this.Id));
                }, icon: "Icon_Confirm");
            });
        }

        public Guid Id {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public Guid KernelId {
            get => _kernelId;
            set {
                _kernelId = value;
                OnPropertyChanged(nameof(KernelId));
            }
        }

        public string RegexPattern {
            get => _regexPattern;
            set {
                _regexPattern = value;
                OnPropertyChanged(nameof(RegexPattern));
            }
        }
    }
}
