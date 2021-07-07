using Spire.Doc;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text.RegularExpressions;

namespace Sleep_Downloader
{
    class StreamFields
    {
        public List<Fields> GetReport (string ReportFile, List<Fields_Whitelist> Whitelist)
        {
            Document Report = new Document();

            /// Load the report.
            /// 
            Report.LoadFromFile(ReportFile);

            List<Fields> ReportValues = new List<Fields>();

            /// Get field names from report.
            ///
            for (int i = 0; i < Report.Variables.Count; i++)
            {
                ReportValues.Add(new Fields()
                {
                    Name = Report.Variables.GetNameByIndex(i),
                    Value = Report.Variables.GetValueByIndex(i)
                });

                /// Remove blanks to avoid weirdness.
                /// 
                if (ReportValues[i].Value == "")
                {
                    ReportValues[i].Value = "-";
                }

                /// Remove tabs, returns, new lines or excessive spaces that will act as delimiters or otherwise change the output.
                /// 
                Regex rgx = new Regex("\t|\r|\n|\\s+");
                string Result = rgx.Replace(ReportValues[i].Value, " ");
                ReportValues[i].Value = Result;
                
            }

            /// Remove empty variables beginning with _.
            /// 
            ///int j = 0;
            for (int i = 0; i < ReportValues.Count; i++)
            {
                if (FileSystemName.MatchesSimpleExpression("_*", ReportValues[i].Name))
                {
                    ///j++;
                    ///
                    ReportValues.RemoveAt(i);
                }
            }
            ///ReportValues.RemoveRange(0, j);

            /// Retrieve report text.
            /// 
            string ReportText = Report.GetText();

            /// Cull report to "Max CO2" - only reliable marker in different report versions.
            /// 
            ReportText = ReportText.Substring(ReportText.IndexOf("Max CO2"));


            /// Cull report after "Signed" - reliable point after report text.
            /// 
            int index = ReportText.IndexOf("Signed");
            if (index >= 0)
            {
                ReportText = ReportText.Substring(24, index);
            }

            Regex rgx2 = new Regex("\t|\r|\n|\\s+");
            string Result2 = rgx2.Replace(ReportText, " ");
            ReportText = Result2;

            /// Add report text as an extra field.
            /// 
            ReportValues.Add(new Fields()
            {
                Name = "ReportText",
                Value = ReportText
            });

            /// Add archive and full path as extra fields.
            /// 


            /// Align whitelist with report fields (so the program is not sensitive to report changes) into a seperate list to write back.
            /// 
            List<Fields> WriteValues = new List<Fields>();
            
            /// Grab field from whitelist.
            /// 
            for (int i = 0; i < Whitelist.Count; i++)
            {
                /// Find the report field to match the whitelist field.
                /// 
                for (int k = 0; k < ReportValues.Count; k++)
                {
                    if (Whitelist[i].Name == ReportValues[k].Name)
                    {
                        WriteValues.Add(new Fields()
                        {
                            Name = ReportValues[k].Name,
                            Value = ReportValues[k].Value
                        });
                    }
                    /// Could put information about discarded fields here?
                    /// 
                }
            }

            /// Cull duplicate fields (appears to happen with the name field). Culls forwards, not backwards.
            /// 

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
