set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
include(${CMAKE_ROOT}/Modules/ExternalProject.cmake)

set(SDK_ROOT "${CMAKE_BINARY_DIR}/SDK/")
set(ULTRALIGHT_INCLUDE_DIR "${SDK_ROOT}/include")
set(ULTRALIGHT_BINARY_DIR "${SDK_ROOT}/bin")
set(ULTRALIGHT_RESOURCES_DIR "${SDK_ROOT}/resources")
set(ULTRALIGHT_INSPECTOR_DIR "${SDK_ROOT}/inspector")

if (UNIX)
  if (APPLE)
    set(PORT UltralightMac)
    set(PLATFORM "mac")
    set(ULTRALIGHT_LIBRARY_DIR "${SDK_ROOT}/bin")
  else ()
    set(PORT UltralightLinux)
    set(PLATFORM "linux")
    set(ULTRALIGHT_LIBRARY_DIR "${SDK_ROOT}/bin")
  endif ()
elseif (CMAKE_SYSTEM_NAME MATCHES "Windows")
    set(PORT UltralightWin)
    set(PLATFORM "win")
    set(ULTRALIGHT_LIBRARY_DIR "${SDK_ROOT}/lib")
else ()
  message(FATAL_ERROR "Unknown OS '${CMAKE_SYSTEM_NAME}'")
endif ()

set(ARCHITECTURE "x64")
set(VERSION "1.3.0")

set(BASE_URL "github.com/ultralight-ux/Ultralight/releases/download")

ExternalProject_Add(UltralightSDK
  URL https://${BASE_URL}/v${VERSION}/ultralight-sdk-${VERSION}-${PLATFORM}-${ARCHITECTURE}.7z
  SOURCE_DIR "${SDK_ROOT}"
  BUILD_IN_SOURCE 1
  CONFIGURE_COMMAND ""
  BUILD_COMMAND ""
  INSTALL_COMMAND ""
)

MACRO(ADD_APP source_list)
  set(APP_NAME ${CMAKE_PROJECT_NAME})

  include_directories("${ULTRALIGHT_INCLUDE_DIR}")
  link_directories("${ULTRALIGHT_LIBRARY_DIR}")
  link_libraries(UltralightCore AppCore Ultralight WebCore)

  if (PORT MATCHES "UltralightLinux")
    SET(CMAKE_SKIP_BUILD_RPATH  FALSE)
    SET(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
    SET(CMAKE_INSTALL_RPATH "$\{ORIGIN\}")
  endif ()

  if (PORT MATCHES "UltralightMac")
    SET(CMAKE_SKIP_BUILD_RPATH  FALSE)
    SET(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
    SET(CMAKE_INSTALL_RPATH "@executable_path/")
  endif ()

  add_executable(${APP_NAME} WIN32 MACOSX_BUNDLE ${source_list})

  if (APPLE)
    # Enable High-DPI on macOS through our custom Info.plist template
    set_target_properties(${APP_NAME} PROPERTIES MACOSX_BUNDLE_INFO_PLIST ${CMAKE_CURRENT_SOURCE_DIR}/cmake/Info.plist.in) 
  endif()

  if (MSVC)
    # Tell MSVC to use main instead of WinMain for Windows subsystem executables
    set_target_properties(${APP_NAME} PROPERTIES LINK_FLAGS "/ENTRY:mainCRTStartup")
  endif()

  # Copy all binaries to target directory
  add_custom_command(TARGET ${APP_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory "${ULTRALIGHT_BINARY_DIR}" $<TARGET_FILE_DIR:${APP_NAME}>) 

  # Set the assets path to "/assets" or "/../Resources/assets" on macOS
  if (APPLE)
    set(ASSETS_PATH "$<TARGET_FILE_DIR:${APP_NAME}>/../Resources/assets") 
  else ()
    set(ASSETS_PATH "$<TARGET_FILE_DIR:${APP_NAME}>/assets") 
  endif ()

  # Copy assets to assets path
  add_custom_command(TARGET ${APP_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/assets/" "${ASSETS_PATH}")

  if(${ENABLE_INSPECTOR})
    # Copy inspector to assets directory
    add_custom_command(TARGET ${APP_NAME} POST_BUILD
      COMMAND ${CMAKE_COMMAND} -E copy_directory "${ULTRALIGHT_INSPECTOR_DIR}" "${ASSETS_PATH}/inspector")
  endif ()

  # Copy resources to assets directory
  add_custom_command(TARGET ${APP_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory "${ULTRALIGHT_RESOURCES_DIR}" "${ASSETS_PATH}/resources")
    
  add_dependencies(${APP_NAME} UltralightSDK)
ENDMACRO()