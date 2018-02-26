using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CoreDrawablesGenerator
{
    public static class Clipboard
    {
        public static void SetText(string text)
        {
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, text);

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    $"type {tempFile} | clip".Bat();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    $"cat \"{tempFile}\" | pbcopy".Bash();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (Shell.LinuxCommandExists("xsel"))
                    {
                        $"cat \"{tempFile}\" | xsel --input --clipboard".Bash();
                    }
                    else if (Shell.LinuxCommandExists("xclip"))
                    {
                        // xclip hangs on ubuntu with stdout redirect for some reason
                        ProcessOptions opts = new ProcessOptions
                        {
                            ShouldRedirectStdOut = false
                        };
                        $"cat \"{tempFile}\" | xclip -selection clipboard".Bash(opts);
                    }
                    else
                    {
                        throw new NotSupportedException("Copying to clipboard is not supported without any of the following packages:" + Environment.NewLine +
                            "- xsel" + Environment.NewLine +
                            "- xclip");
                    }
                }
                else
                {
                    throw new NotSupportedException("Copying to clipboard failed: Unknown Operating System.");
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
            
        }
    }
}
