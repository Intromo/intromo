using System;
using System.Security.Cryptography;
using System.Text;

namespace Domain
{
    public class Room
    {
        public Guid Id { get; }
        public string Name { get; }

        public Room(string name)
        {
            Id = GetRoomId(name);
            Name = name;
        }

        public static Guid GetRoomId(string roomName)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(roomName));
                return new Guid(hash);
            }
        }
    }
}