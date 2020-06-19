# --------------------------------------------------------------------------
#                   OpenMS -- Open-Source Mass Spectrometry
# --------------------------------------------------------------------------
# Copyright The OpenMS Team -- Eberhard Karls University Tuebingen,
# ETH Zurich, and Freie Universitaet Berlin 2002-2018.
#
# This software is released under a three-clause BSD license:
#  * Redistributions of source code must retain the above copyright
#    notice, this list of conditions and the following disclaimer.
#  * Redistributions in binary form must reproduce the above copyright
#    notice, this list of conditions and the following disclaimer in the
#    documentation and/or other materials provided with the distribution.
#  * Neither the name of any author or any participating institution
#    may be used to endorse or promote products derived from this software
#    without specific prior written permission.
# For a full list of authors, refer to the file AUTHORS.
# --------------------------------------------------------------------------
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
# AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
# IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
# ARE DISCLAIMED. IN NO EVENT SHALL ANY OF THE AUTHORS OR THE CONTRIBUTING
# INSTITUTIONS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
# EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
# PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
# OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
# WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
# OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
# ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#
# --------------------------------------------------------------------------
# $Maintainer: Julianus Pfeuffer $
# $Authors: Julianus Pfeuffer $
# --------------------------------------------------------------------------

### CMake OpenMS config file for external code
### configured by the OpenMS build system from <OpenMS>/cmake/OpenMSConfig.cmake.in

# we need this to reference the target file
get_filename_component(OPENMS_CMAKE_DIR "${CMAKE_CURRENT_LIST_FILE}" PATH)

# include directories for targets
set(OpenMS_GUI_INCLUDE_DIRECTORIES "C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include/sqlite;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include/eigen3;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include/WildMagic;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/dev/Qt/5.10.1/msvc2015_64/include/;C:/dev/Qt/5.10.1/msvc2015_64/include/QtCore;C:/dev/Qt/5.10.1/msvc2015_64/.//mkspecs/win32-msvc;C:/dev/Qt/5.10.1/msvc2015_64/include/;C:/dev/Qt/5.10.1/msvc2015_64/include/QtNetwork;C:/dev/Qt/5.10.1/msvc2015_64/include/QtCore;C:/dev/Qt/5.10.1/msvc2015_64/.//mkspecs/win32-msvc;C:/dev/Qt/5.10.1/msvc2015_64/include/;C:/dev/Qt/5.10.1/msvc2015_64/include/QtWidgets;C:/dev/Qt/5.10.1/msvc2015_64/include/QtGui;C:/dev/Qt/5.10.1/msvc2015_64/include/QtCore;C:/dev/Qt/5.10.1/msvc2015_64/.//mkspecs/win32-msvc;C:/dev/Qt/5.10.1/msvc2015_64/include//QtANGLE;C:/dev/Qt/5.10.1/msvc2015_64/include/;C:/dev/Qt/5.10.1/msvc2015_64/include/QtSvg;C:/dev/Qt/5.10.1/msvc2015_64/include/QtWidgets;C:/dev/Qt/5.10.1/msvc2015_64/include/QtGui;C:/dev/Qt/5.10.1/msvc2015_64/include/QtCore;C:/dev/Qt/5.10.1/msvc2015_64/.//mkspecs/win32-msvc;C:/dev/Qt/5.10.1/msvc2015_64/include//QtANGLE;C:/dev/Qt/5.10.1/msvc2015_64/include/;C:/dev/Qt/5.10.1/msvc2015_64/include/QtNetwork;C:/dev/Qt/5.10.1/msvc2015_64/include/QtCore;C:/dev/Qt/5.10.1/msvc2015_64/.//mkspecs/win32-msvc;include")

set(SuperHirn_INCLUDE_DIRECTORIES "C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include/sqlite;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include/eigen3;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include/WildMagic;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/dev/Qt/5.10.1/msvc2015_64/include/;C:/dev/Qt/5.10.1/msvc2015_64/include/QtCore;C:/dev/Qt/5.10.1/msvc2015_64/.//mkspecs/win32-msvc;C:/dev/Qt/5.10.1/msvc2015_64/include/;C:/dev/Qt/5.10.1/msvc2015_64/include/QtNetwork;C:/dev/Qt/5.10.1/msvc2015_64/include/QtCore;C:/dev/Qt/5.10.1/msvc2015_64/.//mkspecs/win32-msvc;include")

set(OpenMS_INCLUDE_DIRECTORIES "C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include/sqlite;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include/eigen3;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include/WildMagic;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;C:/dev/Qt/5.10.1/msvc2015_64/include/;C:/dev/Qt/5.10.1/msvc2015_64/include/QtCore;C:/dev/Qt/5.10.1/msvc2015_64/.//mkspecs/win32-msvc;C:/dev/Qt/5.10.1/msvc2015_64/include/;C:/dev/Qt/5.10.1/msvc2015_64/include/QtNetwork;C:/dev/Qt/5.10.1/msvc2015_64/include/QtCore;C:/dev/Qt/5.10.1/msvc2015_64/.//mkspecs/win32-msvc;include")

set(OpenSwathAlgo_INCLUDE_DIRECTORIES "C:/jenkins/ws/openms_user_experimental/openms_test_packaging/c2e226b2/contrib_build/include;include")



set(OPENMS_ADDCXX_FLAGS " /wd4251 /wd4275 /wd4996 /wd4661 /wd4503 /wd4068")

## The targets file
include("${OPENMS_CMAKE_DIR}/OpenMSTargets.cmake")
