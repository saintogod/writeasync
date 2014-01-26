﻿//-----------------------------------------------------------------------
// <copyright file="InputQueue.cs" company="Brian Rogers">
// Copyright (c) Brian Rogers. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace QueueSample
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class InputQueue<T> : IDisposable
    {
        private readonly Queue<T> items;

        private TaskCompletionSource<T> pending;

        public InputQueue()
        {
            this.items = new Queue<T>();
        }

        public Task<T> DequeueAsync()
        {
            Task<T> task;
            lock (this.items)
            {
                if (this.pending != null)
                {
                    throw new InvalidOperationException("A dequeue operation is already in progress.");
                }

                TaskCompletionSource<T> current = new TaskCompletionSource<T>();
                task = current.Task;

                if (this.items.Count > 0)
                {
                    T item = this.items.Dequeue();
                    current.SetResult(item);
                }
                else
                {
                    this.pending = current;
                }
            }

            return task;
        }

        public void Enqueue(T item)
        {
            TaskCompletionSource<T> current = null;
            lock (this.items)
            {
                if (this.pending == null)
                {
                    this.items.Enqueue(item);
                }
                else
                {
                    current = this.pending;
                    this.pending = null;
                }
            }

            if (current != null)
            {
                current.SetResult(item);
            }
        }

        public void Dispose()
        {
            TaskCompletionSource<T> current = null;
            lock (this.items)
            {
                current = this.pending;
                this.pending = null;
            }

            if (current != null)
            {
                current.SetException(new ObjectDisposedException("InputQueue"));
            }
        }
    }
}
