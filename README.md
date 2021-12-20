# MemuHelper Library  
 Thư viện này dùng để hỗ trợ các thao tác với máy ảo Memu Play thuận tiện và dễ dàng hơn;  
# Example 
### Set Path Memu Play & ADB
```js
MemuControl.memuFolderPath = @"C:\Program Files\Microvirt\MEmu";  
MemuControl.adbFolderPath = @"C:\Program Files\Microvirt\MEmu";  
```
### 1 Initialization  
```js
int index = 1;  
MemuControl memuControl = new MemuControl(index);  
```
### 2 Using
- Open VM  
```js
await memuControl.OpenVM();
```
- Stop VM
```js
await memuControl.StopVM(false);
```
- Reboot VM
```js
await memuControl.RebootVM();
```
### OR 
- ADB Execute  
```js
string version = MemuControl.ADBExecute("version");
```
```js
string[] devices = MemuControl.ADBExecute("devices").Split('\n');
```
```js
string version = MemuControl.ADBExecute("version");
```
```js
int secondTimeOut = -1; 
int index = 1;
string screenXml = MemuControl.ADBExecute("shell uiautomator dump", secondTimeOut, index);
```
- CMD Execute  
```js
string ip = MemuControl.CMDExecute("ipconfig", Application.StartupPath);
```
- Stop All VMs  
```js
await MemuControl.StopAllVMs();
```
- Sort All VMs  
```js
await MemuControl.SortVMs();
```
# See more  
https://www.memuplay.com/blog/memucommand-reference-manual.html  

# Contact with me  
- Facebook : www.facebook.com/quachvinhky  
- Gmail: quachvinhky2000@gmail.com  
