using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XJSKaiPaoAuth
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // 绑定按钮点击事件
            btnGenerate.Click += btnGenerate_Click;
            // 绑定标题栏按钮事件
            MinimizeButton.Click += MinimizeButton_Click;
            MaximizeButton.Click += MaximizeButton_Click;
            CloseButton.Click += CloseButton_Click;
        }

        // 窗口拖动功能
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // 最小化按钮事件
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // 最大化/还原按钮事件
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        // 关闭按钮事件
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            // 获取机器码和激活时长
            string machineCode = txtMachineCode.Text.Trim();
            int duration = int.Parse(cmbDuration.SelectedItem.ToString().Trim());

            if (string.IsNullOrEmpty(machineCode))
            {
                MessageBox.Show("请输入机器码");
                return;
            }

            // 生成激活码 (这里使用简单的逻辑，实际应用中应使用更复杂的加密算法)
            string activationCode = GenerateActivationCode(machineCode, duration);

            // 显示激活码
            txtActivationCode.Text = activationCode;
        }

        private string GenerateActivationCode(string machineCode, int duration)
        {
            // 简单的激活码生成逻辑，仅作示例
            string baseString = $"{machineCode}-{duration}-{DateTime.Now:yyyyMMdd}";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(baseString);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
        }
    }
}
