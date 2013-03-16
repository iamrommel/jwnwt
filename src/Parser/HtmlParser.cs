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
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Parser
{
    public class HtmlParser
    {


        private string sourcePath = @"C:\Personal\GitHub\jwnwt\lib\raw\tg\";
        private string destinationPath = @"C:\Personal\Github\jwnwt\src\jwnwt.Mobile\template\tg\";
        private string scriptPath = @"C:\Personal\Github\jwnwt\src\jwnwt.Mobile\js\";


        public void Run()
        {



            var sourceFileNames = Directory.GetFiles(sourcePath).OrderBy(m => m);
            var totalFilesNames = sourceFileNames.Count();
            int ctr = 0;

            foreach (var fileName in sourceFileNames)
            {


                ctr++;

                Console.WriteLine(string.Format("Doing {0} out of {1}, for file {2}", ctr, totalFilesNames, fileName));

                var sourceFileName = Path.GetFileName(fileName);
                var finalSource = string.Format(@"{0}{1}", sourcePath, sourceFileName);

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

                var finalDesitnation = string.Format(@"{0}{1}", destinationPath, destinationFileName);
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
            var destinationFileName = Directory.GetFiles(destinationPath);
            var bookNames = destinationFileName.Select(m =>
            {
                var fileName = Path.GetFileNameWithoutExtension(m);
                var items = fileName.Split('-');
                var chapterIndex = int.Parse(items[2]);
                var upperVerse = int.Parse(items[3]);
                var chaptersVerses = new List<ChapterVerse>() { 
                    new ChapterVerse()
                    {
                        Chapter = chapterIndex,
                        UpperVerse = upperVerse
                    }};

                var result = new BookIndex()
                {
                    Code = items[1],
                    Chapters = chaptersVerses,
                    TraditionalOrder = int.Parse(items[0]),
                };




                return result;
            })
            ;

            var uniqueBooks = bookNames.OrderBy(m => m.TraditionalOrder);


            //var uniqueBooks = bookNames.Distinct(new BookIndexEqualityComparer())
            //    .Select(m =>
            //    {
            //        var maxChapter = bookNames.Max(n => n.UpperChapter);
            //        string biblename = "Bible Name here";
            //        //get the booknames
            //        //var bookPath = string.Format("{0}{1}-1.xhtml", destinationPath, m.Code);
            //        //var xmlReader = new XmlTextReader(bookPath) { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
            //        //var element = XElement.Load(xmlReader);
            //        //  biblename = element.Descendants()
            //        //    .FirstOrDefault(u => u.Name.LocalName.Equals("p", StringComparison.InvariantCultureIgnoreCase))
            //        //    .Value;

            //            var result = new List<ChapterVerse>();

            //            for (int i = 1; i <= maxChapter; i++)
            //            {
            //                result.Add(new ChapterVerse() { Chapter = i });
            //            }


            //        return new BookIndex() { Code = m.Code, UpperChapter = maxChapter, Name = biblename };

            //    })
            //    ;





            var jsonResult = JsonConvert.SerializeObject(uniqueBooks.ToList());

            File.WriteAllText(string.Format("{0}tg-bookindex.json", scriptPath), jsonResult);


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

    [DataContract]
    public class BookIndex
    {
        [DataMember(Name = "c")]
        public string Code { get; set; }

        [DataMember(Name = "n")]
        public string Name { get; set; }

        [DataMember(Name = "cp")]
        public List<ChapterVerse> Chapters
        {
            get;
            set;

        }

        [DataMember(Name = "to")]
        public int TraditionalOrder { get; set; }
         
    }

    [DataContract]
    public class ChapterVerse
    {
        [DataMember(Name = "cpc")]
        public int Chapter { get; set; }


        [DataMember(Name = "cpu")]
        public int UpperVerse { get { return 20; } set { } }
    }

    class BookIndexEqualityComparer : IEqualityComparer<BookIndex>
    {

        public bool Equals(BookIndex b1, BookIndex b2)
        {
            if (b1.Code == b2.Code)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public int GetHashCode(BookIndex bx)
        {
            //int hCode =  bx.Code.GetHashCode() ^  bx.UpperChapter ^ bx.UpperVerse;
            return bx.Code.GetHashCode();
        }

    }

}
