# LR1Calculator
一个为编译技术作业的完成**提供思路和提示**的LR1计算器，能快速为你算出闭包和他们的转换关系



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

由于编写较为匆忙，部分情况暂未考虑，建议把扩充的起始文法作为第一个规则输入（也就是说非常建议最后一次输入“哪个是起始文法”时输入保持为0）



## 更新

- 2024/12/10

添加了输出转换表的功能

- 2024/12/12

修复了当转移（`Transition`）到已经存在的闭包时，答案不正确的问题
