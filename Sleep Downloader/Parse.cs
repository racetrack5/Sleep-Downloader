using Spire.Doc;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sleep_Downloader
{
    class Parse
    {
        public List<Fields> GetReport(string ReportFile, List<Fields> Whitelist, int i_PreCull, int i_PostCull)
        {
            Document Report = new Document();

            // Load the report.
            Report.LoadFromFile(ReportFile);

            List<Fields> Report_Contents = new List<Fields>();

            // Get field names from report.
            for (int i = 0; i < Report.Variables.Count; i++)
            {
                Report_Contents.Add(new Fields()
                {
                    Name = Report.Variables.GetNameByIndex(i),
                    Value = Report.Variables.GetValueByIndex(i)
                });

                // Remove blanks to avoid weirdness.
                if (Report_Contents[i].Value == "")
                {
                    Report_Contents[i].Value = "-";
                }

                // Remove tabs, returns, new lines or excessive spaces that will act as delimiters or otherwise change the output.
                Regex rgx = new Regex("\t|\r|\n|\\s+");
                string Result = rgx.Replace(Report_Contents[i].Value, " ");
                Report_Contents[i].Value = Result;

            }

            // Remove empty variables beginning with _.
            int j = 0;
            int l = -1;

            for (int i = 0; i < Report_Contents.Count; i++)
            {
                if (Report_Contents[i].Name.StartsWith("_"))
                {
                    j++;
                    if (l < 0)
                    {
                        l = i;
                    }
                }
                else
                {
                    if (j > 0)
                    {
                        Report_Contents.RemoveRange(l, j);
                        j = 0;
                        l = -1;
                    }
                }
            }

            // Retrieve report text. 
            string ReportText = Report.GetText();

            // Cull report to "Max CO2" - only reliable marker in different report versions.
            ReportText = ReportText.Substring(ReportText.IndexOf("Max CO2"));


            // Cull report a few chars after "Consultant" for service planning.
            int index = ReportText.IndexOf("Consultant");
            if (index >= i_PostCull)
            {
                ReportText = ReportText.Substring(i_PreCull, index);
            }

            Regex rgx2 = new Regex("\t|\r|\n|\\s+");
            string Result2 = rgx2.Replace(ReportText, " ");
            ReportText = Result2;

            // Add report text as an extra field.
            Report_Contents.Add(new Fields()
            {
                Name = "ReportText",
                Value = ReportText
            });

            // Add path as an extra field.
            Report_Contents.Add(new Fields()
            {
                Name = "Path",
                Value = ReportFile
            });

            // Align whitelist with report fields (so the program is not sensitive to report changes) into a seperate list to write back.
            List<Fields> WriteValues = new List<Fields>();

            // Grab field from whitelist.
            for (int i = 0; i < Whitelist.Count; i++)
            {
                // Find the report field to match the whitelist field.
                for (int k = 0; k < Report_Contents.Count; k++)
                {
                    if (Whitelist[i].Name == Report_Contents[k].Name)
                    {
                        WriteValues.Add(new Fields()
                        {
                            Name = Report_Contents[k].Name,
                            Value = Report_Contents[k].Value
                        });
                    }
                }
            }

            // Cull duplicate fields (appears to happen with the name field). Culls forwards, not backwards.
            for (int i = 0; i < WriteValues.Count; i++)
            {
                for (int k = i + 1; k < WriteValues.Count; k++)
                {
                    if (WriteValues[k].Name == WriteValues[i].Name)
                    {
                        WriteValues.RemoveAt(k);
                    }
                }
            }

            return WriteValues;
        }
    }
}
