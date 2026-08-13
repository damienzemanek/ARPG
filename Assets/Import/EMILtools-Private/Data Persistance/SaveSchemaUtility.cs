using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

public static class SaveSchemaUtility
{
    public static string GetSchemaHash(System.Type type)
    {
        var fields = type
            .GetFields(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance)
            .Where(f => !f.IsNotSerialized)
            .Where(f => f.Name != nameof(SavedDataSO.schemaInfo))
            .OrderBy(f => f.Name)
            .Select(f => $"{f.Name}:{f.FieldType.FullName}");

        string schema = string.Join("|", fields);

        using var sha = SHA256.Create();

        byte[] bytes = Encoding.UTF8.GetBytes(schema);
        byte[] hash = sha.ComputeHash(bytes);

        return ToHex(hash);    
    }
    
    static readonly char[] HexChars = "0123456789ABCDEF".ToCharArray();

    static string ToHex(byte[] bytes)
    {
        char[] chars = new char[bytes.Length * 2];

        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            chars[i * 2] = HexChars[b >> 4];
            chars[i * 2 + 1] = HexChars[b & 0xF];
        }

        return new string(chars);
    }
    
    [Serializable]
    public struct SaveSchemaInfo
    {
        public string hash;
        public int version;
    }
}