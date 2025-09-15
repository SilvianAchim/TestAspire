using System.Security.Cryptography;
using System.Text;

namespace MSCoffee.Common.Rooms;

public sealed class DefaultCodeGenerator : ICodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no I, O, 0, 1

    public string NewRoomCode(int length = 6)
    {
        if (length < 4 || length > 12) length = 6;

        var bytes = RandomNumberGenerator.GetBytes(length);
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            sb.Append(Alphabet[bytes[i] % Alphabet.Length]);
        }
        return sb.ToString();
    }
}
