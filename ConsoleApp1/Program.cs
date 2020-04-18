using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eq2crate
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    _ = new RunCrate();
                    break;
                default:
                    break;
            }
        }
    }
}
