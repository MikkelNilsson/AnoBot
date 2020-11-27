using System;
using System.Threading.Tasks;

namespace SimpBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new SimpBot().InitalizeAsync();
        }
    }
}
