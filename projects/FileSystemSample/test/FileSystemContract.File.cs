﻿// <copyright file="FileSystemContract.File.cs" company="Brian Rogers">
// Copyright (c) Brian Rogers. All rights reserved.
// </copyright>

namespace FileSystemSample.Test
{
    using System;
    using System.IO;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public abstract partial class FileSystemContract
    {
        [TestMethod]
        public void ShouldCreateFileIfDoesNotExist()
        {
            IFileSystem fs = this.Create();

            IFile file = this.CreateFile(fs, "Parent", "new.txt");

            file.Path.ToString().Should().Match(@"*\Parent\new.txt");
        }

        [TestMethod]
        public void ShouldGetFileIfAlreadyExists()
        {
            IFileSystem fs = this.Create();
            IFile file1 = this.CreateFile(fs, "Parent", "NotNew.txt");

            IFile file2 = this.CreateFile(fs, "Parent", "NotNew.txt");

            file2.Path.Should().Be(file1.Path);
            file2.Path.ToString().Should().Match(@"*Parent\NotNew.txt");
        }

        [TestMethod]
        public void ShouldGetFileIfAlreadyExistsIgnoreCase()
        {
            IFileSystem fs = this.Create();
            IFile file1 = this.CreateFile(fs, "Parent", "NotNew.txt");

            IFile file2 = this.CreateFile(fs, "Parent", "nOTnEW.TXT");

            file2.Path.Should().Be(file1.Path);
            file2.Path.ToString().Should().MatchEquivalentOf(@"*Parent\NotNew.txt");
        }

        [TestMethod]
        public void ShouldFailCreateOnInvalidNames()
        {
            IFileSystem fs = this.Create();

            this.FailCreateFile(fs, null, "*<null>*");
            this.FailCreateFile(fs, string.Empty, "*<empty>*");
            this.FailCreateFile(fs, "fwd/slash", "*'fwd/slash'*");
            this.FailCreateFile(fs, "|pipe", "*'|pipe'*");
            this.FailCreateFile(fs, "asterisk*", "*'asterisk**'*");
            this.FailCreateFile(fs, "<less-than", "*'<less-than'*");
            this.FailCreateFile(fs, "greater>than", "*'greater>than'*");
            this.FailCreateFile(fs, "back\\slash", "*'back\\slash'*");
        }

        [TestMethod]
        public void ShouldReadEmptyFile()
        {
            IFileSystem fs = this.Create();

            IFile file = this.CreateFile(fs, "Parent", "Empty.txt");

            this.ReadToEnd(file).Should().Be(string.Empty);
        }

        [TestMethod]
        public void ShouldReadJustWrittenFile()
        {
            IFileSystem fs = this.Create();
            IFile file = this.CreateFile(fs, "Parent", "Text.txt");

            this.WriteAll(file, "some text");

            this.ReadToEnd(file).Should().Be("some text");
        }

        [TestMethod]
        public void ShouldClearFileOnRecreate()
        {
            IFileSystem fs = this.Create();
            IFile file1 = this.CreateFile(fs, "Parent", "Old.txt");
            this.WriteAll(file1, "old-old-old-old-old-old");

            IFile file2 = this.CreateFile(fs, "Parent", "Old.txt");

            string empty = this.ReadToEnd(file1);
            this.ReadToEnd(file2).Should().Be(empty);
        }

        [TestMethod]
        public void ShouldOverwriteOnEachWrite()
        {
            IFileSystem fs = this.Create();
            IFile file = this.CreateFile(fs, "Parent", "Old.txt");
            this.WriteAll(file, "old-old-old-old-old-old");

            this.WriteAll(file, "NEW!");

            this.ReadToEnd(file).Should().Be("NEW!");
        }

        [TestMethod]
        public void ShouldAllowMultipleReaders()
        {
            this.AllowMultipleReaders("Read.txt");
        }

        [TestMethod]
        public void ShouldAllowMultipleReadersIgnoreCase()
        {
            this.AllowMultipleReaders("Read.txt", "rEAD.TXT");
        }

        [TestMethod]
        public void ShouldFailMultipleWriters()
        {
            this.FailMultipleWriters("Write.txt");
        }

        [TestMethod]
        public void ShouldFailMultipleWritersIgnoreCase()
        {
            this.FailMultipleWriters("Write.txt", true);
        }

        [TestMethod]
        public void ShouldFailReadWhenAlreadyWriting()
        {
            IFileSystem fs = this.Create();
            IFile file1 = this.CreateFile(fs, "Parent", "WriteRead.txt");
            IFile file2 = this.CreateFile(fs, "Parent", "WriteRead.txt");

            using (Stream write1 = file1.OpenWrite())
            {
                Action act = () => file2.OpenRead();

                act.ShouldThrow<FileSystemException>()
                    .WithMessage(@"* '*\WriteRead.txt' is already opened.")
                    .WithInnerException<IOException>();
            }
        }

        [TestMethod]
        public void ShouldFailWriteWhenAlreadyReading()
        {
            IFileSystem fs = this.Create();
            IFile file1 = this.CreateFile(fs, "Parent", "ReadWrite.txt");
            IFile file2 = this.CreateFile(fs, "Parent", "ReadWrite.txt");

            using (Stream read1 = file1.OpenRead())
            {
                Action act = () => file2.OpenWrite();

                act.ShouldThrow<FileSystemException>()
                    .WithMessage(@"* '*\ReadWrite.txt' is already opened.")
                    .WithInnerException<IOException>();
            }
        }

        [TestMethod]
        public void ShouldFailWriteWhenAlreadyReadingTwice()
        {
            IFileSystem fs = this.Create();
            IFile file1 = this.CreateFile(fs, "Parent", "ReadReadWrite.txt");
            IFile file2 = this.CreateFile(fs, "Parent", "ReadReadWrite.txt");

            using (Stream read1 = file1.OpenRead())
            using (Stream read2 = file1.OpenRead())
            {
                Action act = () => file2.OpenWrite();

                act.ShouldThrow<FileSystemException>()
                    .WithMessage(@"* '*\ReadReadWrite.txt' is already opened.")
                    .WithInnerException<IOException>();
            }
        }

        private void FailMultipleWriters(string name, bool changeCase = false)
        {
            IFileSystem fs = this.Create();
            IFile file1 = this.CreateFile(fs, "Parent", changeCase ? "Write.txt" : "wRITE.TXT");
            IFile file2 = this.CreateFile(fs, "Parent", "Write.txt");

            using (Stream write1 = file1.OpenWrite())
            {
                Action act = () => file2.OpenWrite();

                act.ShouldThrow<FileSystemException>()
                    .WithMessage(@"* '*\Write.txt' is already opened.")
                    .WithInnerException<IOException>();
            }
        }

        private void AllowMultipleReaders(string name1, string name2 = null)
        {
            IFileSystem fs = this.Create();
            IFile file1 = this.CreateFile(fs, "Parent", name1);
            IFile file2 = this.CreateFile(fs, "Parent", name2 ?? name1);
            this.WriteAll(file2, "read me");

            string text1;
            string text2;
            using (Stream read1 = file1.OpenRead())
            using (Stream read2 = file2.OpenRead())
            {
                text1 = this.ReadToEnd(read1);
                text2 = this.ReadToEnd(read2);
            }

            text1.Should().Be(text2);
            text2.Should().Be("read me");
        }

        private void FailCreateFile(IFileSystem fs, string badName, string expectedError)
        {
            Action act = () => this.CreateFile(fs, "Dir", badName);

            act.ShouldThrow<ArgumentException>().WithMessage(expectedError).Which.ParamName.Should().Be("name");
        }
    }
}
