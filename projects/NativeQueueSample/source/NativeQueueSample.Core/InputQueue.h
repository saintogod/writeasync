//-----------------------------------------------------------------------
// <copyright file="InputQueue.h" company="Brian Rogers">
// Copyright (c) Brian Rogers. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma once

#include <string>
#include <ppltasks.h>
#include <queue>

namespace NativeQueueSample
{
    template <typename T>
    class InputQueue
    {
    public:
        InputQueue()
            : items_(),
            pending_()
        {
        }
        
        ~InputQueue()
        {
        }

        void Enqueue(T item)
        {
            if (!pending_)
            {
                items_.push(item);
            }
            else
            {
                pending_->set(item);
                pending_.reset();
            }
        }

        concurrency::task<T> DequeueAsync()
        {
            pending_ = make_unique<concurrency::task_completion_event<T>>();
            concurrency::task<T> task = concurrency::task<T>(*pending_);
            if (!items_.empty())
            {
                T item = items_.front();
                items_.pop();
                pending_->set(item);
                pending_.reset();
            }

            return task;
        }

    private:
        std::queue<T> items_;
        std::unique_ptr<concurrency::task_completion_event<T>> pending_;

        InputQueue(InputQueue const & other);
        InputQueue & operator=(InputQueue const & other);
    };
}