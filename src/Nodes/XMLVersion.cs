using System.Numerics;

namespace FurinaXML.Nodes;

public struct XMLVersion(int minor)
    : IEquatable<XMLVersion>, IComparable<XMLVersion>, IComparisonOperators<XMLVersion, XMLVersion, bool>
{
    public static readonly int Major = 1;
    public int Minor = minor;

    public static implicit operator Version(XMLVersion ver)
        => new(Major, ver.Minor);
    public override readonly int GetHashCode()
        => Minor;
    public override readonly string ToString()
        => $"{Major}.{Minor}";
    public override readonly bool Equals(object? obj)
        => obj is XMLVersion ver && Equals(ver);
    public readonly bool Equals(XMLVersion other)
        => Minor.Equals(other.Minor);
    public readonly int CompareTo(XMLVersion other)
        => Minor.CompareTo(other.Minor);
    public static bool operator ==(XMLVersion left, XMLVersion right)
        => left.Minor == right.Minor;
    public static bool operator !=(XMLVersion left, XMLVersion right)
         => left.Minor != right.Minor;
    public static bool operator <(XMLVersion left, XMLVersion right)
        => left.Minor < right.Minor;
    public static bool operator >(XMLVersion left, XMLVersion right)
        => left.Minor > right.Minor;
    public static bool operator <=(XMLVersion left, XMLVersion right)
        => left.Minor <= right.Minor;
    public static bool operator >=(XMLVersion left, XMLVersion right)
        => left.Minor >= right.Minor;
}