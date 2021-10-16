using System;
using System.ComponentModel;

namespace Sleep_Downloader
{
    public class Fields : IComparable<Fields>, INotifyPropertyChanged
    {
        private string name { get; set; }
        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.NotifyPropertyChanged("Name");
                }
            }
        }
        private string name2 { get; set; }
        public string Name2
        {
            get { return this.name2; }
            set
            {
                if (this.name2 != value)
                {
                    this.name2 = value;
                    this.NotifyPropertyChanged("Name2");
                }
            }
        }
        public string Value { get; set; }
        public int CompareTo(Fields compareFields)
        {
            // A null value means that this object is greater.
            if (compareFields == null)
                return 1;

            else
                return this.Name.CompareTo(compareFields.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
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
