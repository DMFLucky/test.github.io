using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XiaomiToastBridge
{
    public sealed partial class MainPage : Page
    {
        private const string TargetAumid = "com.xiaomi.hyperConnect";
        private UserNotificationListener _listener;
        private readonly HashSet<uint> _knownTargetIds = new HashSet<uint>();
        private bool _isListening;

        public MainPage()
        {
            InitializeComponent();
            _listener = UserNotificationListener.Current;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var access = await _listener.RequestAccessAsync();
                Log("权限请求结果：" + access);

                if (access != UserNotificationListenerAccessStatus.Allowed)
                {
                    StatusText.Text = "状态：未获得通知访问权限。请在 Windows 设置中允许本应用访问通知，然后重新启动本程序。";
                    return;
                }

                if (!_isListening)
                {
                    _listener.NotificationChanged += Listener_NotificationChanged;
                    _isListening = true;
                }

                StatusText.Text = "状态：监听中。请让小米手机产生一条新通知。";
                await RefreshNotificationsAsync(true);
            }
            catch (Exception ex)
            {
                Log("启动监听失败：" + ex);
                StatusText.Text = "状态：启动失败，详见日志。";
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshNotificationsAsync(false);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Text = "";
        }

        private async void Listener_NotificationChanged(
            UserNotificationListener sender,
            UserNotificationChangedEventArgs args)
        {
            // 事件可能来自非 UI 线程；回到 UI Dispatcher 后重新枚举当前通知。
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await RefreshNotificationsAsync(false);
            });
        }

        private async System.Threading.Tasks.Task RefreshNotificationsAsync(bool establishBaseline)
        {
            try
            {
                var status = _listener.GetAccessStatus();
                if (status != UserNotificationListenerAccessStatus.Allowed)
                {
                    StatusText.Text = "状态：通知访问权限不可用：" + status;
                    return;
                }

                IReadOnlyList<UserNotification> notifications =
                    await _listener.GetNotificationsAsync(NotificationKinds.Toast);

                int matched = 0;
                foreach (var item in notifications)
                {
                    try
                    {
                        string aumid = item.AppInfo?.AppUserModelId ?? "";
                        if (!string.Equals(aumid, TargetAumid, StringComparison.OrdinalIgnoreCase))
                            continue;

                        matched++;
                        bool isNew = _knownTargetIds.Add(item.Id);
                        if (establishBaseline)
                        {
                            Log($"[已有通知] ID={item.Id} | AUMID={aumid} | {ReadText(item)}");
                        }
                        else if (isNew)
                        {
                            Log($"[检测到新增/当前列表新出现] ID={item.Id} | AUMID={aumid} | {ReadText(item)}");
                        }
                    }
                    catch (Exception exItem)
                    {
                        Log("解析单条通知失败：" + exItem.Message);
                    }
                }

                if (establishBaseline)
                    StatusText.Text = $"状态：监听中；当前通知列表中匹配到 {matched} 条小米互联通知。现在请发送一条新消息进行测试。";
                else
                    StatusText.Text = $"状态：监听中；当前通知列表中匹配到 {matched} 条小米互联通知。";
            }
            catch (Exception ex)
            {
                Log("读取通知失败：" + ex);
            }
        }

        private static string ReadText(UserNotification item)
        {
            var notification = item.Notification;
            if (notification?.Visual == null)
                return "(通知没有可读取的 Visual 内容)";

            var binding = notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            if (binding == null)
                return "(没有 ToastGeneric binding；可能是自定义通知布局)";

            var texts = binding.GetTextElements()
                               .Select(x => x.Text)
                               .Where(x => !string.IsNullOrWhiteSpace(x));
            string content = string.Join(" | ", texts);
            return string.IsNullOrWhiteSpace(content) ? "(Toast 文本为空)" : content;
        }

        private void Log(string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            LogBox.Text += line + Environment.NewLine;
            LogBox.SelectionStart = LogBox.Text.Length;
            LogBox.SelectionLength = 0;
        }
    }
}
