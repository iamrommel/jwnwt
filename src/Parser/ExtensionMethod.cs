using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Parser
{
    public static class ExtensionMethod
    {
        public static IEnumerable<XElement> GetByCssClassName(this XElement source, string className)
        {
            return source.Descendants().Where(m => m.Attribute("class") != null && m.Attribute("class").Value == className);

        }

        public static XElement ReplaceNode(this XElement source, string openingTag, string openingTagReplace, string closingTag, string closingTagReplace)
        {
        //    var tagfindValue = openingTagReplace.Split(' ').FirstOrDefault();
        //    tagfindValue = tagfindValue.Substring(1, tagfindValue.Length);
        //    var tagReplaceValue = openingTagReplace.Split(' ').FirstOrDefault();
        //    tagReplaceValue = tagReplaceValue.Substring(1, tagReplaceValue.Length);

            var result = source.ToString(SaveOptions.DisableFormatting)
                           .Replace(openingTag, openingTagReplace)
                           .Replace(closingTag, closingTagReplace);
                         

           return  XElement.Parse(result);



        }

    }
}
