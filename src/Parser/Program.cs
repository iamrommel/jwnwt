using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {

            var parser = new HtmlParser();
            //parser.Run();
            parser.SetBookIndex();
        }
    }
}
