namespace MSCoffee.Common.Rooms;

public interface ICodeGenerator
{
    string NewRoomCode(int length = 6);
}
