using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLib
{
    public class RandomString
    {

        public RandomString()
        {
            random = new Random(Guid.NewGuid().GetHashCode());
        }

        public string GetString(int length)
        {

            this.length = length;
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private int length;
        private Random random;
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    }
}
