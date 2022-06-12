using System;
using System.Collections.Generic;
using System.IO;

namespace Sleep_Downloader
{
    class Fetch
    {
        public List<Reports> GetReports(string SelectedFolder, string Archive, string Study, string Filter, string Filter2, bool Repeat)
        {
            List<Reports> l_Report = new List<Reports>();

            // Search for folders in selected path.
            string JoinedPath = String.Format(@"{0}\{1}\{2}\", SelectedFolder, Archive, Study);
            string[] Files = Directory.GetFiles(JoinedPath, Filter, SearchOption.TopDirectoryOnly);

            foreach (string File in Files)
            {
                l_Report.Add(new Reports()
                {
                    Name = File
                });
            }

            for (int i = 0; i < l_Report.Count; i++)
            {
                if (l_Report[i].Name.Contains(Filter2))
                {
                    l_Report.RemoveRange(i, 1);
                }
            }

            // Double back if option selected and no report found.
            if (Repeat)
            {
                l_Report.RemoveRange(0, l_Report.Count); /// Clear the list.
                Files = Directory.GetFiles(JoinedPath, Filter, SearchOption.TopDirectoryOnly);
                foreach (string File in Files)
                {
                    l_Report.Add(new Reports()
                    {
                        Name = File
                    });
                }
            }

            // Catch "hidden files" that Windows and other OS' leave around.
            for (int i = 0; i < l_Report.Count; i++)
            {
                if (l_Report[i].Name.Contains(@"$"))
                {
                    l_Report.RemoveRange(i, 1);
                }
            }

            return l_Report;
        }

        public List<Studies> GetStudies(string SelectedFolder, string Archive)
        {
            List<Studies> l_Study = new List<Studies>();

            // Search for folders in selected path.
            string JoinedPath = String.Format(@"{0}\{1}", SelectedFolder, Archive);
            string[] Folders = Directory.GetDirectories(JoinedPath, "*", SearchOption.TopDirectoryOnly);

            foreach (string Folder in Folders)
            {
                l_Study.Add(new Studies()
                {
                    Name = Folder
                });
            }

            // Remove JoinedPath somehow being included in this string.
            JoinedPath += @"\";
            for (int i = 0; i < l_Study.Count; i++)
            {
                l_Study[i].Name = l_Study[i].Name.Replace(JoinedPath, "");
            }

            l_Study.Sort();

            return l_Study;
        }

        public List<Archives> GetArchives(string SelectedFolder)
        {
            List<Archives> l_Archive = new List<Archives>();

            // Search for folders in selected path.
            string[] Folders = Directory.GetDirectories(SelectedFolder, "*", SearchOption.TopDirectoryOnly);

            foreach (string Folder in Folders)
            {
                l_Archive.Add(new Archives()
                {
                    Name = Folder,
                    Selected = true // Selected by default.
                });
            }

            // Remove path from archive name for easier reading.
            SelectedFolder += @"\";
            for (int i = 0; i < l_Archive.Count; i++)
            {
                l_Archive[i].Name = l_Archive[i].Name.Replace(SelectedFolder, "");
            }

            l_Archive.Sort();

            return l_Archive;
        }
    }
}
