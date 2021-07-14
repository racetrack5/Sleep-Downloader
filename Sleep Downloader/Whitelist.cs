using System.Collections.Generic;
using System.IO;
namespace Sleep_Downloader
{
    class Whitelist
    {
        public List<Fields_Whitelist> FilterFields()
        {
            List<Fields_Whitelist> Whitelist = new List<Fields_Whitelist>();

            StreamReader File = new StreamReader(@"Whitelist.txt");
            string Line;

            /// Get whitelisted fields.
            ///
            while((Line = File.ReadLine()) != null)
            {
                Whitelist.Add(new Fields_Whitelist()
                {
                    Name = Line
                });
            }

            return Whitelist;
        }

        public List<Fields_Whitelist> FetchFields(string FilePath)
        {
            List<Fields_Whitelist> Whitelist = new List<Fields_Whitelist>();

            StreamReader File = new StreamReader(FilePath);
            string Line;

            /// Get whitelisted fields.
            ///
            while ((Line = File.ReadLine()) != null)
            {
                Whitelist.Add(new Fields_Whitelist()
                {
                    Name = Line
                });
            }

            return Whitelist;
        }
    }
}
