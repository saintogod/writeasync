﻿// <copyright file="FakeFileSystem.cs" company="Brian Rogers">
// Copyright (c) Brian Rogers. All rights reserved.
// </copyright>

namespace FileSystemSample
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public sealed class FakeFileSystem : IFileSystem
    {
        private readonly FakeDirectories fakeDirs;

        public FakeFileSystem()
        {
            this.fakeDirs = new FakeDirectories();
        }

        public IDirectory Create(FullPath name)
        {
            return this.fakeDirs.Create(name);
        }

        private sealed class FakeDirectories
        {
            private readonly Dictionary<FullPath, FakeDirectory> fakeDirs;

            public FakeDirectories()
            {
                this.fakeDirs = new Dictionary<FullPath, FakeDirectory>();
            }

            public IDirectory Create(FullPath name)
            {
                FakeDirectory dir;
                if (!this.fakeDirs.TryGetValue(name, out dir))
                {
                    dir = new FakeDirectory(name);
                    this.fakeDirs.Add(name, dir);
                }

                return dir;
            }

            private sealed class FakeDirectory : IDirectory
            {
                private readonly FakeFiles fakeFiles;

                public FakeDirectory(FullPath path)
                {
                    this.Path = path;
                    this.fakeFiles = new FakeFiles(this.Path);
                }

                public FullPath Path { get; private set; }

                public IFile CreateFile(PathPart name) => this.fakeFiles.Create(name);

                public IFile[] GetFiles(string pattern) => this.fakeFiles.Get(pattern);

                private sealed class FakeFiles
                {
                    private readonly Dictionary<PathPart, FakeFile> fakeFiles;
                    private readonly FullPath root;

                    public FakeFiles(FullPath root)
                    {
                        this.fakeFiles = new Dictionary<PathPart, FakeFile>();
                        this.root = root;
                    }

                    public IFile Create(PathPart name)
                    {
                        FakeFile file;
                        if (!this.fakeFiles.TryGetValue(name, out file))
                        {
                            file = new FakeFile(this.root.Combine(name));
                            this.fakeFiles.Add(name, file);
                        }

                        return file;
                    }

                    public IFile[] Get(string pattern) => this.GetMatching(pattern).ToArray();

                    private IEnumerable<IFile> GetMatching(string rawPattern)
                    {
                        PathPattern pattern = new PathPattern(rawPattern);
                        foreach (KeyValuePair<PathPart, FakeFile> kv in this.fakeFiles)
                        {
                            if (pattern.Matches(kv.Key))
                            {
                                yield return kv.Value;
                            }
                        }
                    }

                    private sealed class PathPattern
                    {
                        private readonly Regex regex;

                        public PathPattern(string rawPattern)
                        {
                            this.regex = AsRegex(rawPattern);
                        }

                        public bool Matches(PathPart path) => this.regex.IsMatch(path.ToString());

                        private static Regex AsRegex(string rawPattern)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append('^');
                            foreach (char c in rawPattern)
                            {
                                AppendRegex(sb, c);
                            }

                            sb.Append('$');
                            System.Console.WriteLine(sb.ToString());
                            return new Regex(sb.ToString(), RegexOptions.IgnoreCase);
                        }

                        private static void AppendRegex(StringBuilder sb, char c)
                        {
                            switch (c)
                            {
                                case '.':
                                    sb.Append('\\');
                                    sb.Append(c);
                                    break;
                                case '*':
                                    sb.Append('.');
                                    sb.Append(c);
                                    break;
                                case '?':
                                    sb.Append('.');
                                    break;
                                default:
                                    sb.Append(c);
                                    break;
                            }
                        }
                    }

                    private sealed class FakeFile : IFile
                    {
                        private Buffer buffer;

                        public FakeFile(FullPath path)
                        {
                            this.Path = path;
                            this.buffer = new Buffer();
                        }

                        public FullPath Path { get; private set; }

                        public Stream OpenRead()
                        {
                            try
                            {
                                return this.buffer.OpenRead();
                            }
                            catch (IOException e)
                            {
                                throw FileSystemError.AlreadyOpen(this.Path, e);
                            }
                        }

                        public Stream OpenWrite()
                        {
                            try
                            {
                                return this.buffer.OpenWrite();
                            }
                            catch (IOException e)
                            {
                                throw FileSystemError.AlreadyOpen(this.Path, e);
                            }
                        }

                        private sealed class Buffer
                        {
                            private byte[] buffer;
                            private int write;

                            public Buffer()
                            {
                                this.buffer = new byte[0];
                            }

                            public Stream OpenRead()
                            {
                                if (this.write != 0)
                                {
                                    throw new IOException("Cannot read.");
                                }

                                return new MemoryStream(this.buffer, false);
                            }

                            public Stream OpenWrite()
                            {
                                if (this.write != 0)
                                {
                                    throw new IOException("Cannot write.");
                                }

                                ++this.write;
                                return new WriteStream(this.OnCloseWriter);
                            }

                            private void OnCloseWriter(byte[] newBuffer)
                            {
                                this.buffer = newBuffer;
                                --this.write;
                            }

                            private sealed class WriteStream : MemoryStream
                            {
                                private readonly Action<byte[]> onClose;

                                public WriteStream(Action<byte[]> onClose)
                                {
                                    this.onClose = onClose;
                                }

                                public override void Close()
                                {
                                    try
                                    {
                                        this.onClose(this.ToArray());
                                    }
                                    finally
                                    {
                                        base.Close();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
