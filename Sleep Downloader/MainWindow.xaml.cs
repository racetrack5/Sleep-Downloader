using Spire.Doc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace Sleep_Downloader
{
    public partial class MainWindow : Window
    {
        // Lists globally accessible.
        public ObservableCollection<Fields> f_New;
        public ObservableCollection<Fields> f_Current;

        // Working files.
        public const string file_Whitelist      = @"Whitelist\Whitelist.txt";
        public const string file_bak_Whitelist  = @"Whitelist\Whitelist.txt.bak";
        public const string file_Log            = @"Output\Log.txt";
        public const string file_Missing        = @"Output\Missing Reports.txt";

        public MainWindow()
        {
            InitializeComponent();

            Logger("Sleep Downloader version 2.0.0\n");

            // Populate current whitelist in setup tab.
            Whitelist h_Whitelist = new Whitelist();

            try
            {
                f_Current = new ObservableCollection<Fields>(h_Whitelist.FieldList(file_Whitelist));
                cVars_Current.ItemsSource = f_Current;

                lFields2.Content = String.Format("{0} fields", cVars_Current.Items.Count);
                Logger("whitelist found");
            }
            catch
            {
                Logger("ERROR fetching current whitelist.");
            }
        }

        private void Logger(string Message)
        {
            string Output = String.Format("{0} - {1}\n", DateTime.Now, Message);

            // Update component on UI thread.
            this.Dispatcher.Invoke(() =>
            {
                tOutput.Text += Output;
            });
        }

        private void Tracker(string Message)
        {
            string Output = String.Format("{0} - {1}\n", DateTime.Now, Message);

            // Update component on UI thread.
            this.Dispatcher.Invoke(() =>
            {
                lCount.Content = Output;
            });
        }

        // Worker thread template.
        private void thread_Worker(bool Output, bool Repeat, string s_File, string s_Folder, string s_Select, string s_Filter, 
            bool Missing, int i_PreCull, int i_PostCull)
        {
            Whitelist h_Whitelist = new Whitelist();
            Fetch h_Archive = new Fetch();
            List<Archives> l_Archive = new List<Archives>();  

            // Get archive list.
            try
            {
                l_Archive = h_Archive.GetArchives(s_Folder);
            }
            catch
            {
                Logger("ERROR obtaining archive list... stopping.");
                return;
            }

            // Populate field whitelist.
            List<Fields> Whitelist = new List<Fields>();

            try
            {
                if (Output)
                {
                    Logger("refetching whitelist.");
                }
                Whitelist = h_Whitelist.FieldList(file_Whitelist);
            }
            catch
            {
                Logger("ERROR applying whitelist... stopping.");
                return;
            }

            // Setup.
            String OutputFile                   = String.Format(@"Output\{0}", s_File);
            StreamWriter Writer                 = new StreamWriter(OutputFile);
            StreamWriter LogWriter              = new StreamWriter(file_Log);
            StreamWriter MissingReportWriter    = new StreamWriter(file_Missing);
            StringBuilder Builder               = new StringBuilder();
            StringBuilder LogBuilder            = new StringBuilder();
            StringBuilder MissingReportBuilder  = new StringBuilder();

            // Variables to be used later.
            string Line         = "";
            double var_Counter  = 0;
            double var_Errors   = 0;
            double var_Missing  = 0;
            bool AlignmentFlag  = false;

            // Format missing report log.
            MissingReportBuilder.AppendLine(String.Format("Archive\tStudy"));

            l_Archive.ForEach(delegate (Archives Archive)
            {
                if (Archive.Selected)
                {
                    try
                    {
                        if (Output)
                        {
                            Logger("opening archive...");
                        }

                        List<Studies> l_Study = new List<Studies>();
                        l_Study = h_Archive.GetStudies(s_Folder, Archive.Name);

                        // Open study and find report according to filter settings.
                        l_Study.ForEach(delegate (Studies Study)
                        {
                            try
                            {
                                if (Output)
                                {
                                    Logger(String.Format("opening study {0}...", Study.Name));
                                }

                                List<Reports> l_Report = new List<Reports>();
                                l_Report = h_Archive.GetReports(s_Folder, Archive.Name, Study.Name, s_Select, s_Filter, Repeat);

                                // Check if report is missing and add to list.
                                if (l_Report.Count == 0)
                                {
                                    MissingReportBuilder.AppendLine(String.Format("{0}\t{1}", Archive.Name, Study.Name));
                                    if (Output)
                                    {
                                        Logger(String.Format("missing report for study {0}, in archive {1}...", Study.Name, Archive.Name));
                                    }
                                    var_Missing++;
                                }

                                l_Report.ForEach(delegate (Reports Report)
                                {
                                    try
                                    {
                                        if (Output)
                                        {
                                            Logger(String.Format("opening report {0}...", Report.Name));
                                        }

                                        Parse h_Stream = new Parse();
                                        List<Fields> Report_Contents = new List<Fields>();
                                        Report_Contents = h_Stream.GetReport(Report.Name, Whitelist, i_PreCull, i_PostCull);

                                        if (var_Counter == 0)
                                        {
                                            // Write headers from whitelist (sets structure of output).
                                            for (int j = 0; j < Whitelist.Count; j++)
                                            {
                                                Line += String.Format("{0}\t", Whitelist[j].Name2);
                                            }
                                            if (Output)
                                            {
                                                Logger("writing headers from whitelist...");
                                            }
                                            Builder.AppendLine(Line);
                                        }

                                        // Ready for next line.
                                        Line = "";

                                        // Write field values.
                                        for (int i = 0; i < Whitelist.Count; i++)
                                        {
                                            AlignmentFlag = false;
                                            for (int j = 0; j < Report_Contents.Count; j++)
                                            {
                                                if (Report_Contents[j].Name == Whitelist[i].Name)
                                                {
                                                    Line += String.Format("{0}\t", Report_Contents[j].Value);
                                                    AlignmentFlag = true;
                                                }
                                            }
                                            if (AlignmentFlag == false)
                                            {
                                                Line = Line + "-\t";
                                            }
                                        }

                                        if (Output)
                                        {
                                            Logger(String.Format("writing values from {0}...", Report.Name));
                                        }

                                        Builder.AppendLine(Line);
                                        Line = "";

                                        var_Counter++;
                                        Tracker(String.Format("{0} reports imported, {1} reports skipped due to errors, {2} missing reports", var_Counter, var_Errors, var_Missing));

                                        // Write and clear buffer.
                                        Writer.Write(Builder);
                                        Writer.Flush();
                                        Builder.Clear();
                                    }
                                    catch
                                    {
                                        Logger(String.Format("ERROR opening report {0}... skipping...", Report.Name));
                                        var_Errors++;
                                    }
                                });
                            }
                            catch
                            {
                                Logger(String.Format("ERROR opening study {0}... skipping...", Study.Name));
                                var_Errors++;
                            }
                        });
                    }
                    catch
                    {
                        Logger(String.Format("ERROR opening archive {0}... skipping...", Archive.Name));
                        var_Errors++;
                    }
                }
            });

            // Write missing reports list if checked.
            if (Missing)
            {
                try
                {
                    MissingReportWriter.Write(MissingReportBuilder);
                    MissingReportWriter.Flush();
                    MissingReportBuilder.Clear();

                    Logger("missing reports log written.\n");

                }
                catch
                {
                    Logger("ERROR writing missing reports log.\n");
                }
            }

            // Finish and close files.
            Writer.Close();
            LogWriter.Close();
            MissingReportWriter.Close();

            Logger("finished.\n");
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            string s_File = cFileName.Text;
            string s_Folder = cFolderPath.Text;
            string s_Select = cSelect.Text;
            string s_Filter = cFilter.Text;

            int i_PreCull   = Convert.ToInt32(cReportTextValue.Text);
            int i_PostCull  = Convert.ToInt32(cReportTextValuePost.Text);

            bool Output = true;
            if (cOutputEnable.IsChecked == false)
            {
                Output = false;
            }
            bool Repeat = true;
            if (cFilterCheck.IsChecked == false)
            {
                Repeat = false;
            }
            bool Missing = true;
            if (cMissingReports.IsChecked == false)
            {
                Missing = false;
            }

            // Separate UI and work threads.
            Logger("Starting worker thread...\n");
            Thread Worker = new Thread(() => thread_Worker(Output, Repeat, s_File, s_Folder, s_Select, s_Filter, Missing, i_PreCull, i_PostCull));
            Worker.Start();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Whitelist h_Whitelist = new Whitelist();

            try
            {
                Logger("clearing lists...");

                if (f_New != null)
                {
                    f_New.Clear();
                }
                if (f_Current != null)
                {
                    f_Current.Clear();
                }

                f_Current = new ObservableCollection<Fields>(h_Whitelist.FieldList(file_Whitelist));
                cVars_Current.ItemsSource = f_Current;

                lFields2.Content = String.Format("{0} fields", cVars_Current.Items.Count);
                Logger("whitelist found.");
            }
            catch
            {
                Logger("ERROR resetting... suggest restarting tool.");
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var Dialog = new FolderBrowserDialog())
            {
                DialogResult FolderName = Dialog.ShowDialog();

                if (FolderName == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(Dialog.SelectedPath))
                {
                    cFolderPath.Text = Dialog.SelectedPath;
                }
            }

            Fetch h_Archive = new Fetch();

            try
            {
                Logger("populating archive list.");
                cArchiveList.ItemsSource = h_Archive.GetArchives(cFolderPath.Text);
            }
            catch
            {
                Logger("ERROR obtaining archive list... stopping.");
                return;
            }
        }

        private void SelectWhitelist_Click(object sender, RoutedEventArgs e)
        {
            List<Fields> Report_Contents = new List<Fields>();

            try
            {
                using (var Dialog = new OpenFileDialog())
                {
                    DialogResult FileName = Dialog.ShowDialog();

                    if (FileName == System.Windows.Forms.DialogResult.OK)
                    {
                        Document Report = new Document();

                        // Load the report.
                        Report.LoadFromFile(Dialog.FileName);

                        // Get field names from report.
                        for (int i = 0; i < Report.Variables.Count; i++)
                        {
                            Report_Contents.Add(new Fields()
                            {
                                Name        = Report.Variables.GetNameByIndex(i),
                                Name2       = Report.Variables.GetNameByIndex(i),
                                Value       = Report.Variables.GetValueByIndex(i)
                            }); ;
                        }
                    }
                }

                // Remove empty variables beginning with _.
                int j = 0;
                int k = -1;

                for (int i = 0; i < Report_Contents.Count; i++)
                {
                    if (Report_Contents[i].Name.StartsWith("_"))
                    {
                        j++;
                        if (k < 0)
                        {
                            k = i;
                        }
                    }
                    else
                    {
                        if (j > 0)
                        {
                            Report_Contents.RemoveRange(k, j);
                            j = 0;
                            k = -1;
                        }
                    }
                }

                // Add report text as an extra field.
                Report_Contents.Add(new Fields()
                {
                    Name    = "ReportText",
                    Name2   = "Report Text",
                    Value   = ""
                });

                // Add path as an extra field.
                Report_Contents.Add(new Fields()
                {
                    Name    = "Path",
                    Name2   = "Path",
                    Value   = ""
                });

                // Add new variables to current list (if option selected).
                try
                {
                    if (cUpdateOption.IsChecked == true)
                    {
                        Whitelist h_Whitelist = new Whitelist();
                        List<Fields> Whitelist = new List<Fields>();

                        Whitelist = h_Whitelist.FieldList(file_Whitelist);

                        // Work through each old field and add if it doesn't exist in the new list.
                        int a = 0;
                        Logger("merging lists...");

                        for (int i = 0; i < Whitelist.Count; i++)
                        {
                            for (int m = 0; m < Report_Contents.Count; m++)
                            {
                                if (Whitelist[i].Name == Report_Contents[m].Name)
                                {
                                    a++;
                                }
                            }

                            if (a == 0)
                            {
                                Report_Contents.Add(new Fields()
                                {
                                    Name    = Whitelist[i].Name,
                                    Name2   = Whitelist[i].Name2,
                                    Value   = ""
                                });
                                Logger(String.Format("adding new field {0}...", Whitelist[i].Name));
                            }
                            a = 0;
                        }
                    }
                }
                catch
                {
                    Logger("ERROR merging fields. Old whitelist not used.");
                }

                f_New = new ObservableCollection<Fields>(Report_Contents);
                cVars_New.ItemsSource = f_New;
                cWhitelist_Update.IsEnabled = true;

                Logger("fields imported.");
                lFields1.Content = String.Format("{0} fields", Report_Contents.Count);
            }
            catch
            {
                Logger("ERROR importing fields.");
            }

        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Whitelist h_Whitelist = new Whitelist();
                f_Current.Clear();
                f_Current = new ObservableCollection<Fields>(h_Whitelist.FieldList(file_Whitelist));
                cVars_Current.ItemsSource = f_Current;

                Logger("refreshing current whitelist.");
            }
            catch
            {
                Logger("ERROR refreshing current whitelist.");
            }

        }

        private void cWhitelist_Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(file_Whitelist);
                if (fi.Exists)
                {
                    try
                    {
                        Logger("checking for previous whitelist backup... if yes, delete.");
                        File.Delete(file_bak_Whitelist);
                    }
                    catch
                    {
                        Logger("no backup found.");
                    }
                    fi.MoveTo(file_bak_Whitelist);
                }

                // Write to new file.
                List<Fields> Report_Contents    = new List<Fields>(f_New);
                StreamWriter Writer             = new StreamWriter(file_Whitelist);
                StringBuilder Builder           = new StringBuilder();

                string Line = "";

                for (int i = 0; i < Report_Contents.Count; i++)
                {
                    Line += String.Format("{0}\t{1}", Report_Contents[i].Name, Report_Contents[i].Name2);
                    if (i < (Report_Contents.Count - 1))
                    {
                        Line += "\n";
                    }
                }

                Builder.AppendLine(Line);
                Writer.Write(Builder);
                Writer.Flush();
                Builder.Clear();
                Writer.Close();

                Whitelist h_Whitelist           = new Whitelist();
                f_Current                       = new ObservableCollection<Fields>(h_Whitelist.FieldList(file_Whitelist));
                cVars_Current.ItemsSource       = f_Current;

                lFields2.Content = String.Format("{0} fields", cVars_Current.Items.Count);
                Logger("whitelist updated, backup created.");
            }
            catch
            {
                Logger("ERROR updating whitelist.");
            }
        }

        private void cWhitelistCurrent_Update_Click(object sender, RoutedEventArgs e)
        {
            List<Fields> Report_Contents = new List<Fields>(f_Current);

            try
            {
                StreamWriter Writer     = new StreamWriter(file_Whitelist);
                StringBuilder Builder   = new StringBuilder();
                string Line             = "";

                for (int i = 0; i < Report_Contents.Count; i++)
                {
                    Line += String.Format("{0}\t{1}", Report_Contents[i].Name, Report_Contents[i].Name2);
                    if (i < (Report_Contents.Count - 1))
                    {
                        Line += "\n";
                    }
                }

                Builder.AppendLine(Line);
                Writer.Write(Builder);
                Writer.Flush();
                Builder.Clear();
                Writer.Close();

                cWhitelist_Update.IsEnabled = true;

                Logger("whitelist updated.");
            }
            catch
            {
                Logger("ERROR updating whitelist.");
            }
        }

        private void bNewUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cVars_New.SelectedIndex >= 0)
                {
                    // Get index of selected field.
                    int i = cVars_New.SelectedIndex;
                    int j = i - 1;
                    string TempName     = f_New[i].Name;
                    string TempName2    = f_New[i].Name2;
                    string TempValue    = f_New[i].Value;

                    f_New[i].Name   = f_New[j].Name;
                    f_New[i].Name2  = f_New[j].Name2;
                    f_New[i].Value  = f_New[j].Value;

                    f_New[j].Name   = TempName;
                    f_New[j].Name2  = TempName2;
                    f_New[j].Value  = TempValue;

                    Logger("field moved.");

                    cVars_New.SelectedIndex = j;
                }
                else
                {
                    Logger("select field to move.");
                }
            }
            catch
            {
                Logger("ERROR re-ordering new whitelist.");
            }
        }

        private void bNewDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cVars_New.SelectedIndex >= 0)
                {
                    // Get index of selected field.
                    int i = cVars_New.SelectedIndex;
                    int j = i + 1;
                    string TempName     = f_New[i].Name;
                    string TempName2    = f_New[i].Name2;
                    string TempValue    = f_New[i].Value;

                    f_New[i].Name   = f_New[j].Name;
                    f_New[i].Name2  = f_New[j].Name2;
                    f_New[i].Value  = f_New[j].Value;

                    f_New[j].Name   = TempName;
                    f_New[j].Name2  = TempName2;
                    f_New[j].Value  = TempValue;

                    Logger("field moved.");

                    cVars_New.SelectedIndex = j;
                }
                else
                {
                    Logger("select field to move.");
                }
            }
            catch
            {
                Logger("ERROR re-ordering new whitelist.");
            }
        }

        private void bCurrentUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cVars_Current.SelectedIndex >= 0)
                {
                    // Get index of selected field.
                    int i = cVars_Current.SelectedIndex;
                    int j = i - 1;
                    string TempName         = f_Current[i].Name;
                    string TempName2        = f_Current[i].Name2;
                    string TempValue        = f_Current[i].Value;

                    f_Current[i].Name   = f_Current[j].Name;
                    f_Current[i].Name2  = f_Current[j].Name2;
                    f_Current[i].Value  = f_Current[j].Value;

                    f_Current[j].Name   = TempName;
                    f_Current[j].Name2  = TempName2;
                    f_Current[j].Value  = TempValue;

                    Logger("field moved.");

                    cVars_Current.SelectedIndex = j;
                }
                else
                {
                    Logger("select field to move.");
                }
            }
            catch
            {
                Logger("ERROR re-ordering new whitelist.");
            }
        }

        private void bCurrentDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cVars_Current.SelectedIndex >= 0)
                {
                    // Get index of selected field.
                    int i = cVars_Current.SelectedIndex;
                    int j = i + 1;
                    string TempName         = f_Current[i].Name;
                    string TempName2        = f_Current[i].Name2;
                    string TempValue        = f_Current[i].Value;

                    f_Current[i].Name   = f_Current[j].Name;
                    f_Current[i].Name2  = f_Current[j].Name2;
                    f_Current[i].Value  = f_Current[j].Value;

                    f_Current[j].Name   = TempName;
                    f_Current[j].Name2  = TempName2;
                    f_Current[j].Value  = TempValue;

                    Logger("field moved.");

                    cVars_Current.SelectedIndex = j;
                }
                else
                {
                    Logger("select field to move.");
                }
            }
            catch
            {
                Logger("ERROR re-ordering new whitelist.");
            }
        }
    }
}