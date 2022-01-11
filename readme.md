# Juxtapose
## 1. Intro
基于 `SourceGenerator` 的硬编码 `.Net` 多`进程`运行库。

## 2. Features
 - 可以为`接口`和`静态类`生成代理，无需手动编写RPC相关代码，即可进行`多进程`开发；
 - 编译时生成所有代码，运行时无显式的反射调用和动态构造；
 - 支持`委托`和`CancellationToken`类型的方法参数（其余类型未特殊处理，将会进行序列化，目前回调`委托`不支持嵌套和`CancellationToken`）；
 - 支持`Linux`、`Windows`（其它未测试）；

## 3. Requirement
 - .Net5.0+(其它版本没有尝试过)

## 4. 使用方法

### 4.1 引用包
```XML
<ItemGroup>
  <PackageReference Include="Juxtapose" Version="1.0.*-*" />
  <PackageReference Include="Juxtapose.SourceGenerator" Version="1.0.*-*" />
</ItemGroup>
```

------

### 4.2 建立上下文
```C#
[Illusion(typeof(Greeter), typeof(IGreeter), "Juxtapose.Test.GreeterAsIGreeterIllusion")]
public partial class GreeterJuxtaposeContext : JuxtaposeContext
{
}
```
示例代码将为`Greeter`生成`IGreeter`接口的代理类型`Juxtapose.Test.GreeterAsIGreeterIllusion`；

Note!!!
 - 必须继承`JuxtaposeContext`；
 - 必须标记`partial`关键字；

------

### 4.3 添加入口点
在`Main`方法中添加入口点代码，并使用指定上下文
```C#
await JuxtaposeEntryPoint.TryAsEndpointAsync(args, GreeterJuxtaposeContext.SharedInstance);
```

------

#### 到此已完成开发，创建类型`Juxtapose.Test.GreeterAsIGreeterIllusion`的对象，并调用其方法，其实际逻辑将在子进程中运行；

## 5. 工作逻辑
`SourceGenerator`在编译时生成代理类型，封装通信消息。默认使用命名管道进行进程间通信，使用`System.Text.Json`进行消息的序列化与反序列化。

## 参见示例，未完待续......

----

### 示例列表

|       项目        |       内容        |
| ---------------- | ---------------- |
|SampleLibrary|基于库的使用示例，可由其它程序直接使用|
|SampleConsoleApp|基于控制台的使用示例，可使用当前程序集生成的类，或使用其他库生成的类|
|ResourceBasedObjectPool|基于系统资源的动态对象池示例|