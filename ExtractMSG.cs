using MsgReader.Outlook;
using System.Text;

namespace InvoiceAPP.Services;

public class MsgAttachmentExtractor
{
    public MsgAttachmentExtractor()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public void ExtractPdfs(string folderMsgPath, string outputDirectory, string archiveDirectory)
    {
        string[] msgFiles = Directory.GetFiles(
            folderMsgPath,
            "*.msg",
            SearchOption.TopDirectoryOnly
        );

        foreach (string msgPath in msgFiles)
        {
            using (var msg = new Storage.Message(msgPath))
            {
                foreach (Storage.Attachment attachment in msg.Attachments)
                {
                    if (!attachment.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string outputPath = Path.Combine( outputDirectory, attachment.FileName);

                    File.WriteAllBytes( outputPath,attachment.Data);

                }
            }

            string archivePath = Path.Combine(archiveDirectory, Path.GetFileName(msgPath));

            File.Move(msgPath, archivePath);

        }
    }
}