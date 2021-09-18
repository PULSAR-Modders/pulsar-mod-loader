using System;
using System.Net;
using System.Text;

namespace PulsarModLoader.Utilities.Uploaders
{
    class TransferShUploader : IUploader
    {
        private readonly Uri uploadUri = new Uri("https://transfer.sh/");

        public string UploadFile(string filePath)
        {
            using (WebClient wc = new WebClient())
            {
                return Encoding.UTF8.GetString(wc.UploadFile(uploadUri, "PUT", filePath)).TrimEnd('\0');
            }
        }
    }
}
