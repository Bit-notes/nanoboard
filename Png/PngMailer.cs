using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace nboard
{
    class PngMailer
    {
        bool _isReading = false;

        public void ReadInbox(NanoDB to)
        {
            if (_isReading) return;
            _isReading = true;

            if (Directory.Exists(Strings.Download))
            {
                string[] files = Directory.GetFiles(Strings.Download);

                foreach (string f in files)
                {
                    string pathToPng = f;
                    byte[] packed = null;

                    try
                    {
                        packed = new PngStegoUtil().ReadHiddenBytesFromPng(pathToPng);
                        GC.Collect();
                    }
                    catch
                    {
                        // invalid container
                    }

                    NanoPost[] posts = null;

                    try
                    {   
                        posts = NanoPostPackUtil.Unpack(packed);
                    }
                    catch
                    {
                        // inavlid container
                    }

                    bool any = false;

                    if (posts != null)
                    {
                        foreach (var p in posts)
                        {
                            any |= to.AddPost(p);
                        }
                    }

                    if (any)
                    {
                        NotificationHandler.Instance.AddNotification("Извлечены новые сообщения.");
                        to.WriteNewPosts(false);
                    }
                }

                try
                {
                    foreach (string f in files)
                    {
                        File.Delete(f);
                    }
                }
                catch
                {
                }
            }

            _isReading = false;
        }

        public void FillOutbox(NanoDB from)
        {
            if (!Directory.Exists(Strings.Upload))
            {
                Directory.CreateDirectory(Strings.Upload);
            }

            new PngContainerCreatorNew().SaveToPngContainer(from);
            from.WriteNewPosts(false);
        }
    }
}