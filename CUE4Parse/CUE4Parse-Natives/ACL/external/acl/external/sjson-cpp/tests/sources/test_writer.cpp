////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2018 Nicholas Frechette, Cody Jones, and sjson-cpp contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

#include <catch2/catch.hpp>

#include <sjson/writer.h>

#include <sstream>
#include <string>

using namespace sjson;

class StringStreamWriter final : public StreamWriter
{
public:
	StringStreamWriter() = default;

	virtual void write(const void* buffer, size_t buffer_size) override
	{
		m_buffer.sputn(reinterpret_cast<const char*>(buffer), buffer_size);
	}

	std::string str() const { return m_buffer.str(); }

private:
	std::stringbuf m_buffer;
};

TEST_CASE("Writer Object Bool Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", true);
		CHECK(str_writer.str() == "key = true\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", false);
		CHECK(str_writer.str() == "key = false\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = true;
		CHECK(str_writer.str() == "key = true\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = false;
		CHECK(str_writer.str() == "key = false\r\n");
	}
}

TEST_CASE("Writer Object String Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", "some string");
		CHECK(str_writer.str() == "key = \"some string\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = "some string";
		CHECK(str_writer.str() == "key = \"some string\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", "some\tstring");
		CHECK(str_writer.str() == "key = \"some\tstring\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = "some\tstring";
		CHECK(str_writer.str() == "key = \"some\tstring\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", "some\nstring");
		CHECK(str_writer.str() == "key = \"some\nstring\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = "some\nstring";
		CHECK(str_writer.str() == "key = \"some\nstring\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", "some\"string");
		CHECK(str_writer.str() == "key = \"some\"string\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = "some\"string";
		CHECK(str_writer.str() == "key = \"some\"string\"\r\n");
	}
}

TEST_CASE("Writer Object Number Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", 123.0);
		CHECK(str_writer.str() == "key = 123\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = 123.0;
		CHECK(str_writer.str() == "key = 123\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", 123.456);
		CHECK(str_writer.str() == "key = 123.456\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = 123.456;
		CHECK(str_writer.str() == "key = 123.456\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", std::nan(""));
		CHECK(str_writer.str() == "key = \"nan\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = std::nan("");
		CHECK(str_writer.str() == "key = \"nan\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", std::numeric_limits<double>::infinity());
		CHECK(str_writer.str() == "key = \"inf\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = std::numeric_limits<double>::infinity();
		CHECK(str_writer.str() == "key = \"inf\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", -std::numeric_limits<double>::infinity());
		CHECK(str_writer.str() == "key = \"-inf\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = -std::numeric_limits<double>::infinity();
		CHECK(str_writer.str() == "key = \"-inf\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", 123.0F);
		CHECK(str_writer.str() == "key = 123\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = 123.0F;
		CHECK(str_writer.str() == "key = 123\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", 123.5F);
		CHECK(str_writer.str() == "key = 123.5\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = 123.5F;
		CHECK(str_writer.str() == "key = 123.5\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", std::nanf(""));
		CHECK(str_writer.str() == "key = \"nan\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = std::nanf("");
		CHECK(str_writer.str() == "key = \"nan\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", std::numeric_limits<float>::infinity());
		CHECK(str_writer.str() == "key = \"inf\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = std::numeric_limits<float>::infinity();
		CHECK(str_writer.str() == "key = \"inf\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", -std::numeric_limits<float>::infinity());
		CHECK(str_writer.str() == "key = \"-inf\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = -std::numeric_limits<float>::infinity();
		CHECK(str_writer.str() == "key = \"-inf\"\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		int8_t value = -123;
		writer.insert("key", value);
		CHECK(str_writer.str() == "key = -123\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		int8_t value = -123;
		writer["key"] = value;
		CHECK(str_writer.str() == "key = -123\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		uint8_t value = 123;
		writer.insert("key", value);
		CHECK(str_writer.str() == "key = 123\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		uint8_t value = 123;
		writer["key"] = value;
		CHECK(str_writer.str() == "key = 123\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		int16_t value = -1234;
		writer.insert("key", value);
		CHECK(str_writer.str() == "key = -1234\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		int16_t value = -1234;
		writer["key"] = value;
		CHECK(str_writer.str() == "key = -1234\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		uint16_t value = 1234;
		writer.insert("key", value);
		CHECK(str_writer.str() == "key = 1234\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		uint16_t value = 1234;
		writer["key"] = value;
		CHECK(str_writer.str() == "key = 1234\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		int32_t value = -123456;
		writer.insert("key", value);
		CHECK(str_writer.str() == "key = -123456\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		int32_t value = -123456;
		writer["key"] = value;
		CHECK(str_writer.str() == "key = -123456\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		uint32_t value = 123456;
		writer.insert("key", value);
		CHECK(str_writer.str() == "key = 123456\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		uint32_t value = 123456;
		writer["key"] = value;
		CHECK(str_writer.str() == "key = 123456\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		int64_t value = -1234567890123456LL;
		writer.insert("key", value);
		CHECK(str_writer.str() == "key = -1234567890123456\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		int64_t value = -1234567890123456LL;
		writer["key"] = value;
		CHECK(str_writer.str() == "key = -1234567890123456\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		uint64_t value = 1234567890123456ULL;
		writer.insert("key", value);
		CHECK(str_writer.str() == "key = 1234567890123456\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		uint64_t value = 1234567890123456ULL;
		writer["key"] = value;
		CHECK(str_writer.str() == "key = 1234567890123456\r\n");
	}
}

TEST_CASE("Writer Object Array Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& /*array_writer*/)
		{
		});
		CHECK(str_writer.str() == "key = [  ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push(123.5);
			array_writer.push(456.5);
		});
		CHECK(str_writer.str() == "key = [ 123.5, 456.5 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = [](ArrayWriter& array_writer)
		{
			array_writer.push(123.5);
			array_writer.push(456.5);
		};
		CHECK(str_writer.str() == "key = [ 123.5, 456.5 ]\r\n");
	}
}

TEST_CASE("Writer Object Object Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ObjectWriter& /*object_writer*/)
		{
		});
		CHECK(str_writer.str() == "key = {\r\n}\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ObjectWriter& object_writer)
		{
			object_writer["key0"] = 123.5;
			object_writer["key1"] = 456.5;
		});
		CHECK(str_writer.str() == "key = {\r\n\tkey0 = 123.5\r\n\tkey1 = 456.5\r\n}\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer["key"] = [](ObjectWriter& object_writer)
		{
			object_writer["key0"] = 123.5;
			object_writer["key1"] = 456.5;
		};
		CHECK(str_writer.str() == "key = {\r\n\tkey0 = 123.5\r\n\tkey1 = 456.5\r\n}\r\n");
	}
}

TEST_CASE("Writer Array Bool Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push(true);
		});
		CHECK(str_writer.str() == "key = [ true ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push(false);
		});
		CHECK(str_writer.str() == "key = [ false ]\r\n");
	}
}

TEST_CASE("Writer Array String Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push("some string");
		});
		CHECK(str_writer.str() == "key = [ \"some string\" ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push("some\tstring");
		});
		CHECK(str_writer.str() == "key = [ \"some\tstring\" ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push("some\nstring");
		});
		CHECK(str_writer.str() == "key = [ \"some\nstring\" ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push("some\"string");
		});
		CHECK(str_writer.str() == "key = [ \"some\"string\" ]\r\n");
	}
}

TEST_CASE("Writer Array Number Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push(123.0);
		});
		CHECK(str_writer.str() == "key = [ 123 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push(123.456);
		});
		CHECK(str_writer.str() == "key = [ 123.456 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push(std::nan(""));
			array_writer.push(std::numeric_limits<double>::infinity());
			array_writer.push(-std::numeric_limits<double>::infinity());
		});
		CHECK(str_writer.str() == "key = [ \"nan\", \"inf\", \"-inf\" ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push(123.0F);
		});
		CHECK(str_writer.str() == "key = [ 123 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push(123.5F);
		});
		CHECK(str_writer.str() == "key = [ 123.5 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push(std::nanf(""));
			array_writer.push(std::numeric_limits<float>::infinity());
			array_writer.push(-std::numeric_limits<float>::infinity());
		});
		CHECK(str_writer.str() == "key = [ \"nan\", \"inf\", \"-inf\" ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			int8_t value = -123;
			array_writer.push(value);
		});
		CHECK(str_writer.str() == "key = [ -123 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			uint8_t value = 123;
			array_writer.push(value);
		});
		CHECK(str_writer.str() == "key = [ 123 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			int16_t value = -1234;
			array_writer.push(value);
		});
		CHECK(str_writer.str() == "key = [ -1234 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			uint16_t value = 1234;
			array_writer.push(value);
		});
		CHECK(str_writer.str() == "key = [ 1234 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			int32_t value = -123456;
			array_writer.push(value);
		});
		CHECK(str_writer.str() == "key = [ -123456 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			uint32_t value = 123456;
			array_writer.push(value);
		});
		CHECK(str_writer.str() == "key = [ 123456 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			int64_t value = -1234567890123456LL;
			array_writer.push(value);
		});
		CHECK(str_writer.str() == "key = [ -1234567890123456 ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			uint64_t value = 1234567890123456ULL;
			array_writer.push(value);
		});
		CHECK(str_writer.str() == "key = [ 1234567890123456 ]\r\n");
	}
}

TEST_CASE("Writer Array Array Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push([](ArrayWriter& /*array_writer*/)
			{
			});
		});
		CHECK(str_writer.str() == "key = [ [  ] ]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push([](ArrayWriter& array_writer1)
			{
				array_writer1.push(123.5);
				array_writer1.push(456.5);
			});
		});
		CHECK(str_writer.str() == "key = [ [ 123.5, 456.5 ] ]\r\n");
	}
}

TEST_CASE("Writer Array Object Writing", "[writer]")
{
	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push([](ObjectWriter& /*object_writer*/)
			{
			});
		});
		CHECK(str_writer.str() == "key = [ \r\n\t{\r\n\t}\r\n]\r\n");
	}

	{
		StringStreamWriter str_writer;
		Writer writer(str_writer);
		writer.insert("key", [](ArrayWriter& array_writer)
		{
			array_writer.push([](ObjectWriter& object_writer1)
			{
				object_writer1["key0"] = 123.5;
				object_writer1["key1"] = 456.5;
			});
		});
		CHECK(str_writer.str() == "key = [ \r\n\t{\r\n\t\tkey0 = 123.5\r\n\t\tkey1 = 456.5\r\n\t}\r\n]\r\n");
	}
}
