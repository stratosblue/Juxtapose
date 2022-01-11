﻿// <Auto-Generated></Auto-Generated>

namespace System
{
    /// <summary>
    /// 存储大小
    /// </summary>
    public readonly struct StorageSize : IEquatable<StorageSize>
    {
        #region Public 属性

        /// <summary>
        /// Byte
        /// </summary>
        public long Byte { get; }

        /// <summary>
        /// KB
        /// </summary>
        public double Kilobyte { get; }

        /// <summary>
        /// MB
        /// </summary>
        public double Megabyte { get; }

        /// <summary>
        /// GB
        /// </summary>
        public double Gigabyte { get; }

        /// <summary>
        /// TB
        /// </summary>
        public double Terabyte { get; }

        /// <summary>
        /// PB
        /// </summary>
        public double Petabyte { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="StorageSize"/>
        /// </summary>
        /// <param name="byteSize">Byte 大小</param>
        public StorageSize(long byteSize)
        {
            Byte = byteSize;
            Kilobyte = Byte / 1024;
            Megabyte = Kilobyte / 1024;
            Gigabyte = Megabyte / 1024;
            Terabyte = Gigabyte / 1024;
            Petabyte = Terabyte / 1024;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <summary>
        /// + 运算符
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static StorageSize operator +(StorageSize a, StorageSize b) => new(a.Byte + b.Byte);

        /// <summary>
        /// - 运算符
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static StorageSize operator -(StorageSize a, StorageSize b) => new(a.Byte - b.Byte);

        /// <summary>
        /// * 运算符
        /// </summary>
        /// <param name="a"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static StorageSize operator *(StorageSize a, int value) => new(a.Byte * value);

        /// <summary>
        /// / 运算符
        /// </summary>
        /// <param name="a"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static StorageSize operator /(StorageSize a, int value) => new(a.Byte / value);

        /// <summary>
        /// 大于 运算符
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(StorageSize a, StorageSize b) => a.Byte > b.Byte;

        /// <summary>
        /// 小于 运算符
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(StorageSize a, StorageSize b) => a.Byte < b.Byte;

        /// <summary>
        /// 等于 运算符
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(StorageSize a, StorageSize b) => a.Byte == b.Byte;

        /// <summary>
        /// != 运算符
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(StorageSize a, StorageSize b) => a.Byte != b.Byte;

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is StorageSize storage && Equals(storage);
        }

        /// <inheritdoc/>
        public bool Equals(StorageSize other)
        {
            return Byte == other.Byte;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Byte.GetHashCode();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Megabyte:F4}Mb";
        }

        #endregion Public 方法
    }
}