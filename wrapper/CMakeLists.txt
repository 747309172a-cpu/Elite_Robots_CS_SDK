set(ELITE_C_WRAPPER_TARGET elite_cs_series_sdk_c)

set(
    ELITE_C_WRAPPER_SOURCES
    source/Dashboard/Elite_Dashboard_C.cpp
    source/Elite/Elite_Driver_C.cpp
    source/Elite/Elite_Controllerlog_C.cpp
    source/Elite/Elite_Log_C.cpp
    source/Elite/Elite_Remoteupgrade_C.cpp
    source/Elite/Elite_Serialcommunication_C.cpp
    source/Elite/Elite_Versioninfo_C.cpp
    source/Primary/Elite_Primary_C.cpp
    source/Rtsi/Elite_Rtsi_Client_C.cpp
    source/Rtsi/Elite_Rtsi_Recipe_C.cpp
    source/Rtsi/Elite_Rtsi_Io_C.cpp
)

add_library(${ELITE_C_WRAPPER_TARGET} SHARED ${ELITE_C_WRAPPER_SOURCES})
target_compile_features(${ELITE_C_WRAPPER_TARGET} PRIVATE cxx_std_17)

if(CMAKE_SYSTEM_NAME STREQUAL "Windows")
    target_compile_definitions(${ELITE_C_WRAPPER_TARGET} PRIVATE ELITE_C_EXPORT_LIBRARY)
endif()

target_link_libraries(${ELITE_C_WRAPPER_TARGET} PRIVATE elite-cs-series-sdk::shared)

target_include_directories(
    ${ELITE_C_WRAPPER_TARGET}
    PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
        $<INSTALL_INTERFACE:include>
)

if(CMAKE_SYSTEM_NAME STREQUAL "Linux" OR APPLE)
    set_target_properties(${ELITE_C_WRAPPER_TARGET} PROPERTIES OUTPUT_NAME "elite_cs_series_sdk_c")
endif()

if(ELITE_INSTALL)
    install(TARGETS ${ELITE_C_WRAPPER_TARGET} LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR})
    install(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}/include/ DESTINATION ${CMAKE_INSTALL_INCLUDEDIR})
endif()
