# FurinaXML
[**English**](./README.md) | [简体中文](./README_zh-Hans.md)

A library for reading/writing XML tags.

## Dependencies
.NET 8.0\
No third-party dependencies

## Features
- Supports NativeAOT
- No heap memory allocation when reading
- Only supports reading from `ReadOnlySpan<byte>` or `ReadOnlySpan<char>` containing complete XML
- Supports writing to `Span<byte>`, `Span<char>`, `Stream` or `TextWriter`
- Only provides basic tag processing and content escaping, does not support XSD/DTD, does not support declarations such as `<!ENTITY`