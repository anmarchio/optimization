using System;
//using Microsoft.VisualBasic.FileIO;

namespace Optimization.Serialization
{
    public static class CSVReader
    {
        public static double[][] Read(string path, string[] commentTokens, string[] delimiters, bool containsColumnNames)
        {
            throw new NotImplementedException();
            /*
            using (var parser = new TextFieldParser(path, Encoding.Default))
            {
                if (containsColumnNames) parser.ReadLine();
                parser.SetDelimiters(delimiters);
                if (commentTokens != null) parser.CommentTokens = commentTokens;
                var list = new List<double[]>();
                while(!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    var row = new double[fields.Length];
                    for(int i = 0; i < fields.Length; i++)
                    {
                        if(fields.Count(x => x.Contains(".")) > 0)
                            row[i] = double.Parse(fields[i], System.Globalization.NumberFormatInfo.InvariantInfo);
                        else
                            row[i] = double.Parse(fields[i]);
                    }

                    list.Add(row);
                }
                return list.ToArray();
            }*/
        }


        /// <summary>
        /// specify bracket type if you want to extract only text from within the brackets (string, because brackets need to be escaped: openingBracket + @"([^)]*)" + closingBracket ) -- don't forget closing and opening
        /// e.g.: openingBracket = @"\[", closingBracket = @"\]"
        /// 
        ///  @"([^)]*)"
        /// </summary>
        /// <param name="path"></param>
        /// <param name="delimiters"></param>
        /// <param name="bracketType"></param>
        /// <returns></returns>
        public static string[] ReadHead(string path, string[] delimiters, string openingBracket = null, string closingBracket = null)
        {
            throw new NotImplementedException();
            /*
            using (var parser = new TextFieldParser(path, Encoding.Default))
            {
                parser.SetDelimiters(delimiters);
                var head = parser.ReadFields();

                if (openingBracket != null && closingBracket != null)
                    for (int i = 0; i < head.Length; i++)
                        head[i] = Regex.Match(head[i], openingBracket + @"(.*)" + closingBracket).Groups[1].Value;
                return head;
            }*/
        }
    }
}
