# LR1Calculator
一个编译技术作业使用的LR1计算器，能快速为你算出闭包和他们的转换关系



## 使用

为了使用此代码，你需要：

```
dotnet sdk >= 9.0
```

你可以在此处下载：[下载 .NET(Linux、macOS 和 Windows)](https://dotnet.microsoft.com/zh-cn/download)

如果已经安装，你可以使用`dotnet --info`来查看自己的SDK是否满足要求，作者的SDK版本为：

```
.NET SDK:
 Version:           9.0.101
 Commit:            eedb237549
 Workload version:  9.0.100-manifests.4a280210
 MSBuild version:   17.12.12+1cce77968

运行时环境:
 OS Name:     Windows
 OS Version:  10.0.22631
 OS Platform: Windows
 RID:         win-x64
 Base Path:   C:\Program Files\dotnet\sdk\9.0.101\
```

使用`git clone`获取项目后使用`cd`指令进入`ConsoleSolver`项目

```
cd ConsoleSolver
```

使用`dotnet run`来运行项目。



## 关于输入的一些注意事项

对于一个规则，你只能使用形似：

````
S -> Sab
````

的规则，**不要使用任何`'`，多余的空格分割，处理器会将空格看作终结符**，**箭头符号`->`前后固定有一个空格，不可省略！**

目前只支持`1~20`个规则，并且不保证任何情况（非LR(1)文法情况等）可用，也不完全保证准确性！
