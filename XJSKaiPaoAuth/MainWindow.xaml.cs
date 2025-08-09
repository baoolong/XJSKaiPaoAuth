using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            new Thread(() =>
            {
                // 清理365天前的日志
                Logger.CleanOldLogs(365);
            }).Start();

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
            int duration = int.Parse(((ComboBoxItem)(cmbDuration.SelectedItem)).Content.ToString().Trim());

            if (string.IsNullOrEmpty(machineCode))
            {
                MessageBox.Show("请输入机器码");
                return;
            }
            Logger.Info($"\r\n\t生成激活码\r\n\t机器码：{machineCode}\r\n\t时长：{duration}月");
            machineCode = machineCode.Replace("-", "");
            // 生成激活码 (这里使用简单的逻辑，实际应用中应使用更复杂的加密算法)
            string activationCode = GenerateActivationCode(machineCode, duration);

            // 显示激活码
            txtActivationCode.Text = activationCode;
            try
            {
                // 复制到剪贴板
                Clipboard.SetText(activationCode);
                MessageBox.Show("激活码已复制到剪贴板");
            }
            catch (Exception ex)
            {
                Logger.Error($"复制激活码到剪贴板时出错: {ex.Message}");
                MessageBox.Show($"复制激活码到剪贴板时出错: 请手动复制", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateActivationCode(string machineCode, int duration)
        {

            // 生成随机混淆字符的辅助函数
            string GenerateRandomString(int minLength, int maxLength)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&()_+-=[]{}|;:,.<>?";
                Random random = new Random();
                int length = random.Next(minLength, maxLength + 1);
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            
            
            try
            {
                using (AESEncryptionManager rsaManager = new AESEncryptionManager())
                {
                    string decryptmachineCode = rsaManager.Decrypt(machineCode);
                    // 使用字符串数组作为分隔符
                    string[] parts = decryptmachineCode.Split(new string[] { "**" }, StringSplitOptions.None);
                    string cpuSerial = string.Empty;
                    string diskSerial = string.Empty;
                    if (parts.Length >= 9)
                    {
                        cpuSerial = parts[2];
                        diskSerial = parts[6];
                    }
                    else
                    {
                        Logger.Error($"机器码格式不正确，解析异常！");

                        // 处理分隔符不符合预期的情况
                        MessageBox.Show("机器码格式不正确，请重新输入");
                        return string.Empty;
                    }
                    string ObfuscatedCharsHead = GenerateRandomString(24,36);
                    string ObfuscatedCharsMiddle = GenerateRandomString(24, 36);
                    string ObfuscatedCharsMiddle2 = GenerateRandomString(24, 36);
                    string ObfuscatedCharsEnd = GenerateRandomString(24, 36);

                    // 计算当前时间加上duration * 31 天后的时间
                    DateTime expirationDate = DateTime.Now.AddDays(duration * 31);

                    // 使用机器码、时长和过期时间生成激活码
                    string timeString = $"{DateTime.Now:yyyyMMdd}-{expirationDate:yyyyMMdd}";

                    string uniqueId = ObfuscatedCharsHead 
                    + "**CPUID**" + cpuSerial + "**CPUID**" 
                    + ObfuscatedCharsMiddle 
                    + "**TIME**" + timeString + "**TIME**" 
                    + ObfuscatedCharsMiddle2
                    + "**DISKID**" + diskSerial + "**DISKID**" 
                    + ObfuscatedCharsEnd;

                    CPUSerial.Content = "COU序号：" + cpuSerial;
                    DiskSerial.Content = "硬盘序号：" + diskSerial;
                    ExpirationTime.Content = "到期时间：" + expirationDate.ToString("yyyy-MM-dd");

                    // 加密并返回激活码
                    string  EncryptuniqueId = rsaManager.Encrypt(uniqueId);
                    



                    // 格式化输出，每4个字符添加一个连字符
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < EncryptuniqueId.Length; i++)
                    {
                        sb.Append(EncryptuniqueId[i]);
                        if ((i + 1) % 4 == 0 && i < EncryptuniqueId.Length - 1)
                        {
                            sb.Append("-");
                        }
                    }
                    Logger.Info($"\r\n\t生成激活码：{sb.ToString()}" +
                    $"\r\n\tCPU序号：{cpuSerial}\r\n\t硬盘序号：{diskSerial}\r\n\t过期时间：{expirationDate:yyyy-MM-dd}");
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"生成激活码时出错: {ex.Message}");
                MessageBox.Show($"生成激活码时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }
    }
}
