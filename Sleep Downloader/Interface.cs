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
    class Interface
    {
        public string Logger(string Message)
        {
            string Output = String.Format("{0} - {1}\n", DateTime.Now, Message);
            return Output;
        }
    }
}
