using System;

namespace Figma2Ugui.Utils
{
    public static class FigmaUrlParser
    {
        public static string ExtractFileKey(string url)
        {
            // URL 格式: https://www.figma.com/design/{fileKey}/{fileName}
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == "design" && i + 1 < segments.Length)
                {
                    return segments[i + 1];
                }
                if (segments[i] == "file" && i + 1 < segments.Length)
                {
                    return segments[i + 1];
                }
            }

            throw new Exception("Invalid Figma URL format");
        }
    }
}
