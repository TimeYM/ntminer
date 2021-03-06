﻿using NTMiner.Vms;
using System;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class RestartWindows : UserControl {
        public static void ShowDialog() {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                Title = "Windows系统重启倒计时",
                Width = 400,
                Height = 200,
                IsDialogWindow = true,
                CloseVisible = System.Windows.Visibility.Collapsed,
                IconName = "Icon_Restart"
            }, ucFactory: (window) => {
                RestartWindows uc = new RestartWindows();
                uc.CloseWindow = () => {
                    window.Close();
                };
                return uc;
            }, fixedSize: true);
        }

        public RestartWindowsViewModel Vm {
            get {
                return (RestartWindowsViewModel)this.DataContext;
            }
        }

        private bool _isCanceled = false;
        public Action CloseWindow;
        public RestartWindows() {
            InitializeComponent();
            System.Timers.Timer t = new System.Timers.Timer(1000);
            t.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => {
                if (_isCanceled) {
                    t.Enabled = false;
                    t.Stop();
                    return;
                }
                Execute.OnUIThread(() => {
                    Vm.Seconds = Vm.Seconds - 1;
                    if (Vm.Seconds <= 0) {
                        Windows.Power.Restart();
                    }
                });
            };
            t.Start();
        }

        private void KbCancelButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            _isCanceled = true;
            CloseWindow?.Invoke();
        }
    }

    public class RestartWindowsViewModel : ViewModelBase {
        private int _seconds = 9;

        public int Seconds {
            get => _seconds;
            set {
                _seconds = value;
                OnPropertyChanged(nameof(Seconds));
            }
        }
    }
}
