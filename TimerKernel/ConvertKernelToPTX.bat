call "%VS120COMNTOOLS%\..\..\vc\vcvarsall.bat
rem call "%VS140COMNTOOLS%\..\..\vc\vcvarsall.bat
rem nvcc --ptx --gpu-architecture sm_35 --machine 32 TimerKernel.cu
nvcc --ptx --gpu-architecture sm_35 --machine 64 TimerKernel.cu

del ..\TimerKernel.ptx
copy TimerKernel.ptx ..

pause