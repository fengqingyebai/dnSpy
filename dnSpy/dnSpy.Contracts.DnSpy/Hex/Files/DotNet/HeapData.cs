﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.ObjectModel;
using System.Text;

namespace dnSpy.Contracts.Hex.Files.DotNet {
	/// <summary>
	/// #GUID heap record data
	/// </summary>
	public sealed class GuidHeapRecordData : GuidData {
		/// <summary>
		/// Gets the heap
		/// </summary>
		public GUIDHeap Heap { get; }

		/// <summary>
		/// Gets the GUID index (1-based)
		/// </summary>
		public uint Index { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buffer">Buffer</param>
		/// <param name="position">Position</param>
		/// <param name="heap">Owner heap</param>
		/// <param name="index">Guid index (1-based)</param>
		public GuidHeapRecordData(HexBuffer buffer, HexPosition position, GUIDHeap heap, uint index)
			: base(buffer, position) {
			if (heap == null)
				throw new ArgumentNullException(nameof(heap));
			if (index == 0)
				throw new ArgumentOutOfRangeException(nameof(index));
			Heap = heap;
			Index = index;
		}
	}

	/// <summary>
	/// #Strings heap record data
	/// </summary>
	public sealed class StringsHeapRecordData : StructureData {
		const string NAME = "StringZ";

		/// <summary>
		/// Gets the owner heap
		/// </summary>
		public StringsHeap Heap { get; }

		/// <summary>
		/// Gets tokens of records referencing this string
		/// </summary>
		public ReadOnlyCollection<uint> Tokens { get; }

		/// <summary>
		/// Gets the string
		/// </summary>
		public StructField<StringData> String { get; }

		/// <summary>
		/// Gets the terminator or null if there's none
		/// </summary>
		public StructField<ByteData> Terminator { get; }

		/// <summary>
		/// Gets the fields
		/// </summary>
		protected override BufferField[] Fields { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buffer">Buffer</param>
		/// <param name="stringSpan">Span of string, not including the terminating zero</param>
		/// <param name="hasTerminatingZero">true if there's a terminating zero, false if there's no terminating zero
		/// or if the string is too long</param>
		/// <param name="heap">Owner heap</param>
		/// <param name="tokens">Tokens of records referencing this string</param>
		public StringsHeapRecordData(HexBuffer buffer, HexSpan stringSpan, bool hasTerminatingZero, StringsHeap heap, uint[] tokens)
			: base(NAME, new HexBufferSpan(buffer, HexSpan.FromBounds(stringSpan.Start, stringSpan.End + (hasTerminatingZero ? 1 : 0)))) {
			if (heap == null)
				throw new ArgumentNullException(nameof(heap));
			if (tokens == null)
				throw new ArgumentNullException(nameof(tokens));
			Heap = heap;
			Tokens = new ReadOnlyCollection<uint>(tokens);
			String = new StructField<StringData>("String", new StringData(new HexBufferSpan(buffer, stringSpan), Encoding.UTF8));
			if (hasTerminatingZero)
				Terminator = new StructField<ByteData>("Terminator", new ByteData(buffer, stringSpan.End));
			if (Terminator != null) {
				Fields = new BufferField[] {
					String,
					Terminator,
				};
			}
			else {
				Fields = new BufferField[] {
					String,
				};
			}
		}
	}

	/// <summary>
	/// #US heap record data
	/// </summary>
	public sealed class USHeapRecordData : StructureData {
		const string NAME = "US";

		/// <summary>
		/// Gets the owner heap
		/// </summary>
		public USHeap Heap { get; }

		/// <summary>
		/// Gets the length
		/// </summary>
		public StructField<BlobEncodedUInt32Data> Length { get; }

		/// <summary>
		/// Gets the string data
		/// </summary>
		public StructField<StringData> String { get; }

		/// <summary>
		/// Gets the terminal byte or null if none exists
		/// </summary>
		public StructField<ByteData> TerminalByte { get; }

		/// <summary>
		/// Gets the fields
		/// </summary>
		protected override BufferField[] Fields { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buffer">Buffer</param>
		/// <param name="lengthSpan">Span of length</param>
		/// <param name="stringSpan">Span of string data</param>
		/// <param name="terminalByteSpan">Span of terminal byte (0 or 1 byte)</param>
		/// <param name="heap">Owner heap</param>
		public USHeapRecordData(HexBuffer buffer, HexSpan lengthSpan, HexSpan stringSpan, HexSpan terminalByteSpan, USHeap heap)
			: base(NAME, new HexBufferSpan(buffer, HexSpan.FromBounds(lengthSpan.Start, terminalByteSpan.End))) {
			if (lengthSpan.End != stringSpan.Start)
				throw new ArgumentOutOfRangeException(nameof(stringSpan));
			if (stringSpan.End != terminalByteSpan.Start)
				throw new ArgumentOutOfRangeException(nameof(stringSpan));
			if (!terminalByteSpan.IsEmpty && terminalByteSpan.Length != 1)
				throw new ArgumentOutOfRangeException(nameof(terminalByteSpan));
			if (heap == null)
				throw new ArgumentNullException(nameof(heap));
			Heap = heap;
			Length = new StructField<BlobEncodedUInt32Data>("Length", new BlobEncodedUInt32Data(new HexBufferSpan(buffer, lengthSpan)));
			String = new StructField<StringData>("String", new StringData(new HexBufferSpan(buffer, stringSpan), Encoding.Unicode));
			if (!terminalByteSpan.IsEmpty)
				TerminalByte = new StructField<ByteData>("TerminalByte", new ByteData(new HexBufferSpan(buffer, terminalByteSpan)));
			if (TerminalByte != null) {
				Fields = new BufferField[] {
					Length,
					String,
					TerminalByte,
				};
			}
			else {
				Fields = new BufferField[] {
					Length,
					String,
				};
			}
		}
	}

	/// <summary>
	/// #Blob heap record data
	/// </summary>
	public sealed class BlobHeapRecordData : StructureData {
		const string NAME = "Blob";

		/// <summary>
		/// Gets the owner heap
		/// </summary>
		public BlobHeap Heap { get; }

		/// <summary>
		/// Gets the token referencing the blob or 0 if none (eg. referenced from data in the #Blob)
		/// </summary>
		public uint Token { get; }

		/// <summary>
		/// Gets the length
		/// </summary>
		public StructField<BlobEncodedUInt32Data> Length { get; }

		/// <summary>
		/// Gets the data
		/// </summary>
		public StructField<BufferData> Data { get; }

		/// <summary>
		/// Gets the fields
		/// </summary>
		protected override BufferField[] Fields { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buffer">Buffer</param>
		/// <param name="span">Span</param>
		/// <param name="lengthSpan">Span of length</param>
		/// <param name="data">Data</param>
		/// <param name="token">Token referencing this blob or 0 if none</param>
		/// <param name="heap">Owner heap</param>
		public BlobHeapRecordData(HexBuffer buffer, HexSpan span, HexSpan lengthSpan, BufferData data, uint token, BlobHeap heap)
			: base(NAME, new HexBufferSpan(buffer, span)) {
			if (lengthSpan.Start != span.Start)
				throw new ArgumentOutOfRangeException(nameof(lengthSpan));
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (heap == null)
				throw new ArgumentNullException(nameof(heap));
			Heap = heap;
			Token = token;
			Length = new StructField<BlobEncodedUInt32Data>("Length", new BlobEncodedUInt32Data(new HexBufferSpan(buffer, lengthSpan)));
			Data = new StructField<BufferData>("Data", data);
			Fields = new BufferField[] {
				Length,
				Data,
			};
		}
	}
}
