using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace lpubsppop01.ReplaceText
{
    static class EncodingDetector
    {
        static EncodingDetector()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static (Encoding encoding, string newLine) Detect(string path)
        {
            var file = new FileInfo(path);
            using (var reader = new Hnx8.ReadJEnc.FileReader(file))
            {
                var charCode = reader.Read(file);
                if (reader.Text == null) return (null, null);
                var encoding = charCode.GetEncoding();
                var newLine = (reader.Text.DetectNewLineKind() ?? NewLineKind.CRLF).ToNewLineString();
                return (encoding, newLine);
            }
        }
    }

    enum NewLineKind
    {
        CRLF, LF
    }

    static class NewLineUtility
    {
        public static string ToNewLineString(this NewLineKind kind)
        {
            switch (kind)
            {
                default:
                case NewLineKind.CRLF: return "\r\n";
                case NewLineKind.LF: return "\n";
            }
        }

        public static NewLineKind ToNewLineKind(this string newLineStr)
        {
            if (newLineStr == "\r\n")
            {
                return NewLineKind.CRLF;
            }
            else if (newLineStr == "\n")
            {
                return NewLineKind.LF;
            }
            throw new ArgumentException("The passed newLineStr value is not new line characters.");
        }

        public static NewLineKind? DetectNewLineKind(this string str)
        {
            int iLF = str.IndexOf('\n');
            if (iLF == -1) return NewLineKind.CRLF;
            if (iLF == 0) return NewLineKind.LF;
            return (str[iLF - 1] == '\r') ? NewLineKind.CRLF : NewLineKind.LF;
        }
    }
}
