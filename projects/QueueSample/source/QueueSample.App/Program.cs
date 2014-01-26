﻿//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Brian Rogers">
// Copyright (c) Brian Rogers. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace QueueSample
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            InputQueue<int> queue = new InputQueue<int>();
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Task enqueueTask = PrintException(EnqueueLoopAsync(queue, cts.Token));
                Task dequeueTask = PrintException(DequeueLoopAsync(queue, cts.Token));

                Console.WriteLine("Press ENTER to cancel.");
                Console.ReadLine();
                cts.Cancel();

                Task.WaitAll(enqueueTask, dequeueTask);
            }
        }

        private static async Task EnqueueLoopAsync(InputQueue<int> queue, CancellationToken token)
        {
            await Task.Yield();

            int i = 0;
            while (!token.IsCancellationRequested)
            {
                ++i;
                queue.Enqueue(i);
                await Task.Delay(1);
            }
        }

        private static async Task DequeueLoopAsync(InputQueue<int> queue, CancellationToken token)
        {
            await Task.Yield();

            int previous = 0;
            while (!token.IsCancellationRequested)
            {
                int current = await queue.DequeueAsync();
                if (current - previous != 1)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "Invalid data! Current is {0} but previous was {1}.", current, previous);
                    throw new InvalidOperationException(message);
                }

                previous = current;
            }
        }

        private static async Task PrintException(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
