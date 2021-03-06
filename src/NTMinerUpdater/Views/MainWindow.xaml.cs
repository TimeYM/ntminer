﻿using MahApps.Metro.Controls;
using NTMiner.Vms;
using System.Windows;
using System.Windows.Controls;

namespace NTMiner.Views {
    public partial class MainWindow : MetroWindow {
        public MainWindowViewModel Vm {
            get {
                return (MainWindowViewModel)this.DataContext;
            }
        }

        public MainWindow() {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void MetroWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                this.DragMove();
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (!DevMode.IsDevMode) {
                return;
            }
            DataGrid dg = (DataGrid)sender;
            if (dg.SelectedItem != null) {
                ((NTMinerFileViewModel)dg.SelectedItem).Edit.Execute(null);
            }
        }
    }
}
