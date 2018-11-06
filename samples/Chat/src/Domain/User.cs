using System;
using System.Security.Cryptography;
using System.Text;

namespace Domain
{
    public class User
    {
        public Guid Id { get; }
        public string Name { get; }

        public User(string name)
        {
            Id = GetUserId(name);
            Name = name;
        }

        public static Guid GetUserId(string userName)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(userName));
                return new Guid(hash);
            }
        }
    }
}