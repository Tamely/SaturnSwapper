cmake_minimum_required(VERSION 3.2)
project(sjson-cpp_unit_tests_gradle_shim NONE)

# Set our project root since our gradle files used to build live in the binary output directory
# but the actual source files live in the source/current directory.
set(SJSON_PROJECT_ROOT ${CMAKE_CURRENT_BINARY_DIR}/app)
file(RELATIVE_PATH SJSON_PROJECT_ROOT ${SJSON_PROJECT_ROOT} ${PROJECT_SOURCE_DIR}/app)

# Configure gradle for our build configuration
configure_file(app/build.gradle.in app/build.gradle @ONLY)

# Copy gradle related files
file(COPY build.gradle gradle.properties settings.gradle gradle DESTINATION .)

add_custom_target(${PROJECT_NAME} ALL
	COMMAND "${PROJECT_SOURCE_DIR}/gradlew.bat"
	# Decide whether we should build Debug or Release
	$<$<CONFIG:Debug>:assembleDebug>
	$<$<CONFIG:Release>:assembleRelease>)
