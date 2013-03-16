using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            string destinationPath = @"C:\Personal\Github\jwnwt\src\jwnwt.Mobile\template\" + langauge;



            var sourceFileNames = Directory.GetFiles(sourcePath).OrderBy(m => m);
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

                destinationFileName = string.Format("{0}{1}", SetFileName(destinationFileName), Path.GetExtension(fileName));
                destinationFileName = destinationFileName.Replace("split", "");

                var finalSource = string.Format(@"{0}{1}", sourcePath, sourceFileName);
                var finalDesitnation = string.Format(@"{0}{1}", destinationPath, destinationFileName);

                var stringResult = UsingXElement(finalSource, finalDesitnation);

                File.WriteAllText(finalDesitnation, stringResult);

                //this is is a temporary exit loop
                //if (ctr >= 3)
                //    break;
            }


            Console.WriteLine("DONE!!");


        }


        private string UsingXElement(string sourcePath, string destinationPath)
        {
            var xmlReader = new XmlTextReader(sourcePath) { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
            var element = XElement.Load(xmlReader);

            var bodyElement =
                element.Descendants()
                       .FirstOrDefault(m => m.Name.LocalName.Equals("body", StringComparison.InvariantCultureIgnoreCase));

            var xml =
                bodyElement.ToString(SaveOptions.DisableFormatting)
                           .Replace(@"<body xmlns=""http://www.w3.org/1999/xhtml"">", "<div>")
                           .Replace("</body>", "</div>")
                            .Replace(@"<a id=""", @"<div id=""")
                            .Replace("</a>", "</div>")
                            .Replace(@"\r\n", "")
                                       ;

            var cleanBodyElementt = XElement.Parse(xml);

            //clean up the index file
            //<p class="st"><a id="page7"></a><b>Genesis</b></p>

            var stParagraph = cleanBodyElementt.GetByCssClassName("st").FirstOrDefault();
            XElement removeAnchor = null;
            var bibleName = string.Empty;
            if (stParagraph != null)
            {

                //remove the BOLD TAG
                removeAnchor = stParagraph
                    .Descendants().FirstOrDefault(s => s != null && s.Name == "div");

                if (removeAnchor != null)
                {
                    removeAnchor.Remove();
                  

                    bibleName = stParagraph.Descendants().FirstOrDefault().Value;
                   
                }

            }



            //split paragraph
            var cleanedElement = cleanBodyElementt.GetByCssClassName("sb")
                .Select(m => SplitString(m));
            var allVerseName = string.Join("", cleanedElement);
            string newBody = null;
            if (string.IsNullOrEmpty(bibleName))
            {
                newBody = string.Format("<div>{0}</div>", allVerseName);
            }
            else
            {
                newBody = string.Format(@"<div><p class=""biblename"">{0}</p>{1}</div>", bibleName, allVerseName);
            }


            return XElement.Parse(newBody).ToString();

        }

        private string SplitString(XElement source)
        {
            var input = source.ToString(SaveOptions.DisableFormatting);
            var pattern = @"<div id=""chapter\w*_verse\w*""></div>";
            var splitedItem = Regex.Split(input, pattern, RegexOptions.IgnoreCase).ToList();

            var matchCollection = Regex.Matches(input, pattern);

            var ctr = 0;
            var result = splitedItem.Skip(1).Select(m =>
            {
                var clean = m.Replace("</p>", "");
                var parent = matchCollection[ctr].ToString().Replace("</div>", "");
                var formattedItem = string.Format("{0}{1}</div>", parent, clean);
                ctr++;

                return formattedItem;

            });

            var joinedString = string.Join("", result);

            return joinedString;
            //return XElement.Parse(joinedString);

        }


        private string SetFileName(string destinationFileName)
        {
            var pattern = @"\w*-split\d*";
            var matchCollection = Regex.Matches(destinationFileName, pattern);

            return matchCollection[0].ToString();

        }

    }
}
