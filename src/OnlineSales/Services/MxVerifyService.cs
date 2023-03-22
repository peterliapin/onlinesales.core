// <copyright file="MxVerifyService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Sockets;
using OnlineSales.Interfaces;

namespace OnlineSales.Services;

public class MxVerifyService : IMxVerifyService
{    
    private const int TimeOutSeconds = 3;

    private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

    public async Task<bool> Verify(string mxValue)
    {
        await semaphoreSlim.WaitAsync();

        try
        {
            var timeout = (int)TimeSpan.FromSeconds(TimeOutSeconds).TotalMilliseconds;

            // Define a list of ports to try
            var ports = new[] { 25, 587, 465 };

            var hostName = mxValue.TrimEnd('.');

            foreach (var port in ports)
            {
                try
                {
                    // Try to connect to the SMTP server on the current port
                    using (var client = new TcpClient())
                    {
                        var result = client.BeginConnect(hostName, port, null, null);

                        var success = result.AsyncWaitHandle.WaitOne(timeout);

                        if (!success)
                        {
                            return false;
                        }

                        // Get the stream for reading and writing data
                        using (var stream = client.GetStream())
                        {
                            // Create a StreamReader and StreamWriter for reading and writing data
                            using (StreamReader reader = new StreamReader(stream))
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                // Read the welcome message from the server
                                string welcomeMessage = (await reader.ReadLineAsyncWithTimeOut(timeout)) !;
                                Log.Information($"SMTP server {mxValue}, port {port} replied with the following welcome message: {welcomeMessage}");

                                Console.WriteLine(welcomeMessage);

                                // Send the EHLO command to the server
                                writer.WriteLine($"EHLO waveaccess.global");
                                writer.Flush();

                                // Read the response from the server
                                string ehloResponse = (await reader.ReadLineAsyncWithTimeOut(timeout)) !;
                                Log.Information($"SMTP server {mxValue}, port {port} replied to ehlo request: {ehloResponse}");
                            }
                        }

                        client.EndConnect(result);

                        return true;
                    }
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
            semaphoreSlim.Release();
        }
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