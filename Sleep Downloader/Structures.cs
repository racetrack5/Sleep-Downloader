using System;

namespace Sleep_Downloader
{
    public class Fields_Whitelist
    {
        /// Class just for whitelist purposes (minus values).
        public string Name { get; set; }
    }

    public class Fields : IComparable<Fields>
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int CompareTo(Fields compareFields)
        {
            // A null value means that this object is greater.
            if (compareFields == null)
                return 1;

            else
                return this.Name.CompareTo(compareFields.Name);
        }
    }

    public class Reports : IComparable<Reports>
    {
        public string Name { get; set; }
        public int CompareTo(Reports compareReports)
        {
            // A null value means that this object is greater.
            if (compareReports == null)
                return 1;

            else
                return this.Name.CompareTo(compareReports.Name);
        }
    }

    public class Studies : IComparable<Studies>
    {
        public string Name { get; set; }
        public int CompareTo(Studies compareStudies)
        {
            // A null value means that this object is greater.
            if (compareStudies == null)
                return 1;

            else
                return this.Name.CompareTo(compareStudies.Name);
        }
    }

    public class Archives : IComparable<Archives>
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
        public int CompareTo(Archives compareArchive)
        {
            // A null value means that this object is greater.
            if (compareArchive == null)
                return 1;

            else
                return this.Name.CompareTo(compareArchive.Name);
        }
    }
}
