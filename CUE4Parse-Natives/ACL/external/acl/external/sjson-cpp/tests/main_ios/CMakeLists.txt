cmake_minimum_required (VERSION 3.2)
project(sjson-cpp_unit_tests)

# iOS cmake toolchain does not support CMAKE_CXX_STANDARD
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++${CMAKE_CXX_STANDARD}")

# Force enable debug symbols
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -g")

# Enable optimizations in Release
set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -O3")

set(MACOSX_BUNDLE_EXECUTABLE_NAME ${PROJECT_NAME})
set(MACOSX_BUNDLE_INFO_STRING "com.sjson.cpp.unit-tests")
set(MACOSX_BUNDLE_GUI_IDENTIFIER "com.sjson.cpp.unit-tests")
set(MACOSX_BUNDLE_BUNDLE_NAME "sjson-cpp-unit-tests")

include_directories("${PROJECT_SOURCE_DIR}/../../includes")
include_directories("${PROJECT_SOURCE_DIR}/../../external/catch2/single_include")
include_directories("${PROJECT_SOURCE_DIR}/../sources")

# Grab all of our test source files
file(GLOB_RECURSE ALL_TEST_SOURCE_FILES LIST_DIRECTORIES false
	${PROJECT_SOURCE_DIR}/../sources/*.h
	${PROJECT_SOURCE_DIR}/../sources/*.cpp)

create_source_groups("${ALL_TEST_SOURCE_FILES}" ${PROJECT_SOURCE_DIR}/..)

# Grab all of our main source files
file(GLOB_RECURSE ALL_MAIN_SOURCE_FILES LIST_DIRECTORIES false
	${PROJECT_SOURCE_DIR}/*.cpp)

create_source_groups("${ALL_MAIN_SOURCE_FILES}" ${PROJECT_SOURCE_DIR})

add_executable(${PROJECT_NAME} MACOSX_BUNDLE ${ALL_TEST_SOURCE_FILES} ${ALL_MAIN_SOURCE_FILES})

# Throw on failure to allow us to catch them and recover
add_definitions(-DSJSON_CPP_ON_ASSERT_THROW)
