cmake_minimum_required(VERSION 2.8)

project(split700 C CXX)
set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(MSVC)
    add_definitions(-D_CRT_SECURE_NO_WARNINGS)

    option(STATIC_CRT "Use static CRT libraries" OFF)

    # Rewrite command line flags to use /MT if necessary
    if(STATIC_CRT)
        foreach(flag_var
                CMAKE_CXX_FLAGS CMAKE_CXX_FLAGS_DEBUG CMAKE_CXX_FLAGS_RELEASE
                CMAKE_CXX_FLAGS_MINSIZEREL CMAKE_CXX_FLAGS_RELWITHDEBINFO)
            if(${flag_var} MATCHES "/MD")
                string(REGEX REPLACE "/MD" "/MT" ${flag_var} "${${flag_var}}")
            endif(${flag_var} MATCHES "/MD")
        endforeach(flag_var)
    endif()

    # Allow for wildcards in command-line path arguments
    # http://msdn.microsoft.com/en-us/library/8bch7bkk.aspx
    set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} setargv.obj")
endif()


#============================================================================
# split700
#============================================================================

set(SPLIT700_HDRS
    src/SPCFile.h
    src/SPCSampDir.h
    src/WavWriter.h
    src/cpath.h
    src/split700.h
)
set(SPLIT700_SRCS
    src/SPCFile.cpp
    src/SPCSampDir.cpp
    src/WavWriter.cpp
    src/split700.cpp
)

add_executable(split700 ${SPLIT700_SRCS} ${SPLIT700_HDRS})

#============================================================================
# brr2wav
#============================================================================

set(BRR2WAV_HDRS
    src/SPCFile.h
    src/SPCSampDir.h
    src/WavWriter.h
    src/cpath.h
)
set(BRR2WAV_SRCS
    src/SPCFile.cpp
    src/SPCSampDir.cpp
    src/WavWriter.cpp
    src/brr2wav.cpp
)

add_executable(brr2wav ${BRR2WAV_SRCS} ${BRR2WAV_HDRS})
