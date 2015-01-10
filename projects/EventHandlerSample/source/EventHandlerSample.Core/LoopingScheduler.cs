﻿//-----------------------------------------------------------------------
// <copyright file="LoopingScheduler.cs" company="Brian Rogers">
// Copyright (c) Brian Rogers. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace EventHandlerSample
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class LoopingScheduler
    {
        private readonly Func<Task> doAsync;
        private readonly Stopwatch stopwatch;

        public LoopingScheduler(Func<Task> doAsync)
        {
            this.doAsync = doAsync;
            this.stopwatch = Stopwatch.StartNew();
            this.GetElapsed = this.DefaultGetElapsed;
        }

        public event EventHandler Paused;

        public Func<TimeSpan> GetElapsed { get; set; }

        public async Task RunAsync(TimeSpan pauseInterval)
        {
            while (true)
            {
                await this.doAsync();
                if (this.GetElapsed() >= pauseInterval)
                {
                    EventHandler handler = this.Paused;
                    handler(this, EventArgs.Empty);
                }
            }
        }

        private TimeSpan DefaultGetElapsed()
        {
            return this.stopwatch.Elapsed;
        }
    }
}
