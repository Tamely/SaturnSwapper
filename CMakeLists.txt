project(Saturn C CXX)
cmake_minimum_required(VERSION 3.20)

include(cmake/Saturn.cmake)

include_directories("src")
include_directories("lib")
link_directories("lib")
link_libraries(libcurl_a)

add_compile_definitions(CURL_STATICLIB)

if (MSVC)
    add_compile_options("/utf-8")
endif()

file(GLOB_RECURSE saturn_SRC true "src/*.h"
                                  "src/*.hpp"
                                  "src/*.cpp"
                                  "src/*.ixx"
                                  "lib/*.json"
                                  "lib/*.lib"
                                  "lib/*.c"
                                  "lib/*.h"
                                  "lib/*.hpp"
                                  "lib/*.cpp"
                                  "lib/*.ixx"
                                  "saturn.o")

add_app("${saturn_SRC}")
