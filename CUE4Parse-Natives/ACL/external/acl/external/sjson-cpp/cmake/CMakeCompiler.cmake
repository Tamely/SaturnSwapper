cmake_minimum_required (VERSION 3.2)

macro(setup_default_compiler_flags _project_name)
	if(MSVC)
		# Replace some default compiler switches and add new ones
		STRING(REPLACE "/GR" "" CMAKE_CXX_FLAGS ${CMAKE_CXX_FLAGS})			# Disable RTTI
		if(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
			STRING(REPLACE "/W3" "/W4" CMAKE_CXX_FLAGS ${CMAKE_CXX_FLAGS})			# Enable level 4 warnings
		else()
			if(MSVC_VERSION GREATER 1920)
				# VS2019 and above
				STRING(REPLACE "/W3" "/Wall" CMAKE_CXX_FLAGS ${CMAKE_CXX_FLAGS})	# Enable all warnings
			else()
				STRING(REPLACE "/W3" "/W4" CMAKE_CXX_FLAGS ${CMAKE_CXX_FLAGS})		# Enable level 4 warnings
			endif()
		endif()
		target_compile_options(${_project_name} PRIVATE /Zi)				# Add debug info
		target_compile_options(${_project_name} PRIVATE /Oi)				# Generate intrinsic functions
		target_compile_options(${_project_name} PRIVATE /WX)				# Treat warnings as errors
		target_compile_options(${_project_name} PRIVATE /MP)				# Enable parallel compilation

		if(MSVC_VERSION GREATER 1900)
			# VS2017 and above
			target_compile_options(${_project_name} PRIVATE /permissive-)
		endif()

		# Disable various warnings that are harmless
		target_compile_options(${_project_name} PRIVATE /wd4514)			# Unreferenced inline function removed
		target_compile_options(${_project_name} PRIVATE /wd4619)			# No warning with specified number
		target_compile_options(${_project_name} PRIVATE /wd4820)			# Padding added after data member
		target_compile_options(${_project_name} PRIVATE /wd4710)			# Function not inlined
		target_compile_options(${_project_name} PRIVATE /wd4711)			# Function selected for automatic inlining
		target_compile_options(${_project_name} PRIVATE /wd4738)			# Storing 32-bit float in memory leads to rounding (x86)
		target_compile_options(${_project_name} PRIVATE /wd5045)			# Spectre mitigation for memory load

		if(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
			target_compile_options(${_project_name} PRIVATE -Wno-c++98-compat)				# No need to support C++98
			target_compile_options(${_project_name} PRIVATE -Wno-c++98-compat-pedantic)		# No need to support C++98
		endif()

		# Add linker flags
		set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} /DEBUG")
	else()
		# TODO: Handle OS X properly: https://stackoverflow.com/questions/5334095/cmake-multiarchitecture-compilation
		if(CPU_INSTRUCTION_SET MATCHES "x86")
			target_compile_options(${_project_name} PRIVATE "-m32")
			target_link_libraries(${_project_name} PRIVATE "-m32")
		elseif(CPU_INSTRUCTION_SET MATCHES "x64")
			target_compile_options(${_project_name} PRIVATE "-m64")
			target_link_libraries(${_project_name} PRIVATE "-m64")
		endif()

		target_compile_options(${_project_name} PRIVATE -Wall -Wextra)		# Enable all warnings
		target_compile_options(${_project_name} PRIVATE -Wshadow)			# Enable shadowing warnings
		target_compile_options(${_project_name} PRIVATE -Werror)			# Treat warnings as errors

		# Disable various warnings that are harmless
		if(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
			target_compile_options(${_project_name} PRIVATE -Wno-c++98-compat)	# No need to support C++98
		endif()

		target_compile_options(${_project_name} PRIVATE -g)					# Enable debug symbols
	endif()
endmacro()
