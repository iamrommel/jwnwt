using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Parser
{
    public class HtmlParser
    {
        private const string SourcePath = @"C:\Personal\GitHub\jwnwt\lib\raw\tg\";
        private const string DestinationPath = @"C:\Personal\Github\jwnwt\src\jwnwt.Mobile\template\tg\";
        private const string ScriptPath = @"C:\Personal\Github\jwnwt\src\jwnwt.Mobile\js\";


        public void Run()
        {



            var sourceFileNames = Directory.GetFiles(SourcePath).OrderBy(m => m);
            var totalFilesNames = sourceFileNames.Count();
            int ctr = 0;

            foreach (var fileName in sourceFileNames)
            {

                //if (!fileName.Equals(@"C:\Personal\GitHub\jwnwt\lib\raw\tg\22_BI12_.JOB-split29.xhtml", StringComparison.InvariantCultureIgnoreCase))
                //    continue;

                ctr++;

                Console.WriteLine(string.Format("Doing {0} out of {1}, for file {2}", ctr, totalFilesNames, fileName));

                var sourceFileName = Path.GetFileName(fileName);
                var finalSource = string.Format(@"{0}{1}", SourcePath, sourceFileName);

                int count;
                var stringResult = UsingXElement(finalSource, out count);


                //var destinationFileName = sourceFileName;
                var destinationFileName = Path.GetFileNameWithoutExtension(fileName);
                if (!destinationFileName.Contains("split"))
                {
                    destinationFileName = string.Format("{0}-split1", destinationFileName);
                }

                destinationFileName = string.Format("{0}-{1}{2}", SetFileName(destinationFileName), count, Path.GetExtension(fileName));
                destinationFileName = destinationFileName.Replace("split", "");

                var finalDesitnation = string.Format(@"{0}{1}", DestinationPath, destinationFileName);
                File.WriteAllText(finalDesitnation, stringResult);

                //this is is a temporary exit loop
                //if (ctr >= 3)
                //    break;
            }



            Console.WriteLine("DONE!!");


        }

        public void SetBookIndex()
        {
            //-08-1CH-13-33.xhtml
            var destinationFileName = Directory.GetFiles(DestinationPath);
            var bookNames = destinationFileName.Select(m =>
            {
                var fileName = Path.GetFileNameWithoutExtension(m);
                var items = fileName.Split('-');
                var chapterIndex = int.Parse(items[2]);
                var upperVerse = int.Parse(items[3]);


                var result = new BookIndex()
                {
                    Code = items[1],
                    Chapter = chapterIndex,
                    MaximumVerseOfChapter = upperVerse,
                    TraditionalOrder = int.Parse(items[0]),
                };
                return result;
            }).ToList();

            //get the maximum chapter of the  book
            var groupBookNames = bookNames.GroupBy(m => m.Code)
                                          .Select(m => new
                                              {
                                                  Key = m.FirstOrDefault(),
                                                  MaxChapter = m.Max(u => u.Chapter)
                                              }).ToList();

            //update the MaxChapter of the book
            foreach (var bookName in bookNames)
            {
                bookName.MaximumChapterOfBook =
                    groupBookNames.FirstOrDefault(m => m.Key.Code == bookName.Code).MaxChapter;

            }

            //get the book names
            var firstChapterOfTheBooks = bookNames.Where(m => m.Chapter == 1);
            foreach (var firstChapterOfTheBook in firstChapterOfTheBooks)
            {
                var filename = string.Format("{0}{1}-{2}-{3}-{4}.xhtml", DestinationPath,
                                firstChapterOfTheBook.TraditionalOrder.ToString().PadLeft(2, '0'),
                                firstChapterOfTheBook.Code,
                                firstChapterOfTheBook.Chapter,
                                firstChapterOfTheBook.MaximumVerseOfChapter);


                var xmlReader = new XmlTextReader(filename) { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
                var element = XElement.Load(xmlReader);

                //get the element with "biblename" class value

                var bibleName = element.Descendants()
                                       .FirstOrDefault(n => n.Attribute("class").Value == "biblename").Value;

                //update the name for of thhe current book all remaining chapters
                foreach (var bookName in bookNames)
                {
                    if (bookName.Code == firstChapterOfTheBook.Code)
                        bookName.Name = bibleName;
                }
            }



            //order the book by traditional order then by chapter
            var finalResult = bookNames.OrderBy(m => m.TraditionalOrder).ThenBy(m => m.MaximumChapterOfBook);


            var preValue = "var bookIndex =  ";
            var mainValue = JsonConvert.SerializeObject(finalResult);
            var postValue =  ";";
            var jsonResult = string.Format("{0}{1}{2}", preValue, mainValue, postValue);


            File.WriteAllText(string.Format("{0}tg-bookindex.js", ScriptPath), jsonResult);


        }

        #region Private Helper
        private string UsingXElement(string sourcePath, out int verseCount)
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
                            .Replace(@"<p class=""sl"">", @"<p class=""sb"">")
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

            var newBodyElement = XElement.Parse(newBody);
            verseCount = newBodyElement
                                .Descendants().Where(s => s != null && s.Name == "div")
                                .Count() - 1;


            return newBodyElement.ToString(SaveOptions.DisableFormatting);

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

            return destinationFileName.Replace("_BI12_.", "-");

        }





        #endregion

    }
}
