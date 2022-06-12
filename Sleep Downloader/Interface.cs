using System;

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
