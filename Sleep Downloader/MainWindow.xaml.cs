using Spire.Doc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Sleep_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// Collections to interact with.
        /// 
        public ObservableCollection<Fields> FieldsNew;
        public ObservableCollection<Fields> FieldsCurrent;

        public MainWindow()
        {
            InitializeComponent();

            Interface h_Interface = new Interface();

            tOutput.Text += h_Interface.Logger("Sleep Downloader version 1.4.0\n");

            /// Populate current whitelist in setup tab.
            /// 
            Whitelist h_Whitelist = new Whitelist();

            try
            {
                FieldsCurrent = new ObservableCollection<Fields>(h_Whitelist.FieldList());
                cVariablelist_Current.ItemsSource = FieldsCurrent;
                lFields2.Content = String.Format("{0} fields", cVariablelist_Current.Items.Count);
                tOutput.Text += h_Interface.Logger("whitelist found");
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR fetching current whitelist.\n");
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();
            Whitelist h_Whitelist = new Whitelist();

            try
            {
                tOutput.Text += h_Interface.Logger("clearing lists...\n");

                if (FieldsNew != null)
                {
                    FieldsNew.Clear();
                }
                if (FieldsCurrent != null)
                {
                    FieldsCurrent.Clear();
                }

                FieldsCurrent = new ObservableCollection<Fields>(h_Whitelist.FieldList());
                cVariablelist_Current.ItemsSource = FieldsCurrent;
                lFields2.Content = String.Format("{0} fields", cVariablelist_Current.Items.Count);
                tOutput.Text += h_Interface.Logger("whitelist found.\n");
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR resetting... suggest restarting tool.\n");
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();

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
                tOutput.Text += h_Interface.Logger("populating archive list.\n");
                cArchiveList.ItemsSource = h_Archive.GetArchives(cFolderPath.Text);
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR obtaining archive list... stopping.\n");
                return;
            }
        }

        private void SelectWhitelist_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();

            List<Fields> Report_Contents = new List<Fields>();

            try
            {
                using (var Dialog = new OpenFileDialog())
                {
                    DialogResult FileName = Dialog.ShowDialog();

                    if (FileName == System.Windows.Forms.DialogResult.OK)
                    {
                        Document Report = new Document();

                        /// Load the report.
                        /// 
                        Report.LoadFromFile(Dialog.FileName);

                        /// Get field names from report.
                        ///
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

                /// Remove empty variables beginning with _.
                /// 
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

                /// Add report text as an extra field.
                /// 
                Report_Contents.Add(new Fields()
                {
                    Name    = "ReportText",
                    Name2   = "Report Text",
                    Value   = ""
                });

                /// Add path as an extra field.
                /// 
                Report_Contents.Add(new Fields()
                {
                    Name    = "Path",
                    Name2   = "Path",
                    Value   = ""
                });

                /// Add new variables to current list (if option selected).
                /// 
                try
                {
                    if (cUpdateOption.IsChecked == true)
                    {
                        Whitelist h_Whitelist = new Whitelist();
                        List<Fields> Whitelist = new List<Fields>();

                        Whitelist = h_Whitelist.FieldList();

                        tOutput.Text += h_Interface.Logger("merging lists...\n");
                        System.Windows.Forms.Application.DoEvents();

                        /// Work through each old field and add if it doesn't exist in the new list.
                        ///
                        int a = 0;

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

                                tOutput.Text += h_Interface.Logger(String.Format("adding new field {0}...\n", Whitelist[i].Name));
                                System.Windows.Forms.Application.DoEvents();
                            }

                            a = 0;
                        }
                    }
                }
                catch
                {
                    tOutput.Text += h_Interface.Logger("ERROR merging fields. Old whitelist not used.\n");
                }

                FieldsNew = new ObservableCollection<Fields>(Report_Contents);
                cVariablelist_New.ItemsSource = FieldsNew;

                cWhitelist_Update.IsEnabled = true;

                tOutput.Text += h_Interface.Logger("fields imported.\n");
                lFields1.Content = String.Format("{0} fields", Report_Contents.Count);
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR importing fields.\n");
            }

        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();

            try
            {
                Whitelist h_Whitelist = new Whitelist();
                FieldsCurrent.Clear();
                FieldsCurrent = new ObservableCollection<Fields>(h_Whitelist.FieldList());
                cVariablelist_Current.ItemsSource = FieldsCurrent;

                tOutput.Text += h_Interface.Logger("refreshing current whitelist.\n");
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR refreshing current whitelist.\n");
            }

        }

        private void cWhitelist_Update_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();

            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo("Whitelist.txt");
                if (fi.Exists)
                {
                    // Move file with a new name. Hence renamed.  
                    try
                    {
                        tOutput.Text += h_Interface.Logger("checking for previous whitelist backup... if yes, delete.\n");
                        File.Delete("Whitelist.txt.bak");
                    }
                    catch
                    {
                        tOutput.Text += h_Interface.Logger("no backup found.\n");
                    }
                    fi.MoveTo("Whitelist.txt.bak");
                }

                /// Write to new file.
                /// 
                List<Fields> Report_Contents    = new List<Fields>(FieldsNew);
                StreamWriter Writer             = new StreamWriter("Whitelist.txt");
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
                FieldsCurrent                       = new ObservableCollection<Fields>(h_Whitelist.FieldList());
                cVariablelist_Current.ItemsSource   = FieldsCurrent;

                lFields2.Content = String.Format("{0} fields", cVariablelist_Current.Items.Count);
                tOutput.Text += h_Interface.Logger("whitelist updated, backup created.\n");
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR updating whitelist.\n");
            }
        }

        private void cWhitelistCurrent_Update_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();

            List<Fields> Report_Contents = new List<Fields>(FieldsCurrent);

            try
            {
                StreamWriter Writer     = new StreamWriter("Whitelist.txt");
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

                tOutput.Text += h_Interface.Logger("whitelist updated.\n");
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR updating whitelist.\n");
            }
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();
            Whitelist h_Whitelist = new Whitelist();

            Fetch h_Archive = new Fetch();

            List<Archives> l_Archive = new List<Archives>();

            if (cOutputEnable.IsChecked == false)
            {
                tOutput.Text += h_Interface.Logger("output disbled. Running...\n");
            }

            /// Get archive list.
            /// 
            try
            {
                l_Archive = h_Archive.GetArchives(cFolderPath.Text);
            }
            catch
            {
                if (cOutputEnable.IsChecked == true)
                {
                    tOutput.Text += h_Interface.Logger("ERROR obtaining archive list... stopping.\n");
                    return;
                }
            }

            /// Populate field whitelist.
            /// 
            List<Fields> Whitelist = new List<Fields>();

            try
            {
                if (cOutputEnable.IsChecked == true)
                {
                    tOutput.Text += h_Interface.Logger("refetching whitelist.\n");
                }
                Whitelist = h_Whitelist.FieldList();
            }
            catch
            {
                if (cOutputEnable.IsChecked == true)
                {
                    tOutput.Text += h_Interface.Logger("ERROR applying whitelist... stopping.\n");
                }
                return;
            }

            /// Setup.
            /// 
            String OutputFile                   = String.Format(@"Output\{0}", cFileName.Text);
            StreamWriter Writer                 = new StreamWriter(OutputFile);
            StreamWriter LogWriter              = new StreamWriter(String.Format(@"Output\Log.txt"));
            StreamWriter MissingReportWriter    = new StreamWriter(String.Format(@"Output\Missing Reports.txt"));
            StringBuilder Builder               = new StringBuilder();
            StringBuilder LogBuilder            = new StringBuilder();
            StringBuilder MissingReportBuilder  = new StringBuilder();

            /// Variables to be used later.
            /// 
            int RepeatCheck = 0;
            if (cFilterCheck.IsChecked == true)
            {
                RepeatCheck = 1;
            }
            string Line                 = "";
            double Counter              = 0;
            double ErrorCounter         = 0;
            double MissingReportCounter = 0;
            bool AlignmentFlag = false; /// Used to align whitelist and extracted report values.

            /// Format missing report log.
            /// 
            MissingReportBuilder.AppendLine(String.Format("Archive\tStudy"));

            l_Archive.ForEach(delegate (Archives Archive)
            {
                if (Archive.Selected)
                {
                    try
                    {
                        if (cOutputEnable.IsChecked == true)
                        {
                            tOutput.Text += h_Interface.Logger("opening archive...\n");
                        }
                        List<Studies> l_Study = new List<Studies>();
                        l_Study = h_Archive.GetStudies(cFolderPath.Text, Archive.Name);

                        System.Windows.Forms.Application.DoEvents();

                        /// Open study and find report according to filter settings.
                        l_Study.ForEach(delegate (Studies Study)
                        {
                            try
                            {
                                if (cOutputEnable.IsChecked == true)
                                {
                                    tOutput.Text += h_Interface.Logger(String.Format("opening study {0}...\n", Study.Name));
                                }
                                List<Reports> l_Report = new List<Reports>();
                                l_Report = h_Archive.GetReports(cFolderPath.Text, Archive.Name, Study.Name, cSelect.Text, cFilter.Text, RepeatCheck);

                                System.Windows.Forms.Application.DoEvents();

                                /// Check if report is missing and add to list.
                                /// 
                                if (l_Report.Count == 0)
                                {
                                    MissingReportBuilder.AppendLine(String.Format("{0}\t{1}", Archive.Name, Study.Name));
                                    if (cOutputEnable.IsChecked == true)
                                    {
                                        tOutput.Text += h_Interface.Logger(String.Format("missing report for study {0}, in archive {1}...\n", Study.Name, Archive.Name));
                                    }
                                    MissingReportCounter++;
                                }

                                l_Report.ForEach(delegate (Reports Report)
                                {
                                    try
                                    {
                                        if (cOutputEnable.IsChecked == true)
                                        {
                                            tOutput.Text += h_Interface.Logger(String.Format("opening report {0}...\n", Report.Name));
                                        }
                                        Stream h_Stream = new Stream();
                                        List<Fields> Report_Contents = new List<Fields>();
                                        Report_Contents = h_Stream.GetReport(Report.Name, Whitelist, Convert.ToInt32(cReportTextValue.Text), Convert.ToInt32(cReportTextValuePost.Text));

                                        System.Windows.Forms.Application.DoEvents();

                                        if (Counter == 0)
                                        {
                                            /// Write headers from whitelist (sets structure of output).
                                            /// 
                                            for (int j = 0; j < Whitelist.Count; j++)
                                            {
                                                Line = Line + String.Format("{0}\t", Whitelist[j].Name2);
                                            }
                                            if (cOutputEnable.IsChecked == true)
                                            {
                                                tOutput.Text += h_Interface.Logger("writing headers from whitelist...\n");
                                            }
                                            Builder.AppendLine(Line);
                                            System.Windows.Forms.Application.DoEvents();
                                        }

                                        /// Ready for next line.
                                        Line = "";

                                        /// Write field values.
                                        /// 
                                        for (int i = 0; i < Whitelist.Count; i++)
                                        {
                                            AlignmentFlag = false;
                                            for (int j = 0; j < Report_Contents.Count; j++)
                                            {
                                                if (Report_Contents[j].Name == Whitelist[i].Name)
                                                {
                                                    Line = Line + String.Format("{0}\t", Report_Contents[j].Value);
                                                    AlignmentFlag = true;
                                                }
                                            }
                                            if (AlignmentFlag == false)
                                            {
                                                Line = Line + "-\t";
                                            }
                                        }

                                        if (cOutputEnable.IsChecked == true)
                                        {
                                            tOutput.Text += h_Interface.Logger(String.Format("writing values from {0}...\n", Report.Name));
                                        }
                                        Builder.AppendLine(Line);
                                        System.Windows.Forms.Application.DoEvents();

                                        Line = "";

                                        Counter++;
                                        lCount.Content = String.Format("{0} reports imported, {1} reports skipped due to errors, {2} missing reports", Counter, ErrorCounter, MissingReportCounter);

                                        ///Write and clear buffer.
                                        ///
                                        Writer.Write(Builder);
                                        Writer.Flush();
                                        Builder.Clear();

                                        System.Windows.Forms.Application.DoEvents();
                                    }
                                    catch
                                    {
                                        if (cSkip.IsChecked == true)
                                        {
                                            if (cOutputEnable.IsChecked == true)
                                            {
                                                tOutput.Text += h_Interface.Logger(String.Format("ERROR opening report {0}... skipping...\n", Report.Name));
                                            }
                                            ErrorCounter++;
                                            System.Windows.Forms.Application.DoEvents();
                                        }
                                        else
                                        {
                                            if (cOutputEnable.IsChecked == true)
                                            {
                                                tOutput.Text += h_Interface.Logger(String.Format("ERROR opening report {0}... stopping.\n", Report.Name));
                                            }
                                            ErrorCounter++;
                                            return;
                                        }
                                    }
                                });
                            }
                            catch
                            {
                                if (cSkip.IsChecked == true)
                                {
                                    if (cOutputEnable.IsChecked == true)
                                    {
                                        tOutput.Text += h_Interface.Logger(String.Format("ERROR opening study {0}... skipping...\n", Study.Name));
                                    }
                                    ErrorCounter++;
                                    System.Windows.Forms.Application.DoEvents();
                                }
                                else
                                {
                                    if (cOutputEnable.IsChecked == true)
                                    {
                                        tOutput.Text += h_Interface.Logger(String.Format("ERROR opening study {0}... stopping.\n", Study.Name));
                                    }
                                    ErrorCounter++;
                                    return;
                                }
                            }
                        });
                    }
                    catch
                    {
                        if (cSkip.IsChecked == true)
                        {
                            if (cOutputEnable.IsChecked == true)
                            {
                                tOutput.Text += h_Interface.Logger(String.Format("ERROR opening archive {0}... skipping...\n", Archive.Name));
                            }
                            ErrorCounter++;
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            if (cOutputEnable.IsChecked == true)
                            {
                                tOutput.Text += h_Interface.Logger(String.Format("ERROR opening archive {0}... skipping...\n", Archive.Name));
                            }
                            ErrorCounter++;
                            return;
                        }
                    }
                }
            });

            /// Write log file if option checked.
            /// 
            if (cOutputLog.IsChecked == true)
            {
                try
                {
                    LogBuilder.Append(tOutput.Text);
                    LogWriter.Write(LogBuilder);
                    LogWriter.Flush();
                    LogBuilder.Clear();
                    tOutput.Text += h_Interface.Logger("output log written.\n");

                }
                catch
                {
                    tOutput.Text += h_Interface.Logger("ERROR writing output log.\n");
                }
            }

            /// Write missing reports list if checked.
            /// 
            if (cMissingReports.IsChecked == true)
            {
                try
                {
                    MissingReportWriter.Write(MissingReportBuilder);
                    MissingReportWriter.Flush();
                    MissingReportBuilder.Clear();
                    tOutput.Text += h_Interface.Logger("missing reports log written.\n");

                }
                catch
                {
                    tOutput.Text += h_Interface.Logger("ERROR writing missing reports log.\n");
                }
            }

            /// Finish and close files.
            /// 
            Writer.Close();
            LogWriter.Close();
            MissingReportWriter.Close();
            tOutput.Text += h_Interface.Logger("finished.\n");
        }

        private void bNewUp_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();

            try
            {
                if (cVariablelist_New.SelectedIndex >= 0)
                {
                    /// Get index of selected field.
                    /// 
                    int i = cVariablelist_New.SelectedIndex;
                    int j = i - 1;
                    string TempName     = FieldsNew[i].Name;
                    string TempName2    = FieldsNew[i].Name2;
                    string TempValue    = FieldsNew[i].Value;

                    FieldsNew[i].Name   = FieldsNew[j].Name;
                    FieldsNew[i].Name2  = FieldsNew[j].Name2;
                    FieldsNew[i].Value  = FieldsNew[j].Value;

                    FieldsNew[j].Name   = TempName;
                    FieldsNew[j].Name2  = TempName2;
                    FieldsNew[j].Value  = TempValue;

                    tOutput.Text += h_Interface.Logger("field moved.\n");

                    cVariablelist_New.SelectedIndex = j;
                }
                else
                {
                    tOutput.Text += h_Interface.Logger("select field to move.\n");
                }
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR re-ordering new whitelist.\n");
            }
        }

        private void bNewDown_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();

            try
            {
                if (cVariablelist_New.SelectedIndex >= 0)
                {
                    /// Get index of selected field.
                    /// 
                    int i = cVariablelist_New.SelectedIndex;
                    int j = i + 1;
                    string TempName     = FieldsNew[i].Name;
                    string TempName2    = FieldsNew[i].Name2;
                    string TempValue    = FieldsNew[i].Value;

                    FieldsNew[i].Name   = FieldsNew[j].Name;
                    FieldsNew[i].Name2  = FieldsNew[j].Name2;
                    FieldsNew[i].Value  = FieldsNew[j].Value;

                    FieldsNew[j].Name   = TempName;
                    FieldsNew[j].Name2  = TempName2;
                    FieldsNew[j].Value  = TempValue;

                    tOutput.Text += h_Interface.Logger("field moved.\n");

                    cVariablelist_New.SelectedIndex = j;
                }
                else
                {
                    tOutput.Text += h_Interface.Logger("select field to move.\n");
                }
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR re-ordering new whitelist.\n");
            }
        }

        private void bCurrentUp_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();

            try
            {
                if (cVariablelist_Current.SelectedIndex >= 0)
                {
                    /// Get index of selected field.
                    /// 
                    int i = cVariablelist_Current.SelectedIndex;
                    int j = i - 1;
                    string TempName         = FieldsCurrent[i].Name;
                    string TempName2        = FieldsCurrent[i].Name2;
                    string TempValue        = FieldsCurrent[i].Value;

                    FieldsCurrent[i].Name   = FieldsCurrent[j].Name;
                    FieldsCurrent[i].Name2  = FieldsCurrent[j].Name2;
                    FieldsCurrent[i].Value  = FieldsCurrent[j].Value;

                    FieldsCurrent[j].Name   = TempName;
                    FieldsCurrent[j].Name2  = TempName2;
                    FieldsCurrent[j].Value  = TempValue;

                    tOutput.Text += h_Interface.Logger("field moved.\n");

                    cVariablelist_Current.SelectedIndex = j;
                }
                else
                {
                    tOutput.Text += h_Interface.Logger("select field to move.\n");
                }
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR re-ordering new whitelist.\n");
            }
        }

        private void bCurrentDown_Click(object sender, RoutedEventArgs e)
        {
            Interface h_Interface = new Interface();

            try
            {
                if (cVariablelist_Current.SelectedIndex >= 0)
                {
                    /// Get index of selected field.
                    /// 
                    int i = cVariablelist_Current.SelectedIndex;
                    int j = i + 1;
                    string TempName         = FieldsCurrent[i].Name;
                    string TempName2        = FieldsCurrent[i].Name2;
                    string TempValue        = FieldsCurrent[i].Value;

                    FieldsCurrent[i].Name   = FieldsCurrent[j].Name;
                    FieldsCurrent[i].Name2  = FieldsCurrent[j].Name2;
                    FieldsCurrent[i].Value  = FieldsCurrent[j].Value;

                    FieldsCurrent[j].Name   = TempName;
                    FieldsCurrent[j].Name2  = TempName2;
                    FieldsCurrent[j].Value  = TempValue;

                    tOutput.Text += h_Interface.Logger("field moved.\n");

                    cVariablelist_Current.SelectedIndex = j;
                }
                else
                {
                    tOutput.Text += h_Interface.Logger("select field to move.\n");
                }
            }
            catch
            {
                tOutput.Text += h_Interface.Logger("ERROR re-ordering new whitelist.\n");
            }
        }
    }
}