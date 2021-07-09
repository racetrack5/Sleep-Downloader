using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sleep_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            /// Populate current whitelist in setup tab.
            /// 
            Whitelist WhitelistHandle = new Whitelist();
            List<Fields_Whitelist> Whitelist = new List<Fields_Whitelist>();

            try
            {
                cVariablelist_Current.ItemsSource = WhitelistHandle.FilterFields();
                tOutput.Text += String.Format("{0} - whitelist found.\n", DateTime.Now);
            }
            catch
            {
                tOutput.Text += String.Format("{0} - ERROR fetching current whitelist...\n", DateTime.Now);
                return;
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var Dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult FolderName = Dialog.ShowDialog();

                if (FolderName == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(Dialog.SelectedPath))
                {
                    cFolderPath.Text = Dialog.SelectedPath;
                }
            }
            IO ArchiveHandle = new IO();
            List<Archives> ArchiveList = new List<Archives>();

            try
            {
                tOutput.Text += String.Format("{0} - populating archive list.\n", DateTime.Now);
                cArchiveList.ItemsSource = ArchiveHandle.GetArchives(cFolderPath.Text);
            }
            catch
            {
                tOutput.Text += String.Format("{0} - ERROR obtaining archive list... stopping.\n", DateTime.Now);
                return;
            }
        }

        private void SelectWhitelist_Click(object sender, RoutedEventArgs e)
        {
            Whitelist WhitelistHandle = new Whitelist();
            List<Fields_Whitelist> Whitelist = new List<Fields_Whitelist>();

            using (var Dialog = new System.Windows.Forms.OpenFileDialog())
            {
                System.Windows.Forms.DialogResult FileName = Dialog.ShowDialog();

                if (FileName == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(Dialog.FileName))
                {
                    cVariablelist_New.ItemsSource = WhitelistHandle.FetchFields(Convert.ToString(FileName));
                }
            }
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            IO ArchiveHandle = new IO();
            List<Archives> ArchiveList = new List<Archives>();

            /// Get archive list.
            /// 
            try
            {
                ArchiveList = ArchiveHandle.GetArchives(cFolderPath.Text);
            }
            catch
            {
                tOutput.Text += String.Format("{0} - ERROR obtaining archive list... stopping.\n", DateTime.Now);
                return;
            }

            /// Populate field whitelist.
            /// 
            Whitelist WhitelistHandle = new Whitelist();
            List<Fields_Whitelist> Whitelist = new List<Fields_Whitelist>();

            try
            {
                Whitelist = WhitelistHandle.FilterFields();
            }
            catch
            {
                tOutput.Text += String.Format("{0} - ERROR applying whitelist... stopping.\n", DateTime.Now);
                return;
            }

            /// Setup.
            /// 
            String          OutputFile = String.Format(@"Output\{0}", cFileName.Text);
            StreamWriter    Writer = new StreamWriter(OutputFile);
            StreamWriter    LogWriter = new StreamWriter(String.Format(@"Output\Log.txt"));
            StreamWriter    MissingReportWriter = new StreamWriter(String.Format(@"Output\Missing Reports.txt"));
            StringBuilder   Builder = new StringBuilder();
            StringBuilder   LogBuilder = new StringBuilder();
            StringBuilder   MissingReportBuilder = new StringBuilder();

            /// Variables to be used later.
            /// 
            string  Line = "";
            double  Counter = 0;
            double  ErrorCounter = 0;
            double  MissingReportCounter = 0;
            bool    AlignmentFlag = false; /// Used to align whitelist and extracted report values.

            /// Format missing report log.
            /// 
            MissingReportBuilder.AppendLine(String.Format("Archive\tStudy"));

            ArchiveList.ForEach(delegate (Archives Archive)
            {
                if (Archive.Selected)
                {
                    try
                    {
                        tOutput.Text += String.Format("{0} - opening archive {1}...\n", DateTime.Now, Archive.Name);
                        List<Studies> StudyList = new List<Studies>();
                        StudyList = ArchiveHandle.GetStudies(cFolderPath.Text, Archive.Name);

                        System.Windows.Forms.Application.DoEvents();

                        /// Open study and find report according to filter settings.
                        StudyList.ForEach(delegate (Studies Study)
                        {
                            try
                            {
                                tOutput.Text += String.Format("{0} - opening study {1}...\n", DateTime.Now, Study.Name);
                                List<Reports> ReportList = new List<Reports>();
                                ReportList = ArchiveHandle.GetReports(cFolderPath.Text, Archive.Name, Study.Name, cSelect.Text, cFilter.Text);

                                System.Windows.Forms.Application.DoEvents();

                                /// Check if report is missing and add to list.
                                /// 
                                if (ReportList.Count == 0)
                                {
                                    MissingReportBuilder.AppendLine(String.Format("{0}\t{1}", Archive.Name, Study.Name));
                                    tOutput.Text += String.Format("{0} - missing report for study {1}, in archive {2}...\n", DateTime.Now, Study.Name, Archive.Name);
                                    MissingReportCounter++;
                                }

                                ReportList.ForEach(delegate (Reports Report)
                                {
                                    try
                                    {
                                        tOutput.Text += String.Format("{0} - opening report {1}...\n", DateTime.Now, Report.Name);
                                        StreamFields StreamHandle = new StreamFields();
                                        List<Fields> ReportValues = new List<Fields>();
                                        ReportValues = StreamHandle.GetReport(Report.Name, Whitelist);

                                        System.Windows.Forms.Application.DoEvents();

                                        if (Counter == 0)
                                        {
                                            /// Write headers from whitelist (sets structure of output).
                                            /// 
                                            for (int j = 0; j < Whitelist.Count; j++)
                                            {
                                                    Line = Line + String.Format("{0}\t", Whitelist[j].Name);
                                            }
                                            tOutput.Text += String.Format("{0} - writing headers from whitelist...\n", DateTime.Now);
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
                                            for (int j = 0; j < ReportValues.Count; j++)
                                            {
                                                if (ReportValues[j].Name == Whitelist[i].Name)
                                                {
                                                    Line = Line + String.Format("{0}\t", ReportValues[j].Value);
                                                    AlignmentFlag = true;
                                                }
                                            }
                                            if (AlignmentFlag == false)
                                            {
                                                Line = Line + "-\t";
                                            }
                                        }

                                        tOutput.Text += String.Format("{0} - writing values from {1}...\n", DateTime.Now, Report.Name);
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
                                            tOutput.Text += String.Format("{0} - ERROR opening report {1}... skipping...\n", DateTime.Now, Report.Name);
                                            ErrorCounter++;
                                            System.Windows.Forms.Application.DoEvents();
                                        }
                                        else
                                        {
                                            tOutput.Text += String.Format("{0} - ERROR opening report {1}... stopping.\n", DateTime.Now, Report.Name);
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
                                    tOutput.Text += String.Format("{0} - ERROR opening study {1}... skipping...\n", DateTime.Now, Study.Name);
                                    ErrorCounter++;
                                    System.Windows.Forms.Application.DoEvents();
                                }
                                else
                                {
                                    tOutput.Text += String.Format("{0} - ERROR opening study {1}... stopping.\n", DateTime.Now, Study.Name);
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
                            tOutput.Text += String.Format("{0} - ERROR opening archive {1}... skipping...\n", DateTime.Now, Archive.Name);
                            ErrorCounter++;
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            tOutput.Text += String.Format("{0} - ERROR opening archive {1}... stopping.\n", DateTime.Now, Archive.Name);
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
                    tOutput.Text += String.Format("{0} - output log written.\n", DateTime.Now);

                }
                catch
                {
                    tOutput.Text += String.Format("{0} - ERROR writing output log.\n", DateTime.Now);
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
                    tOutput.Text += String.Format("{0} - missing reports log written.\n", DateTime.Now);

                }
                catch
                {
                    tOutput.Text += String.Format("{0} - ERROR writing missing reports log.\n", DateTime.Now);
                }
            }

            /// Finish.
            /// 
            Writer.Close();
            tOutput.Text += String.Format("{0} - finished.\n", DateTime.Now);
        }

    }
}
