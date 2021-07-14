using System;
using System.IO;
using System.Collections.Generic;

namespace Sleep_Downloader
{
    class Fetch
    {
        public List<Reports> GetReports(string SelectedFolder, string Archive, string Study, string Filter, string Filter2, int RepeatCheck)
        {
            List<Reports> ReportList = new List<Reports>();

            /// Search for folders in selected path.
            /// 
            string JoinedPath = String.Format(@"{0}\{1}\{2}\", SelectedFolder, Archive, Study);
            string[] Files = Directory.GetFiles(JoinedPath, Filter, SearchOption.TopDirectoryOnly);

            foreach (string File in Files)
            {
                ReportList.Add(new Reports()
                {
                    Name = File
                });
            }

            for (int i = 0; i < ReportList.Count; i++)
            {
                if (ReportList[i].Name.Contains(Filter2))
                {
                    ReportList.RemoveRange(i, 1);
                }
            }

            /// Double back if option selected and no report found.
            /// 
            if (RepeatCheck == 1)
            {
                ReportList.RemoveRange(0, ReportList.Count); /// Clear the list.
                Files = Directory.GetFiles(JoinedPath, Filter, SearchOption.TopDirectoryOnly);
                foreach (string File in Files)
                {
                    ReportList.Add(new Reports()
                    {
                        Name = File
                    });
                }
            }

            /// Catch "hidden files" that Windows and other OS' leave around.
            /// 
            for (int i = 0; i < ReportList.Count; i++)
            {
                if (ReportList[i].Name.Contains(@"$"))
                {
                    ReportList.RemoveRange(i, 1);
                }
            }

            return ReportList;
        }

        public List<Studies> GetStudies(string SelectedFolder, string Archive)
        {
            List<Studies> StudyList = new List<Studies>();

            /// Search for folders in selected path.
            /// 
            string JoinedPath = String.Format(@"{0}\{1}", SelectedFolder, Archive);
            string[] Folders = Directory.GetDirectories(JoinedPath, "*", SearchOption.TopDirectoryOnly);

            foreach (string Folder in Folders)
            {
                StudyList.Add(new Studies()
                {
                    Name = Folder
                });
            }

            /// Remove JoinedPath somehow being included in this string.
            ///
            JoinedPath += @"\";
            for (int i = 0; i < StudyList.Count; i++)
            {
                StudyList[i].Name = StudyList[i].Name.Replace(JoinedPath, "");
            }

            StudyList.Sort();

            return StudyList;
        }

        public List<Archives> GetArchives (string SelectedFolder)
        {
            List<Archives> ArchiveList = new List<Archives>();

            /// Search for folders in selected path.
            /// 
            string[] Folders = Directory.GetDirectories(SelectedFolder, "*", SearchOption.TopDirectoryOnly);

            foreach (string Folder in Folders)
            {
                ArchiveList.Add(new Archives()
                {
                    Name = Folder,
                    Selected = true /// Selected by default.
                });
            }

            /// Remove path from archive name for easier reading.
            /// 
            SelectedFolder += @"\";
            for (int i = 0; i < ArchiveList.Count; i++)
            {
                ArchiveList[i].Name = ArchiveList[i].Name.Replace(SelectedFolder, "");
            }

            ArchiveList.Sort();

            return ArchiveList;
        }
    }
}
