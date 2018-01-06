﻿// <copyright file="FileSystemContract.cs" company="Brian Rogers">
// Copyright (c) Brian Rogers. All rights reserved.
// </copyright>

namespace FileSystemSample.Test
{
    using System.IO;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public abstract partial class FileSystemContract
    {
        protected FileSystemContract()
        {
        }

        protected virtual bool MustBlock { get => false; }

        protected abstract FullPath Root { get; }

        protected abstract IFileSystem Create();

        private static IFile CreateFile(IDirectory dir, string fileName) => dir.CreateFile(new PathPart(fileName));

        private IDirectory CreateDir(IFileSystem fs, string name) => fs.Create(this.Root.Combine(new PathPart(name)));

        private IFile CreateFile(IFileSystem fs, string dirName, string fileName)
        {
            IDirectory dir = this.CreateDir(fs, dirName);
            return CreateFile(dir, fileName);
        }

        private string Read(IFile file)
        {
            using (StreamReader reader = new StreamReader(file.OpenRead()))
            {
                return this.ResultOf(reader.ReadToEndAsync());
            }
        }

        private TResult ResultOf<TResult>(Task<TResult> task)
        {
            if (this.MustBlock)
            {
                task.Wait();
            }

            task.IsCompleted.Should().BeTrue(because: "task should complete sync");
            return task.Result;
        }
    }
}
