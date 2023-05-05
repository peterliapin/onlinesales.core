// <copyright file="MxVerifyService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Sockets;
using OnlineSales.Interfaces;

namespace OnlineSales.Services;

public class MxVerifyService : IMxVerifyService
{
    /// <summary>
    /// By RFC 5321 we have next <see href="https://www.rfc-editor.org/rfc/rfc5321#section-4.5.3.2">timeout values</see>:<br/><br/>
    ///  -&gt; Initial 220 Message: <see langword="5 Minutes"/> (300 sec)<br/>
    ///  -&gt; MAIL Command: <see langword="5 Minutes"/> (300 sec)<br/>
    ///  -&gt; RCPT Command: <see langword="5 Minutes"/> (300 sec)<br/>
    /// <br/>
    /// Set timeout as 60 sec to get answer from most valid services.<br/>
    /// <br/>[ LINK ] 
    /// <see href="https://www.rfc-editor.org/rfc/rfc5321">RFC 5321 - SMTP - October 2008</see>.<br/>
    /// </summary>
    private const int TimeOutSeconds = 60;
    private const string SenderSourceHostName = "waveaccess.global";
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

    public async Task<bool> Verify(string mxValue)
    {
        await SemaphoreSlim.WaitAsync();
        var ports = new[] { 25, 465, 587 };

        try
        {
            var hostName = mxValue.TrimEnd('.');
            foreach (var port in ports)
            {
                try
                {
                    using var tcp = new TcpClient();
                    var connecting = tcp.BeginConnect(hostName, port, null, null);
                    var connected = connecting.AsyncWaitHandle.WaitOne(3000);

                    if (!connected || !tcp.Connected)
                    {
                        return false;
                    }

                    using var stream = tcp.GetStream();
                    using var reader = new StreamReader(stream);
                    using var writer = new StreamWriter(stream);

                    (int code, string value) returned;

                    // Read SMTP server join message
                    returned = await ReadLine(reader);
                    Log.Information($"SMTP server {mxValue}, port {port} replied with the following welcome message: {returned.value}");

                    Console.WriteLine(returned.value);

                    // Send HELO code and get answer
                    returned = await WriteLineAndGetAnswer(writer, $"{(port == 587 ? "EHLO" : "HELO")} {SenderSourceHostName}", reader);
                    Log.Information($"SMTP server {mxValue}, port {port} replied to {(port == 587 ? "EHLO" : "HELO")} request: {returned.value}");

                    SendLine(writer, "QUIT");

                    tcp.EndConnect(connecting);

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to connect to the SMTP server {mxValue} on the current port {port}, try the next one.");
                }
            }

            return false;
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public async Task<MxCheckResult[]> BulkVerify(string mxValue, params string[] emails)
    {
        await SemaphoreSlim.WaitAsync();

        var port = 25;
        var returnResults = new List<MxCheckResult>();

        MxCheckResult[] ReturnAllFailedWithCode(int code, string value)
        {
            return emails
                .Select(e => new MxCheckResult(mxValue, e, code, value, false))
                .ToArray();
        }

        try
        {
            using var tcp = new TcpClient();
            var connecting = tcp.BeginConnect(mxValue, port, null, null);
            var connected = connecting.AsyncWaitHandle.WaitOne(3000);

            if (!connected || !tcp.Connected)
            {
                return ReturnAllFailedWithCode((int)MxResultCode.ConnectionFailed, string.Empty);
            }

            using var stream = tcp.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream);

            (int c, string v) rc;

            // Read SMTP server join message
            rc = await ReadLine(reader);
            if (rc.c != 220)
            {
                return ReturnAllFailedWithCode(rc.c, rc.v);
            }

            // Send HELO code and get answer
            rc = await WriteLineAndGetAnswer(writer, $"HELO {SenderSourceHostName}", reader);
            if (rc.c != 250 && rc.c != 220)
            {
                return ReturnAllFailedWithCode((int)MxResultCode.HeloFailed, rc.v);
            }

            // Send MAIL FROM to start checking and wait answer
            rc = await WriteLineAndGetAnswer(writer, $"MAIL FROM: <check@{SenderSourceHostName}>", reader);
            if (rc.c != 250)
            {
                return ReturnAllFailedWithCode((int)MxResultCode.SenderRejected, rc.v);
            }

            // Wait some time to avoid ban
            Thread.Sleep(100);

            foreach (var email in emails)
            {
                // Send RCPT TO to check receiver email and wait answer
                rc = await WriteLineAndGetAnswer(writer, $"RCPT TO: <{email}>", reader);
                switch (rc.c)
                {
                    // 250 - ok
                    case 250:
                        returnResults.Add(new MxCheckResult(mxValue, email, MxResultCode.Ok, string.Empty, true));
                        break;

                    // 450 - user got to many mails, but exist
                    case 450:
                        returnResults.Add(new MxCheckResult(mxValue, email, MxResultCode.OkButTooManyEmailsGot, string.Empty, true));
                        break;

                    // 452 - user mailbox full, but exist
                    case 452:
                        returnResults.Add(new MxCheckResult(mxValue, email, MxResultCode.OkButMailboxFull, string.Empty, true));
                        break;

                    // Other response codes should be treated as a failure code.
                    default:
                        returnResults.Add(new MxCheckResult(mxValue, email, rc.c, rc.v, false));
                        break;
                }
            }

            SendLine(writer, "QUIT");
            return returnResults.ToArray();
        }
        catch (Exception ex)
        {
            // catch all unprocessed emails and generate result
            var notProcessed = emails
                .Where(e => !returnResults.Any(email => email.Email == e))
                .Select(e => new MxCheckResult(mxValue, e, MxResultCode.GeneralException, ex.Message, false))
                .ToArray();

            // Return concated processed and unprocessed results
            return returnResults
                .Concat(notProcessed)
                .ToArray();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    private static async Task<(int, string)> WriteLineAndGetAnswer(StreamWriter w, string text, StreamReader r)
    {
        if (w.BaseStream.CanWrite)
        {
            SendLine(w, text);
        }
        else
        {
            return ((int)MxResultCode.GeneralException, "Can't write line to stream!");
        }

        return await ReadLine(r);
    }

    private static async Task<(int, string)> ReadLine(StreamReader r)
    {
        try
        {
            var so = ((NetworkStream)r.BaseStream).Socket;
            if (so.Available == 0)
            {
                await Task.Delay(100);
            }

            var ln = await r.ReadLineAsyncWithTimeOut(TimeOutSeconds * 1000);

            while (so.Available > 0)
            {
                ln = await r.ReadLineAsyncWithTimeOut(1000);
            }

            if (!string.IsNullOrEmpty(ln))
            {
                return int.TryParse(ln.AsSpan(0, 3), out var answer)
                    ? (answer, ln)
                    : (-3, "Can't parse: " + ln);
            }

            return (-2, "NULL Response");
        }
        catch (Exception e)
        {
            return (-1, e.Message);
        }
    }

    private static void SendLine(StreamWriter w, string text)
    {
        Thread.Sleep(25); // sleep to avoid flag this IP as smap source!
        try
        {
            w.WriteLine(text);
            w.Flush();
        }
        catch
        {
            // Send failed 
        }
    }
}

public enum MxResultCode
{
    GeneralException = -9,
    SenderRejected = -3,
    HeloFailed = -2,
    ConnectionFailed = -1,

    Ok = 250,
    OkButTooManyEmailsGot = 450,
    RetryLater = 451,
    OkButMailboxFull = 452,
    NotFound = 550,
}

public class MxCheckResult
{
    public MxCheckResult(string? mxhost, string? email, int code, string? value, bool success)
    {
        this.TimeStamp = DateTime.Now;
        this.MxHost = mxhost;
        this.Email = email;
        this.StatusCode = (MxResultCode)code;
        this.StatusValue = value;
        this.Successfull = success;
    }

    public MxCheckResult(string? mxhost, string? email, MxResultCode code, string? value, bool success)
    {
        this.TimeStamp = DateTime.Now;
        this.MxHost = mxhost;
        this.Email = email;
        this.StatusCode = code;
        this.StatusValue = value;
        this.Successfull = success;
    }

    public DateTime TimeStamp { get; set; }

    public string? MxHost { get; set; }

    public string? Email { get; set; }

    public MxResultCode StatusCode { get; set; }

    public string? StatusValue { get; set; }

    public bool Successfull { get; set; }

    public override string ToString()
    {
        return $"[{(this.Successfull ? "ok" : "FAIL")}] {this.MxHost} - {this.StatusCode} :: {this.StatusValue}";
    }
}

public static class StreamReaderExtension
{
    public static async Task<string?> ReadLineAsyncWithTimeOut(this StreamReader stream, int timeoutMilliseconds)
    {
        var readLineTask = stream.ReadLineAsync();
        var timeoutTask = Task.Delay(timeoutMilliseconds);

        var completedTask = await Task.WhenAny(readLineTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException("ReadLineAsync timed out.");
        }

        return await readLineTask;
    }
}