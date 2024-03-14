using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extensions
{
    public static class StringExtensions
    {
        
        /// <summary>
        /// Simply checks the file's extension against a list of allowed extensions. Note that this is not very secure, since the extension
        /// does not necessarily reflect the file's contents
        /// </summary>
        /// <param name="s">path to image</param>
        /// <returns>true if known image extension, false else</returns>
        public static bool IsImageFilePath(this string s)
        {
            if (s.EndsWith(".jpg")) return true;
            if (s.EndsWith(".png")) return true;
            if (s.EndsWith(".bmp")) return true;
            if (s.EndsWith(".tiff")) return true;
            return false;
        }

        public static void CreateDirectory(this string dirString)
        {
            try
            {
                Directory.CreateDirectory(dirString);
            }
            catch (IOException) { }
        }

        public static string GetStringSha256Hash(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }


        public static List<string> GetImagesList(this string parentDir)
        {
            var trainImages = Directory
                .EnumerateFiles(Path.Combine(parentDir, "images")).Where(x => x.IsImageFilePath()).ToList();
            return trainImages;
        }

        public static Dictionary<string, IEnumerable<string>> GetLabelDictionary(this List<string> imageList)
        {
            var trainLabels = new Dictionary<string, IEnumerable<string>>();
            foreach (var i in imageList)
                // try all supported extensions; labels should be .png or .bmp
                foreach(var imgType in new List<string> { ".png", ".bmp"})
                        if(File.Exists(Path.Combine(Directory.GetParent(i).Parent.FullName, "labels", Path.GetFileNameWithoutExtension(i) + imgType)))
                            trainLabels.Add(i, new List<string>() { Path.Combine("labels", Path.GetFileNameWithoutExtension(i) + imgType) });
            
                
            return trainLabels;
        }
    }
}
