#----------------------------------------------------------------
# Generated CMake target import file for configuration "Release".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "OpenSwathAlgo" for configuration "Release"
set_property(TARGET OpenSwathAlgo APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(OpenSwathAlgo PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/bin/OpenSwathAlgo.lib"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/OpenSwathAlgo.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS OpenSwathAlgo )
list(APPEND _IMPORT_CHECK_FILES_FOR_OpenSwathAlgo "${_IMPORT_PREFIX}/bin/OpenSwathAlgo.lib" "${_IMPORT_PREFIX}/bin/OpenSwathAlgo.dll" )

# Import target "OpenMS" for configuration "Release"
set_property(TARGET OpenMS APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(OpenMS PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/bin/OpenMS.lib"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/OpenMS.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS OpenMS )
list(APPEND _IMPORT_CHECK_FILES_FOR_OpenMS "${_IMPORT_PREFIX}/bin/OpenMS.lib" "${_IMPORT_PREFIX}/bin/OpenMS.dll" )

# Import target "SuperHirn" for configuration "Release"
set_property(TARGET SuperHirn APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(SuperHirn PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/bin/SuperHirn.lib"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/SuperHirn.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS SuperHirn )
list(APPEND _IMPORT_CHECK_FILES_FOR_SuperHirn "${_IMPORT_PREFIX}/bin/SuperHirn.lib" "${_IMPORT_PREFIX}/bin/SuperHirn.dll" )

# Import target "OpenMS_GUI" for configuration "Release"
set_property(TARGET OpenMS_GUI APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(OpenMS_GUI PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/bin/OpenMS_GUI.lib"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/OpenMS_GUI.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS OpenMS_GUI )
list(APPEND _IMPORT_CHECK_FILES_FOR_OpenMS_GUI "${_IMPORT_PREFIX}/bin/OpenMS_GUI.lib" "${_IMPORT_PREFIX}/bin/OpenMS_GUI.dll" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
