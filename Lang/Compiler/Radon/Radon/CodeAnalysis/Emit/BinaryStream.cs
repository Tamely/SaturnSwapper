using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit;

public sealed class BinaryStream : Stream
{
	private byte[] _buffer;
	private long _position;
	private long _length;

	public BinaryStream()
		: this(Array.Empty<byte>())
	{
	}

	public BinaryStream(byte[] buffer)
	{
		_buffer = buffer;
		_position = 0L;
		_length = buffer.Length;
	}
	
	public BinaryStream(int capacity)
	{
		_buffer = new byte[capacity];
		_position = 0L;
		_length = capacity;
	}

	private void Resize(long size)
	{
		var array = new byte[size];
		Array.Copy(_buffer, array, _buffer.Length);
		_buffer = array;
		_length = size;
	}

	public override void Flush()
	{
		_buffer = Array.Empty<byte>();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		var num = Math.Min(count, _length - _position);
		Array.Copy(_buffer, _position, buffer, offset, num);
		_position += num;
		return (int)num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		var num = origin switch
		{
			SeekOrigin.Begin => offset, 
			SeekOrigin.Current => _position + offset, 
			SeekOrigin.End => _length - offset, 
			_ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
		};

		_position = Math.Max(0L, Math.Min(num, _length));
		return _position;
	}

	public override void SetLength(long value)
	{
		Resize((int)value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		var num = _position + count;
		if (num > _length)
		{
			Resize(num);
		}
		Array.Copy(buffer, offset, _buffer, _position, count);
		_position = num;
	}

	public unsafe void Write<T>(T value) 
		where T : unmanaged
	{
		var num = sizeof(T);
		var num2 = _position + num;
		if (num2 > _length)
		{
			Resize(num2);
		}
		
		fixed (byte* ptr = &_buffer[_position])
		{
			*(T*)ptr = value;
		}
	}

	public void WriteString(string value, bool writeLength = true)
	{
		var array = value.ToCharArray();
		var array2 = new byte[array.Length];
		for (var i = 0; i < array.Length; i++)
		{
			array2[i] = (byte)array[i];
		}
		
		WriteArray(array2, writeLength);
	}

	public void WriteArray<T>(T[] values, bool writeLength = true) 
		where T : unmanaged
	{
		if (writeLength)
		{
			Write(values.Length);
		}
		foreach (var value in values)
		{
			Write(value);
		}
	}

	public unsafe T Read<T>() 
		where T : unmanaged
	{
		var num = sizeof(T);
		var num2 = _position + num;
		if (num2 > _length)
		{
			throw new ArgumentOutOfRangeException();
		}
		
		var array = new byte[num];
		Array.Copy(_buffer, _position, array, 0L, num);
		T result;
		fixed (byte* ptr = array)
		{
			result = *(T*)ptr;
		}
		_position = num2;
		return result;
	}

	public string ReadString(int length)
	{
		var array = new byte[length];
		for (var i = 0; i < length; i++)
		{
			array[i] = Read<byte>();
		}
		
		return new string(array.Select(x => (char)x).ToArray());
	}

	public T[] ReadArray<T>(int length) 
		where T : unmanaged
	{
		var array = new T[length];
		for (var i = 0; i < length; i++)
		{
			array[i] = Read<T>();
		}
		
		return array;
	}

	public byte[] ToArray()
	{
		var array = new byte[_length];
		Array.Copy(_buffer, array, _length);
		return array;
	}
	
	public override bool CanRead => true;
	public override bool CanSeek => true;
	public override bool CanWrite => true;
	public override long Length => _length;

	public override long Position
	{
		get => _position;
		set => _position = (int)value;
	}
}
