using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HyWebSpider.Lib.CefBrowser
{
    public class StringVisitor : IStringVisitor
    {
        public string SourceFileName { get; set; }
        public void Dispose()
        {

        }

        public void Visit(string str)
        {
            bool isRead = false;
            int count = 0;
            while(!isRead)
            try
            {
                File.WriteAllText(SourceFileName, str);
                isRead = true;
            }
            catch (IOException ioe) {
                    if (count++ > 100) throw ioe;
                    Thread.Sleep(10);
            }
        }
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
