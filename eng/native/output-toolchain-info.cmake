# Output the toolchain information required to create a command line that builds with the right rootfs as XML

set (ADDED_COMPILE_OPTIONS)
if (CMAKE_SCRIPT_MODE_FILE)
  # add_compile_options and add_definitions can't be used in scripts,
  # so override the implementations to append to a local property
  macro(add_compile_options)
    list(APPEND ADDED_COMPILE_OPTIONS ${ARGV})
  endmacro()
  macro(add_definitions)
    list(APPEND ADDED_COMPILE_OPTIONS ${ARGV})
  endmacro()
endif()

include(${CMAKE_CURRENT_LIST_DIR}/../common/cross/toolchain.cmake)

message("<toolchain-info>")
message("<target-triple>${TOOLCHAIN}</target-triple>")
message("<linker-args>${CMAKE_SHARED_LINKER_FLAGS_INIT}</linker-args>")
message("<compiler-args>${ADDED_COMPILE_OPTIONS}</compiler-args>")
message("</toolchain-info>")