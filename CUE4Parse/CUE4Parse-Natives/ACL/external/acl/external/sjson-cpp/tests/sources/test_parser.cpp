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

#include <sjson/parser.h>

using namespace sjson;

static Parser parser_from_c_str(const char* c_str)
{
	return Parser(c_str, std::strlen(c_str));
}

TEST_CASE("Parser Misc", "[parser]")
{
	{
		Parser parser = parser_from_c_str("");
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("");
		CHECK(parser.remainder_is_comments_and_whitespace());
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("     ");
		CHECK(parser.remainder_is_comments_and_whitespace());
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("// lol \\n     ");
		CHECK(parser.remainder_is_comments_and_whitespace());
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("\"key-one\" = true");
		bool value = false;
		CHECK(parser.read("key-one", value));
		CHECK(value == true);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = /* bar */ true");
		bool value = false;
		CHECK(parser.read("key", value));
		CHECK(value == true);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = /* bar * true");
		bool value = false;
		CHECK_FALSE(parser.read("key", value));
		CHECK_FALSE(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = // bar \ntrue");
		bool value = false;
		CHECK(parser.read("key", value));
		CHECK(value == true);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key /* bar */ = true");
		bool value = false;
		CHECK(parser.read("key", value));
		CHECK(value == true);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("/* bar */ key = true");
		bool value = false;
		CHECK(parser.read("key", value));
		CHECK(value == true);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}
}

TEST_CASE("Parser Bool Reading", "[parser]")
{
	{
		Parser parser = parser_from_c_str("key = true");
		bool value = false;
		CHECK(parser.read("key", value));
		CHECK(value == true);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = false");
		bool value = true;
		CHECK(parser.read("key", value));
		CHECK(value == false);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = 0");
		bool value = true;
		CHECK_FALSE(parser.try_read("key", value, false));
		CHECK(value == false);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = true");
		bool value = false;
		CHECK(parser.try_read("key", value, false));
		CHECK(value == true);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}
}

TEST_CASE("Parser String Reading", "[parser]")
{
	{
		Parser parser = parser_from_c_str("key = \"Quoted string\"");
		StringView value;
		CHECK(parser.read("key", value));
		CHECK(value == "Quoted string");
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		// Note: Escaped quotes \" are left escaped within the StringView because we do not allocate memory
		Parser parser = parser_from_c_str("key = \"Quoted \\\" string\"");
		StringView value;
		CHECK(parser.read("key", value));
		CHECK(value == "Quoted \\\" string");
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"New\\nline\"");
		StringView value;
		CHECK(parser.read("key", value));
		CHECK(value == "New\\nline");
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"Tab\\tulator\"");
		StringView value;
		CHECK(parser.read("key", value));
		CHECK(value == "Tab\\tulator");
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"Tab\\tulator\"");
		StringView value;
		CHECK(parser.read("key", value));
		CHECK(value == "Tab\\tulator");
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = 0");
		StringView value;
		CHECK_FALSE(parser.try_read("key", value, "default"));
		CHECK(value == "default");
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"good\"");
		StringView value;
		CHECK(parser.try_read("key", value, "default"));
		CHECK(value == "good");
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"bad");
		StringView value;
		CHECK_FALSE(parser.read("key", value));
		CHECK_FALSE(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = bad");
		StringView value;
		CHECK_FALSE(parser.read("key", value));
		CHECK_FALSE(parser.is_valid());
	}
}

TEST_CASE("Parser Number Reading", "[parser]")
{
	// Number reading
	{
		Parser parser = parser_from_c_str("key = 123.456789");
		double value = 0.0;
		CHECK(parser.read("key", value));
		CHECK(value == 123.456789);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"nan\"");
		double value = 0.0;
		CHECK(parser.read("key", value));
		CHECK(std::isnan(value));
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"inf\"");
		double value = 0.0;
		CHECK(parser.read("key", value));
		CHECK(std::isinf(value));
		CHECK(value > 0.0);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"-inf\"");
		double value = 0.0;
		CHECK(parser.read("key", value));
		CHECK(std::isinf(value));
		CHECK(value < 0.0);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 123.456789");
		float value = 0.0F;
		CHECK(parser.read("key", value));
		CHECK(value == 123.456789F);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"nan\"");
		float value = 0.0F;
		CHECK(parser.read("key", value));
		CHECK(std::isnan(value));
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"inf\"");
		float value = 0.0F;
		CHECK(parser.read("key", value));
		CHECK(std::isinf(value));
		CHECK(value > 0.0F);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = \"-inf\"");
		float value = 0.0F;
		CHECK(parser.read("key", value));
		CHECK(std::isinf(value));
		CHECK(value < 0.0F);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = -123");
		int8_t value = 0;
		CHECK(parser.read("key", value));
		CHECK(value == -123);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 123");
		uint8_t value = 0;
		CHECK(parser.read("key", value));
		CHECK(value == 123);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = -1234");
		int16_t value = 0;
		CHECK(parser.read("key", value));
		CHECK(value == -1234);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 1234");
		uint16_t value = 0;
		CHECK(parser.read("key", value));
		CHECK(value == 1234);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = -123456");
		int32_t value = 0;
		CHECK(parser.read("key", value));
		CHECK(value == -123456);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 123456");
		uint32_t value = 0;
		CHECK(parser.read("key", value));
		CHECK(value == 123456);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = -1234567890123456");
		int64_t value = 0;
		CHECK(parser.read("key", value));
		CHECK(value == -1234567890123456LL);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 1234567890123456");
		uint64_t value = 0;
		CHECK(parser.read("key", value));
		CHECK(value == 1234567890123456ULL);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	// Number try reading

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		double value = 0.0;
		CHECK_FALSE(parser.try_read("key", value, 1.0));
		CHECK(value == 1.0);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 2.0");
		double value = 0.0;
		CHECK(parser.try_read("key", value, 1.0));
		CHECK(value == 2.0);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		float value = 0.0F;
		CHECK_FALSE(parser.try_read("key", value, 1.0F));
		CHECK(value == 1.0F);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 2.0");
		float value = 0.0F;
		CHECK(parser.try_read("key", value, 1.0F));
		CHECK(value == 2.0F);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		int8_t value = 0;
		CHECK_FALSE(parser.try_read("key", value, 1));
		CHECK(value == 1);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = -123");
		int8_t value = 0;
		CHECK(parser.try_read("key", value, 1));
		CHECK(value == -123);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		uint8_t value = 0;
		CHECK_FALSE(parser.try_read("key", value, 1));
		CHECK(value == 1);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 123");
		uint8_t value = 0;
		CHECK(parser.try_read("key", value, 1));
		CHECK(value == 123);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		int16_t value = 0;
		CHECK_FALSE(parser.try_read("key", value, 1));
		CHECK(value == 1);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = -1234");
		int16_t value = 0;
		CHECK(parser.try_read("key", value, 1));
		CHECK(value == -1234);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		uint16_t value = 0;
		CHECK_FALSE(parser.try_read("key", value, 1));
		CHECK(value == 1);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 1234");
		uint16_t value = 0;
		CHECK(parser.try_read("key", value, 1));
		CHECK(value == 1234);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		int32_t value = 0;
		CHECK_FALSE(parser.try_read("key", value, 1));
		CHECK(value == 1);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = -123456");
		int32_t value = 0;
		CHECK(parser.try_read("key", value, 1));
		CHECK(value == -123456);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		uint32_t value = 0;
		CHECK_FALSE(parser.try_read("key", value, 1));
		CHECK(value == 1);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 123456");
		uint32_t value = 0;
		CHECK(parser.try_read("key", value, 1));
		CHECK(value == 123456);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		int64_t value = 0;
		CHECK_FALSE(parser.try_read("key", value, 1));
		CHECK(value == 1);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = -1234567890123456");
		int64_t value = 0;
		CHECK(parser.try_read("key", value, 1));
		CHECK(value == -1234567890123456LL);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		uint64_t value = 0;
		CHECK_FALSE(parser.try_read("key", value, 1));
		CHECK(value == 1);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = 1234567890123456");
		uint64_t value = 0;
		CHECK(parser.try_read("key", value, 1));
		CHECK(value == 1234567890123456ULL);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}
}

TEST_CASE("Parser Array Reading", "[parser]")
{
	{
		Parser parser = parser_from_c_str("key = [ 123.456789, 456.789, 151.091 ]");
		double value[3] = { 0.0, 0.0, 0.0 };
		CHECK(parser.read("key", value, 3));
		CHECK(value[0] == 123.456789);
		CHECK(value[1] == 456.789);
		CHECK(value[2] == 151.091);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = [ \"123.456789\", \"456.789\", \"151.091\" ]");
		StringView value[3];
		CHECK(parser.read("key", value, 3));
		CHECK(value[0] == "123.456789");
		CHECK(value[1] == "456.789");
		CHECK(value[2] == "151.091");
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		double value[3] = { 0.0, 0.0, 0.0 };
		CHECK_FALSE(parser.try_read("key", value, 3, 1.0));
		CHECK(value[0] == 1.0);
		CHECK(value[1] == 1.0);
		CHECK(value[2] == 1.0);
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = [ 123.456789, 456.789, 151.091 ]");
		double value[3] = { 0.0, 0.0, 0.0 };
		CHECK(parser.try_read("key", value, 3, 1.0));
		CHECK(value[0] == 123.456789);
		CHECK(value[1] == 456.789);
		CHECK(value[2] == 151.091);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("bad_key = \"bad\"");
		StringView value[3];
		CHECK_FALSE(parser.try_read("key", value, 3, "default"));
		CHECK(value[0] == "default");
		CHECK(value[1] == "default");
		CHECK(value[2] == "default");
		CHECK_FALSE(parser.eof());
		CHECK(parser.is_valid());
	}

	{
		Parser parser = parser_from_c_str("key = [ \"123.456789\", \"456.789\", \"151.091\" ]");
		StringView value[3];
		CHECK(parser.try_read("key", value, 3, "default"));
		CHECK(value[0] == "123.456789");
		CHECK(value[1] == "456.789");
		CHECK(value[2] == "151.091");
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}

#if 0
	{
		Parser parser = parser_from_c_str("key = [ 123.456789, \"456.789\", false, [ 1.0, true ], { key0 = 1.0, key1 = false } ]");

		CHECK(parser.array_begins("key"));
		// TODO
		CHECK(parser.array_ends());
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}
#endif
}

TEST_CASE("Parser Null Reading", "[parser]")
{
	{
		Parser parser = parser_from_c_str("key = null");
		bool value_bool = false;
		CHECK_FALSE(parser.try_read("key", value_bool, true));
		CHECK(value_bool == true);
		double value_dbl = 0.0;
		CHECK_FALSE(parser.try_read("key", value_dbl, 1.0));
		CHECK(value_dbl == 1.0);
		CHECK(parser.eof());
		CHECK(parser.is_valid());
	}
}
