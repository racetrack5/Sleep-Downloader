using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Sleep_Downloader
{
    public class Fields_Whitelist
    {
        public string Name { get; set; }
    }
    public class Fields
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Reports
    {
        public string Name { get; set; }
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
