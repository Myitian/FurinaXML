# FurinaXML
[English](./README.md) | [**简体中文**](./README_zh-Hans.md)

一个用于读取/写入 XML 标记的库。

## 依赖
.NET 8.0\
无第三方依赖

## 特性
- 支持 NativeAOT
- 读取时可以不产生堆内存分配
- 仅支持从包含完整 XML 的 `ReadOnlySpan<byte>` 或 `ReadOnlySpan<char>` 读取
- 支持写入到 `Span<byte>`、`Span<char>`、`Stream` 或 `TextWriter`
- 仅提供基本的标记处理和内容转义，不支持 XSD/DTD，不支持 `<!ENTITY` 等声明