﻿using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System;
using System.Windows.Media;

namespace NTMiner.Vms {
    public class SpeedChartViewModel : ViewModelBase {
        private SeriesCollection _series = new SeriesCollection();
        private AxesCollection _axisY;
        private AxesCollection _axisX;
        private SeriesCollection _seriesShadow = new SeriesCollection();
        private AxesCollection _axisYShadow;
        private AxesCollection _axisXShadow;

        private readonly GpuSpeedViewModel _gpuSpeedVm;
        public SpeedChartViewModel(GpuSpeedViewModel gpuSpeedVm) {
            _gpuSpeedVm = gpuSpeedVm;
            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
                .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            Func<double, string> emptyDateTimeFormatter = (d) => { return string.Empty; };
            Func<double, string> emptySpeedFormatter = (d) => { return string.Empty; };
            Func<double, string> dateTimeFormatter = value => new DateTime((long)value).ToString("HH:mm");
            Func<double, string> speedFormatter = value => value.ToUnitSpeedText();
            //AxisStep forces the distance between each separator in the X axis
            double axisStep = TimeSpan.FromMinutes(1).Ticks;
            //AxisUnit forces lets the axis know that we are plotting Minutes
            //this is not always necessary, but it can prevent wrong labeling
            double axisUnit = TimeSpan.TicksPerMinute;

            ChartValues<MeasureModel> mainCoinSpeedValues = new ChartValues<MeasureModel>();
            ChartValues<MeasureModel> mainCoinSpeedValuesShadow = new ChartValues<MeasureModel>();
            LineSeries mainCoinSpeedLs = new LineSeries {
                Title = gpuSpeedVm.GpuVm.IndexText,
                DataLabels = false,
                PointGeometrySize = 0,
                StrokeThickness = 1,
                Values = mainCoinSpeedValues
            };
            LineSeries mainCoinSpeedLsShadow = new LineSeries {
                Title = gpuSpeedVm.GpuVm.IndexText,
                DataLabels = false,
                PointGeometrySize = 0,
                StrokeThickness = 1,
                Values = mainCoinSpeedValuesShadow
            };
            this.Series.Add(mainCoinSpeedLs);
            this.SeriesShadow.Add(mainCoinSpeedLsShadow);
            SolidColorBrush black = new SolidColorBrush(Colors.Black);
            Axis axisYMainCoin = new Axis() {
                LabelFormatter = emptySpeedFormatter,
                MinValue = 0,
                Separator = new Separator()
            };
            this._axisY = new AxesCollection() {
                axisYMainCoin
            };
            DateTime now = DateTime.Now;
            this._axisX = new AxesCollection() {
                new Axis() {
                    LabelFormatter = emptyDateTimeFormatter,
                    MaxValue = now.Ticks,
                    MinValue = now.Ticks - TimeSpan.FromMinutes(NTMinerRoot.Current.SpeedHistoryLengthByMinute).Ticks,
                    Unit=axisUnit,
                    Separator = new Separator() {
                        Step = axisStep
                    }
                }
            };
            Axis axisYShadowMainCoin = new Axis() {
                LabelFormatter = speedFormatter,
                MinValue = 0,
                FontSize = 14,
                Foreground = black,
                Separator = new Separator()
            };
            this._axisYShadow = new AxesCollection() {
                axisYShadowMainCoin
            };
            this._axisXShadow = new AxesCollection() {
                new Axis() {
                    LabelFormatter = dateTimeFormatter,
                    Foreground = black,
                    MaxValue = 0,
                    FontSize = 14,
                    MinValue=0,
                    Unit=axisUnit,
                    Separator = new Separator() {
                        Step = axisStep
                    }
                }
            };
        }

        public MinerProfileViewModel MinerProfile {
            get {
                return MinerProfileViewModel.Current;
            }
        }

        public GpuSpeedViewModel GpuSpeedVm {
            get {
                return _gpuSpeedVm;
            }
        }

        public SeriesCollection Series {
            get => _series;
            set {
                _series = value;
                OnPropertyChanged(nameof(Series));
            }
        }

        public SeriesCollection SeriesShadow {
            get => _seriesShadow;
            set {
                _seriesShadow = value;
                OnPropertyChanged(nameof(SeriesShadow));
            }
        }

        public AxesCollection AxisY {
            get => _axisY;
            set {
                _axisY = value;
                OnPropertyChanged(nameof(AxisY));
            }
        }

        public AxesCollection AxisX {
            get {
                return _axisX;
            }
            set {
                _axisX = value;
                OnPropertyChanged(nameof(AxisX));
            }
        }

        public AxesCollection AxisYShadow {
            get => _axisYShadow;
            set {
                _axisYShadow = value;
                OnPropertyChanged(nameof(AxisYShadow));
            }
        }

        public AxesCollection AxisXShadow {
            get {
                return _axisXShadow;
            }
            set {
                _axisXShadow = value;
                OnPropertyChanged(nameof(AxisXShadow));
            }
        }

        public static readonly SolidColorBrush Background = new SolidColorBrush(Color.FromRgb(0xEB, 0xEB, 0xEB));
        private static readonly SolidColorBrush SelectedColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush _itemBackground = Background;
        public SolidColorBrush ItemBackground {
            get {
                return _itemBackground;
            }
            set {
                _itemBackground = value;
                OnPropertyChanged(nameof(ItemBackground));
            }
        }

        public void SetSelectedBackground() {
            ItemBackground = SelectedColor;
        }

        public void SetDefaultBackground() {
            ItemBackground = Background;
        }

        public void SetAxisLimits(DateTime now) {
            AxisX[0].MaxValue = now.Ticks + TimeSpan.FromMinutes(1).Ticks;
            AxisX[0].MinValue = now.Ticks - TimeSpan.FromMinutes(NTMinerRoot.Current.SpeedHistoryLengthByMinute).Ticks;
            AxisXShadow[0].MaxValue = AxisX[0].MaxValue;
            AxisXShadow[0].MinValue = AxisX[0].MinValue;
        }
    }
}
