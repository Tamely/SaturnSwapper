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

#include <sjson/string_view.h>

#include <cstring>

using namespace sjson;

TEST_CASE("StringView", "[string]")
{
	CHECK(StringView() == StringView(""));
	CHECK(StringView() == "");
	CHECK(StringView().size() == 0);
	CHECK(StringView().c_str() != nullptr);
	CHECK(StringView("").size() == 0);
	CHECK(StringView("").c_str() != nullptr);

	const char* str0 = "this is a test string";
	const char* str1 = "this is not a test string";
	const char* str2 = "this is a test asset!";

	CHECK(StringView(str0) == str0);
	CHECK(StringView(str0) != str1);
	CHECK(StringView(str0) != str2);
	CHECK(StringView(str0) == StringView(str0));
	CHECK(StringView(str0) != StringView(str1));
	CHECK(StringView(str0) != StringView(str2));
	CHECK(StringView(str0).c_str() == str0);
	CHECK(StringView(str0).size() == std::strlen(str0));
	CHECK(StringView(str0, 4) == StringView(str1, 4));
	CHECK(StringView(str0, 4) == StringView("this"));

	StringView view0(str0);
	CHECK(view0 == str0);
	view0 = str1;
	CHECK(view0 == str1);

	CHECK(StringView().empty() == true);
	CHECK(StringView("").empty() == true);
	CHECK(view0.empty() == false);

	CHECK(view0[0] == 't');
	CHECK(view0[1] == 'h');
	CHECK(view0[2] == 'i');
	CHECK(view0[3] == 's');
	CHECK(view0[4] == ' ');
	CHECK(view0[5] == 'i');
	CHECK(view0[view0.size() - 1] == 'g');
	CHECK_THROWS(view0[view0.size()]);
}
