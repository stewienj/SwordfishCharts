// Classification: OFFICIAL
// 
// Copyright (C) 2021 Commonwealth of Australia.
// 
// All rights reserved.
// 
// The copyright herein resides with the Commonwealth of Australia.
// The material(s) may not be used, modified, copied and/or distributed
// without the written permission of the Commonwealth of Australia
// represented by Defence Science and Technology Group, the Department
// of Defence. The copyright notice above does not evidence any actual or 
// intended publication of such material(s).
// 
// This material is provided on an "AS IS" basis and the Commonwealth of
// Australia makes no representation or warranties of any kind, express 
// or implied, of merchantability or fitness for any purpose. The
// Commonwealth of Australia does not accept any liability arising from or
// connected to the use of the material.
// 
// Use of the material is entirely at the Licensee's own risk.

// based on http://stackoverflow.com/questions/26225373/edit-png-metadata-add-itxt-value


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace Swordfish.NET.General {
  public class PngChunks {
    private readonly byte[] _header = new byte[8];
    private readonly IList<Chunk> _chunks = new List<Chunk>();

    public PngChunks(string filename) {
      _header = new byte[8];
      _chunks = new List<Chunk>();

      using (FileStream stream = new FileStream(filename, FileMode.Open)) {
        using (MemoryStream memoryStream = new MemoryStream()) {
          stream.CopyTo(memoryStream);
          memoryStream.Seek(0, SeekOrigin.Begin);
          memoryStream.Read(_header, 0, _header.Length);
          while (memoryStream.Position < memoryStream.Length) {
            _chunks.Add(ChunkFromStream(memoryStream));
          }
          memoryStream.Close();
        }
      }
    }

    public PngChunks(MemoryStream memoryStream) {
      memoryStream.Seek(0, SeekOrigin.Begin);
      memoryStream.Read(_header, 0, _header.Length);
      while (memoryStream.Position < memoryStream.Length) {
        _chunks.Add(ChunkFromStream(memoryStream));
      }
    }

    public IEnumerable<string> GetText(string keyword) {
      return GetTextChunks(keyword).Select(x => GetText(x));
    }

    private string GetText(Chunk x) {
      string keyword = x.Keyword;
      return Encoding.UTF8.GetString(x.Data, keyword.Length + 1, x.Data.Length - keyword.Length - 1);
    }

    public void ReplaceText(string keyword, string text) {
      var toRemove = GetTextChunks(keyword).FirstOrDefault();
      if (toRemove != null) {
        _chunks.Remove(toRemove);
      }
      AddText(keyword, text);
    }

    private IEnumerable<Chunk> GetTextChunks(string keyword) {
      return _chunks.Where(x => x.Type == "tEXt" && x.Keyword == keyword);
    }


    public void AddText(string keyword, string text) {
      // 1-79     (keyword, null termintated by next character, max 79 chars)
      // 1        (null character)
      // 0+       (text)
      var typeBytes = Encoding.UTF8.GetBytes("tEXt");
      var keywordBytes = Encoding.UTF8.GetBytes(keyword);
      var nullByte = BitConverter.GetBytes('\0')[0];
      var textBytes = Encoding.ASCII.GetBytes(text);

      var data = new List<byte>();
      data.AddRange(keywordBytes);
      data.Add(nullByte);
      data.AddRange(textBytes);
      var chunk = new Chunk(typeBytes, data.ToArray());
      _chunks.Insert(1, chunk);
    }

    public void AddInternationalText(string keyword, string text) {
      // 1-79     (keyword, null termintated by next character, max 79 chars)
      // 1        (null character)
      // 1        (compression flag)
      // 1        (compression method)
      // 0+       (language)
      // 1        (null character)
      // 0+       (translated keyword)
      // 1        (null character)
      // 0+       (text)

      var typeBytes = Encoding.UTF8.GetBytes("iTXt");
      var keywordBytes = Encoding.UTF8.GetBytes(keyword);
      var textBytes = Encoding.UTF8.GetBytes(text);
      var nullByte = BitConverter.GetBytes('\0')[0];
      var zeroByte = BitConverter.GetBytes(0)[0];

      var data = new List<byte>();

      data.AddRange(keywordBytes);
      data.Add(nullByte);
      data.Add(zeroByte);
      data.Add(zeroByte);
      data.Add(nullByte);
      data.Add(nullByte);
      data.AddRange(textBytes);

      var chunk = new Chunk(typeBytes, data.ToArray());

      _chunks.Insert(1, chunk);
    }

    public void SaveTo(string filename) {
      using (FileStream stream = new FileStream(filename, FileMode.Create)) {
        stream.Write(_header, 0, _header.Length);
        foreach (var chunk in _chunks) {
          chunk.WriteToStream(stream);
        }
      }
    }

    public byte[] ToBytes() {
      using (var stream = new MemoryStream()) {
        stream.Write(_header, 0, _header.Length);

        foreach (var chunk in _chunks)
          chunk.WriteToStream(stream);

        var bytes = stream.ToArray();

        stream.Close();

        return bytes;
      }
    }

    private static Chunk ChunkFromStream(Stream stream) {
      var length = ReadBytes(stream, 4);
      var type = ReadBytes(stream, 4);
      var data = ReadBytes(stream, Convert.ToInt32(BitConverter.ToUInt32(length.Reverse().ToArray(), 0)));

      stream.Seek(4, SeekOrigin.Current);

      return new Chunk(type, data);
    }

    private static byte[] ReadBytes(Stream stream, int n) {
      var buffer = new byte[n];
      stream.Read(buffer, 0, n);
      return buffer;
    }

    private static void WriteBytes(Stream stream, byte[] bytes) {
      stream.Write(bytes, 0, bytes.Length);
    }

    private class Chunk {
      public Chunk(byte[] type, byte[] data) {
        _type = type;
        _data = data;
      }

      public string Type {
        get {
          return Encoding.UTF8.GetString(_type);
        }
      }

      public string Keyword {
        get {
          return Encoding.UTF8.GetString(_data.TakeWhile(x => x != 0).ToArray());
        }
      }

      public byte[] Data {
        get {
          return _data;
        }
      }

      public void WriteToStream(Stream stream) {
        WriteBytes(stream, BitConverter.GetBytes(Convert.ToUInt32(_data.Length)).Reverse().ToArray());
        WriteBytes(stream, _type);
        WriteBytes(stream, _data);
        WriteBytes(stream, CalculateCrc(_type, _data));
      }

      private static byte[] CalculateCrc(IEnumerable<byte> type, IEnumerable<byte> data) {
        var bytes = new List<byte>();

        bytes.AddRange(type);
        bytes.AddRange(data);

        var hasher = new Crc32();

        using (var stream = new MemoryStream(bytes.ToArray()))
          return hasher.ComputeHash(stream);
      }

      private readonly byte[] _type;
      private readonly byte[] _data;
    }
  }

  // Copyright (c) Damien Guard.  All rights reserved.
  // Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
  // You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
  // Originally published at http://damieng.com/blog/2006/08/08/calculating_crc32_in_c_and_net


  /// <summary>
  /// Implements a 32-bit CRC hash algorithm compatible with Zip etc.
  /// </summary>
  /// <remarks>
  /// Crc32 should only be used for backward compatibility with older file formats
  /// and algorithms. It is not secure enough for new applications.
  /// If you need to call multiple times for the same data either use the HashAlgorithm
  /// interface or remember that the result of one Compute call needs to be ~ (XOR) before
  /// being passed in as the seed for the next Compute call.
  /// </remarks>
  public sealed class Crc32 : HashAlgorithm {
    public const UInt32 DefaultPolynomial = 0xedb88320u;
    public const UInt32 DefaultSeed = 0xffffffffu;

    private static UInt32[] defaultTable = new UInt32[0];

    private readonly UInt32 seed;
    private readonly UInt32[] table;
    private UInt32 hash;

    public Crc32()
      : this(DefaultPolynomial, DefaultSeed) {
    }

    public Crc32(UInt32 polynomial, UInt32 seed) {
      table = InitializeTable(polynomial);
      this.seed = hash = seed;
    }

    public override void Initialize() {
      hash = seed;
    }

    protected override void HashCore(byte[] buffer, int start, int length) {
      hash = CalculateHash(table, hash, buffer, start, length);
    }

    protected override byte[] HashFinal() {
      var hashBuffer = UInt32ToBigEndianBytes(~hash);
      HashValue = hashBuffer;
      return hashBuffer;
    }

    public override int HashSize { get { return 32; } }

    public static UInt32 Compute(byte[] buffer) {
      return Compute(DefaultSeed, buffer);
    }

    public static UInt32 Compute(UInt32 seed, byte[] buffer) {
      return Compute(DefaultPolynomial, seed, buffer);
    }

    public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer) {
      return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
    }

    private static UInt32[] InitializeTable(UInt32 polynomial) {
      if (polynomial == DefaultPolynomial && defaultTable != null)
        return defaultTable;

      var createTable = new UInt32[256];
      for (var i = 0; i < 256; i++) {
        var entry = (UInt32)i;
        for (var j = 0; j < 8; j++)
          if ((entry & 1) == 1)
            entry = (entry >> 1) ^ polynomial;
          else
            entry = entry >> 1;
        createTable[i] = entry;
      }

      if (polynomial == DefaultPolynomial)
        defaultTable = createTable;

      return createTable;
    }

    private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, IList<byte> buffer, int start, int size) {
      var crc = seed;
      for (var i = start; i < size - start; i++)
        crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
      return crc;
    }

    private static byte[] UInt32ToBigEndianBytes(UInt32 uint32) {
      var result = BitConverter.GetBytes(uint32);

      if (BitConverter.IsLittleEndian)
        Array.Reverse(result);

      return result;
    }
  }
}
