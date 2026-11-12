# XiaomiToastBridge：Windows 11 x64 免安装 Visual Studio 方案

## 先说明交付边界

这个压缩包包含 UWP 测试程序源码、Windows 云端构建工作流和本机安装脚本。**它不是已经编译好的安装包**。当前制作环境不是 Windows，无法在这里实际运行 UWP/MSIX 编译器，因此不能诚实地把源码 ZIP 说成可直接安装的 EXE/MSIX。

此方案不要求你的电脑安装 Visual Studio。由 GitHub Actions 的 Windows 2022 构建机执行 MSBuild，生成签名的 x64 AppX/MSIX 测试包；你只需从 Actions 下载构建产物并在 Windows 11 上安装。

## 生成安装包（无需本机安装 VS）

1. 登录 GitHub，新建一个**私有**仓库，例如 `XiaomiToastBridge`。
2. 将本压缩包解压，把解压后 `XiaomiToastBridge` 文件夹内的全部内容（包括隐藏的 `.github` 文件夹）上传到仓库根目录。确认仓库根目录能看到 `.github/workflows/build-windows.yml` 和 `XiaomiToastBridge/XiaomiToastBridge.csproj`。
3. 打开仓库的 **Actions** 页面，选择 **Build XiaomiToastBridge (Windows x64)**。
4. 点击 **Run workflow**，等待 Windows 构建任务结束。
5. 成功后，在该次运行页面底部的 **Artifacts** 下载 `XiaomiToastBridge-Windows11-x64`，解压到本机。

> GitHub 仓库应设为 Private，因为后续若扩展通知桥接，可能涉及通知内容和调试信息。当前构建流程不会上传你电脑上的通知内容。

## 在 Windows 11 x64 安装

1. 解压构建产物。
2. 在该目录空白处按住 Shift 右键，选择“在此处打开 PowerShell 窗口”或“在终端中打开”。
3. 使用管理员身份打开 PowerShell，然后执行：

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Install-XiaomiToastBridge.ps1
```

脚本会把构建用的自签名证书加入当前用户的 `TrustedPeople` 证书存储，并安装 AppX/MSIX 包。证书只用于这个本地测试包；不要把它用于其他软件或分发给他人。

如果 Windows 提示应用安装策略阻止安装，先不要修改系统安全策略，把完整错误信息发给我确认。

## 测试

1. 从开始菜单启动 `XiaomiToastBridge`。
2. 点击“请求权限并开始监听”，按 Windows 提示授予通知访问权限。
3. 用小米手机向电脑发送一条新消息。
4. 查看程序日志，重点确认是否出现 `AUMID=com.xiaomi.hyperConnect` 以及通知正文。
5. 将日志文本或截图发回，我再根据结果判断下一步。

## 重要限制

- 这是第一阶段验证工具：只读取并记录通知，不会关闭系统 Toast、不清除通知，也不会修改 MyDockFinder。
- `UserNotificationListener` 是通知读取/管理接口，不是 Toast 显示前的拦截钩子。读取成功不代表已经实现“系统右下角完全不弹、只由 Dock 提示”。
- GitHub Actions 构建工作流尚未在本会话环境中实际运行验证；若构建失败，请把 Actions 中第一条红色错误和上下文发给我，我会按具体错误修正。
