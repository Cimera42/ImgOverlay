﻿using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interop;

namespace ImgOverlay
{
    public class YesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString() == "Yes";
        }
    }

    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : Window
    {
        public ControlPanel()
        {
            InitializeComponent();
        }

        private void DragButton_Click(object sender, RoutedEventArgs e)
        {
            if (Owner == null)
                return;

            var locked = (sender as ToggleButton).IsChecked.Value;
            SetLocked(locked);

            e.Handled = true;
        }

        private void SetLocked(bool locked)
        {
            Owner.IsHitTestVisible = !locked;

            var hwnd = new WindowInteropHelper(Owner).Handle;
            if (locked)
            {
                WindowsServices.SetWindowExTransparent(hwnd);
            }
            else
            {
                WindowsServices.SetWindowExOpaque(hwnd);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetLocked(DragButton.IsChecked.Value);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() == true)
            {
                (Owner as MainWindow)?.LoadImage(openDialog.FileName);
            }
        }
    }
}