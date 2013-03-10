using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace Parser
{
    public class HtmlParser
    {

        public void Run()
        {
            string langauge = @"tg\";
            string sourcePath = @"C:\Personal\GitHub\jwnwt\lib\raw\" + langauge;
            string destinationPath = @"C:\Personal\Github\jwnwt\lib\html-template\" + langauge;



            var sourceFileNames = Directory.GetFiles(sourcePath);
            var totalFilesNames = sourceFileNames.Count();
            int ctr = 0;

            foreach (var fileName in sourceFileNames)
            {
                ctr++;

                Console.WriteLine(string.Format("Doing {0} out of {1}, for file {2}", ctr, totalFilesNames, fileName));

                var sourceFileName = Path.GetFileName(fileName);
                var destinationFileName = sourceFileName;
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                if (!fileNameWithoutExtension.Contains("split"))
                    destinationFileName = string.Format("{0}-split1{1}", fileNameWithoutExtension, Path.GetExtension(fileName));

                UsingXElement(string.Format(@"{0}{1}", sourcePath, sourceFileName), string.Format(@"{0}\{1}", destinationPath, destinationFileName));


                //this is is a temporary exit loop
                //if (ctr >= 50)
                //    break;
            }


            Console.WriteLine("DONE!!");


        }

        private static void UsingXElement(string sourcePath, string destinationPath)
        {
            var xmlReader = new XmlTextReader(sourcePath) { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
            var element = XElement.Load(xmlReader);

            var bodyElement =
                element.Descendants()
                       .FirstOrDefault(m => m.Name.LocalName.Equals("body", StringComparison.InvariantCultureIgnoreCase));

            var xml =
                bodyElement.ToString()
                           .Replace(@"<body xmlns=""http://www.w3.org/1999/xhtml"">", "<div>")
                           .Replace("</body>", "</div>");

            var cleanBodyElementt = XElement.Parse(xml);

            //clean up the index file
            //<p class="st"><a id="page7"></a><b>Genesis</b></p>

            var stParagraph = cleanBodyElementt.Descendants().FirstOrDefault(m => m.Attribute("class") != null && m.Attribute("class").Value == "st");
            XElement removeAnchor = null;
            if (stParagraph != null)
            {
              



                //remove the BOLD TAG
                removeAnchor = stParagraph
                    .Descendants().FirstOrDefault(s => s != null && s.Name == "a");

                if (removeAnchor != null)
                {
                    removeAnchor.Remove();
                    //after removal reset the value

                    var value = stParagraph.Descendants().FirstOrDefault().Value;
                    stParagraph.SetValue(value);

                    //update the value fo the class as w_biblebookname
                    stParagraph.Attribute("class").SetValue("w_biblebookname"); 

                }

            }




            File.WriteAllText(destinationPath, cleanBodyElementt.ToString());



        }

    }
}
