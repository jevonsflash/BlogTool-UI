using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BlogTool.Common;

public class PhantomJSOverride : IDisposable
{
    private Process PhantomJsProcess;

    private List<string> errorLines = new List<string>();

    public string ToolPath { get; set; }

    public string TempFilesPath { get; set; }

    public string PhantomJsExeName { get; set; }

    public string CustomArgs { get; set; }

    public ProcessPriorityClass ProcessPriority { get; set; }

    public TimeSpan? ExecutionTimeout { get; set; }

    public event EventHandler<DataReceivedEventArgs> OutputReceived;

    public event EventHandler<DataReceivedEventArgs> ErrorReceived;

    public PhantomJSOverride()
    {
        ToolPath = ResolveAppBinPath();
        PhantomJsExeName = "phantomjs.exe";
        ProcessPriority = ProcessPriorityClass.Normal;
    }

    private string ResolveAppBinPath()
    {
        return null;
    }

    public void Run(string jsFile, string[] jsArgs)
    {
        Run(jsFile, jsArgs, null, null);
    }

    public void Run(string jsFile, string[] jsArgs, Stream inputStream, Stream outputStream)
    {
        if (jsFile == null)
        {
            throw new ArgumentNullException("jsFile");
        }

        RunInternal(jsFile, jsArgs, inputStream, outputStream);
        try
        {
            WaitProcessForExit();
            CheckExitCode(PhantomJsProcess.ExitCode, errorLines);
        }
        finally
        {
            PhantomJsProcess.Close();
            PhantomJsProcess = null;
        }
    }

    public Task<bool> RunAsync(string jsFile, string[] jsArgs)
    {
        if (jsFile == null)
        {
            throw new ArgumentNullException("jsFile");
        }

        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        Action handleExit = delegate
        {
            try
            {
                CheckExitCode(PhantomJsProcess.ExitCode, errorLines);
                tcs.SetResult(result: true);
            }
            catch (Exception exception)
            {
                tcs.TrySetException(exception);
            }
            finally
            {
                PhantomJsProcess.Close();
                PhantomJsProcess = null;
            }
        };
        RunInternal(jsFile, jsArgs, null, null);
        PhantomJsProcess.Exited += delegate
        {
            handleExit();
        };
        if (PhantomJsProcess.HasExited)
        {
            handleExit();
        }

        return tcs.Task;
    }

    public void RunScript(string javascriptCode, string[] jsArgs)
    {
        RunScript(javascriptCode, jsArgs, null, null);
    }

    public void RunScript(string javascriptCode, string[] jsArgs, Stream inputStream, Stream outputStream)
    {
        string text = Path.Combine(GetTempPath(), "phantomjs-" + Path.GetRandomFileName() + ".js");
        try
        {
            File.WriteAllBytes(text, Encoding.UTF8.GetBytes(javascriptCode));
            Run(text, jsArgs, inputStream, outputStream);
        }
        finally
        {
            DeleteFileIfExists(text);
        }
    }

    public Task RunScriptAsync(string javascriptCode, string[] jsArgs)
    {
        string tempPath = GetTempPath();
        string tmpJsFilePath = Path.Combine(tempPath, "phantomjs-" + Path.GetRandomFileName() + ".js");
        File.WriteAllBytes(tmpJsFilePath, Encoding.UTF8.GetBytes(javascriptCode));
        try
        {
            Task<bool> task = RunAsync(tmpJsFilePath, jsArgs);
            task.ContinueWith(delegate
            {
                DeleteFileIfExists(tmpJsFilePath);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }
        catch
        {
            DeleteFileIfExists(tmpJsFilePath);
            throw;
        }
    }

    private string GetTempPath()
    {
        if (!string.IsNullOrEmpty(TempFilesPath) && !Directory.Exists(TempFilesPath))
        {
            Directory.CreateDirectory(TempFilesPath);
        }

        return TempFilesPath ?? Path.GetTempPath();
    }

    private void DeleteFileIfExists(string filePath)
    {
        if (filePath != null && File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
            }
        }
    }

    private string PrepareCmdArg(string arg)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append('"');
        stringBuilder.Append(arg.Replace("\"", "\\\""));
        stringBuilder.Append('"');
        return stringBuilder.ToString();
    }

    private void RunInternal(string jsFile, string[] jsArgs, Stream inputStream, Stream outputStream)
    {
        errorLines.Clear();
        try
        {
            if (string.IsNullOrEmpty(ToolPath))
            {
                throw new ArgumentException("ToolPath property is not initialized with folder that contains phantomjs executable file");
            }

            string text = Path.Combine(ToolPath, PhantomJsExeName);
            if (!File.Exists(text))
            {
                throw new FileNotFoundException("Cannot find PhantomJS executable: " + text);
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(CustomArgs))
            {
                stringBuilder.AppendFormat(" {0} ", CustomArgs);
            }

            stringBuilder.AppendFormat(" {0}", PrepareCmdArg(jsFile));
            if (jsArgs != null)
            {
                foreach (string arg in jsArgs)
                {
                    stringBuilder.AppendFormat(" {0}", PrepareCmdArg(arg));
                }
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(text, stringBuilder.ToString());
            processStartInfo.StandardErrorEncoding = Encoding.UTF8;
            processStartInfo.StandardInputEncoding = Encoding.UTF8;
            processStartInfo.StandardOutputEncoding = Encoding.UTF8;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(ToolPath);
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            PhantomJsProcess = new Process();
            PhantomJsProcess.StartInfo = processStartInfo;
            PhantomJsProcess.EnableRaisingEvents = true;
            PhantomJsProcess.Start();
            if (ProcessPriority != ProcessPriorityClass.Normal)
            {
                PhantomJsProcess.PriorityClass = ProcessPriority;
            }

            PhantomJsProcess.ErrorDataReceived += delegate (object o, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    errorLines.Add(args.Data);
                    if (ErrorReceived != null)
                    {
                        ErrorReceived(this, args);
                    }
                }
            };
            PhantomJsProcess.BeginErrorReadLine();
            if (outputStream == null)
            {
                PhantomJsProcess.OutputDataReceived += delegate (object o, DataReceivedEventArgs args)
                {
                    if (OutputReceived != null)
                    {
                        OutputReceived(this, args);
                    }
                };
                PhantomJsProcess.BeginOutputReadLine();
            }

            if (inputStream != null)
            {
                CopyToStdIn(inputStream);
            }

            if (outputStream != null)
            {
                ReadStdOutToStream(PhantomJsProcess, outputStream);
            }
        }
        catch (Exception ex)
        {
            EnsureProcessStopped();
            throw new Exception("Cannot execute PhantomJS: " + ex.Message, ex);
        }
    }

    protected void CopyToStdIn(Stream inputStream)
    {
        byte[] array = new byte[8192];
        while (true)
        {
            int num = inputStream.Read(array, 0, array.Length);
            if (num <= 0)
            {
                break;
            }

            PhantomJsProcess.StandardInput.BaseStream.Write(array, 0, num);
            PhantomJsProcess.StandardInput.BaseStream.Flush();
        }

        PhantomJsProcess.StandardInput.Close();
    }

    public void WriteLine(string s)
    {
        if (PhantomJsProcess == null)
        {
            throw new InvalidOperationException("PhantomJS is not running");
        }

        PhantomJsProcess.StandardInput.WriteLine(s);
        PhantomJsProcess.StandardInput.Flush();
    }

    public void WriteEnd()
    {
        PhantomJsProcess.StandardInput.Close();
    }

    public void Abort()
    {
        EnsureProcessStopped();
    }

    private void WaitProcessForExit()
    {
        bool hasValue = ExecutionTimeout.HasValue;
        if (hasValue)
        {
            PhantomJsProcess.WaitForExit((int)ExecutionTimeout.Value.TotalMilliseconds);
        }
        else
        {
            PhantomJsProcess.WaitForExit();
        }

        if (PhantomJsProcess == null)
        {
            throw new PhantomJSException(-1, "FFMpeg process was aborted");
        }

        if (hasValue && !PhantomJsProcess.HasExited)
        {
            EnsureProcessStopped();
            throw new PhantomJSException(-2, $"FFMpeg process exceeded execution timeout ({ExecutionTimeout}) and was aborted");
        }
    }

    private void EnsureProcessStopped()
    {
        bool flag = true;
        try
        {
            flag = PhantomJsProcess.HasExited;
        }
        catch
        {
        }

        if (PhantomJsProcess != null && !flag)
        {
            try
            {
                PhantomJsProcess.Kill();
                PhantomJsProcess.Dispose();
                PhantomJsProcess = null;
            }
            catch (Exception)
            {
            }
        }
    }

    private void ReadStdOutToStream(Process proc, Stream outputStream)
    {
        byte[] array = new byte[32768];
        int count;
        while ((count = proc.StandardOutput.BaseStream.Read(array, 0, array.Length)) > 0)
        {
            outputStream.Write(array, 0, count);
        }
    }

    private void CheckExitCode(int exitCode, List<string> errLines)
    {
        if (exitCode != 0)
        {
            throw new PhantomJSException(exitCode, string.Join("\n", errLines.ToArray()));
        }
    }

    public void Dispose()
    {
        EnsureProcessStopped();
    }
}