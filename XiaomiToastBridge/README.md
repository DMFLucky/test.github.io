# XiaomiToastBridge（小米互联 Toast 通知监听测试）

## 目的

验证 Windows 的 `UserNotificationListener` 是否能读取小米互联服务提交的 Toast 通知，目标 AUMID：

`com.xiaomi.hyperConnect`

本项目是**第一阶段验证工具**，不关闭 Windows Toast、不清除通知、不修改 MyDockFinder，也不会自动把通知转发到 Dock。

## 重要说明

- 通知内容可能包含私人消息。日志仅显示在本机窗口，不会上传。
- 必须由用户在 Windows 权限提示中明确允许通知访问。
- 该 API 读取的是 Windows 当前可访问的 Toast 通知列表，并通过 `NotificationChanged` 事件触发重新枚举。
- 若小米互联通知没有进入 Windows Toast 通知平台，或系统没有向 Listener 暴露该通知，本程序也无法读取。
- 这版是前台测试程序。窗口关闭或应用被挂起后，不保证继续监听；后续若验证成功，再做后台常驻/桥接设计。

## 开发环境

- Windows 10 2004（Build 19041）或 Windows 11
- 如需本机编译：Visual Studio 2022 +“通用 Windows 平台开发”工作负载 + Windows SDK（至少 10.0.19041.0）
- 如不安装 Visual Studio：参见项目根目录的 `README-免安装VS构建说明.md`，使用 GitHub Actions 的 Windows 构建机生成安装包

## 编译运行

1. 解压项目。
2. Visual Studio 打开 `XiaomiToastBridge.csproj`。
3. 允许 NuGet 还原。
4. 配置选择 `Debug`、`x64`。
5. 如果首次打包要求证书，使用 Visual Studio 的“创建测试证书”功能生成本地开发证书；仅用于本机测试。
6. 按 F5 部署并启动。
7. 点击“请求权限并开始监听”，在 Windows 弹出的通知访问授权页面/提示中允许。
8. 先让小米手机向电脑发送一条消息，查看日志中是否出现 `com.xiaomi.hyperConnect`。
9. 如果没有新增记录，点击“读取当前通知”，再触发一条新的手机通知并观察。

## 如何判断

### 成功

日志出现类似：

```text
[检测到新增/当前列表新出现] ID=123 | AUMID=com.xiaomi.hyperConnect | 微信 | 张三：测试消息
```

这证明 Listener 能读取小米互联 Toast 的来源身份及文本内容，可以进入第二阶段。

### 只匹配到 AUMID，但没有正文

说明通知可见，但其内容可能不是 `ToastGeneric` 布局，或者通知内容被系统/应用做了限制。下一步需要检查 `Notification.Visual` 的其他 binding 或 XML 结构。

### 完全没有匹配记录

依次检查：
1. 本程序是否获得通知访问权限；
2. Windows 设置中小米互联的通知是否开启；
3. 手机是否确实向 Windows 发出了新通知；
4. 通知是否被系统策略、专注助手或应用设置抑制；
5. 实际 AUMID 是否与 `com.xiaomi.hyperConnect` 完全一致（本程序按不区分大小写匹配）。

## 关于“接管”目标

本测试程序**不会拦截或隐藏**原生 Toast。`UserNotificationListener` 是通知读取/管理接口，不是通用的 Toast 显示前拦截钩子。即使读取成功，也不能仅凭它保证在原生 Toast 出现前将其取消。

下一阶段需要分别验证：
- 能否可靠读取新增通知；
- 能否按通知 ID 调用 `RemoveNotification` 清除对应通知（这会影响通知中心中的该条通知，需谨慎）；
- 如何把内容安全地传递给 MyDockFinder。MyDockFinder 是否有可调用的外部消息注入接口，目前尚未确认。
