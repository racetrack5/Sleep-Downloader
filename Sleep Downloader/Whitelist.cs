using System.Collections.Generic;
using System.IO;
namespace Sleep_Downloader
{
    class Whitelist
    {
        public List<Fields> FilterFields()
        {
            List<Fields> Whitelist = new List<Fields>();

            StreamReader File = new StreamReader(@"Whitelist.txt");
            string Line;

            /// Get whitelisted fields.
            ///
            while ((Line = File.ReadLine()) != null)
            {
                var parts = Line.Split('\t');
                Whitelist.Add(new Fields()
                {
                    Name = parts[0],
                    Name2 = parts[1]
                });
            }

            return Whitelist;
        }

        public List<Fields> FetchFields(string FilePath)
        {
            List<Fields> Whitelist = new List<Fields>();

            StreamReader File = new StreamReader(FilePath);
            string Line;

            /// Get whitelisted fields.
            ///
            while ((Line = File.ReadLine()) != null)
            {
                var parts = Line.Split('\t');
                Whitelist.Add(new Fields()
                {
                    Name = parts[0],
                    Name2 = parts[1]
                });
            }

            return Whitelist;
        }
    }
}
