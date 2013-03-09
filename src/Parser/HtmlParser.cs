using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Parser
{
    public class HtmlParser
    {

        public void Run()
        {
            var path = @"C:\Personal\GitHub\jwnwt\lib\raw\05_BI12_.GE.xhtml";
            var stream = new StreamReader(path);
            var xmlReader = new XmlTextReader(stream);
            xmlReader.DtdProcessing = DtdProcessing.Ignore;
            xmlReader.XmlResolver = null;

            var xmlElement = XElement.Load(xmlReader);

            //from el in xmlTree1.Elements()
            //where((int)el >= 3 && (int)el <= 5)
            //select el
                

        }

        private void LoadEachFileToMemortyForReading()
        {




        }
        /// <summary>
        /// This will load the file names in the specified folder location
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetFileNames()
        {
            throw new NotImplementedException();
        }



    }
}
