// PTX Instruction Generator and Benchmark Utility 
// This projected is licensed under the terms of the MIT license.
// NO WARRANTY. THE SOFTWARE IS PROVIDED TO YOU “AS IS” AND “WITH ALL FAULTS.”
// ANY USE OF THE SOFTWARE IS ENTIRELY AT YOUR OWN RISK.
// Created by Ryan S. White in 2009; Updated in 2011; Updated in 2016
// Note: This code was not written for public consumption so please excuse the sloppiness.

using System;
using System.IO;
using System.Text.RegularExpressions;
using ManagedCuda;
using System.Text;

namespace InstNS
{
    class InstTimerAndTester
    {
        string kernelTop, kernelMid, kernelMid2, kernelBot;

        public InstTimerAndTester()
        {
            // Load the Top and Bottom and Kernel
            string kernel = File.ReadAllText(@"TimerKernel.ptx");
            string[] kernelParts = Regex.Split(kernel, "mov.s32 r6, r6;");
            kernelTop = kernelParts[0];
            kernelMid = kernelParts[1];
            kernelMid2 = kernelParts[2];
            kernelBot = kernelParts[3];

            //System.Diagnostics.Process p = new System.Diagnostics.Process();
            //p.StartInfo.FileName = "ptxas.exe";
            //p.StartInfo.Arguments = @"""" + Environment.GetEnvironmentVariable("TEMP") + @"\kernelTimer.ptx""  -o """ + Environment.GetEnvironmentVariable("TEMP") + @"\kernelTimer.cubin""";
            //p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardOutput = true;
        }

        public int Benchmark(string header, string inst, string tail)
        {
            string ptxcode = kernelTop + header + kernelMid + inst + kernelMid2 + tail + kernelBot;
            byte[] ptx= Encoding.ASCII.GetBytes(ptxcode);
            int[] timer = new int[202]; //2 for timer + 200 for junk

            CudaContext ctx = new CudaContext();
            try
            {
                ManagedCuda.BasicTypes.CUmodule cumodule = ctx.LoadModulePTX(ptx);
                CudaKernel cuKernel = new CudaKernel("timedFunc", cumodule, ctx);
                CudaDeviceVariable<int> dtimer = new CudaDeviceVariable<int>(202);

                cuKernel.GridDimensions = new ManagedCuda.VectorTypes.dim3(1, 1, 1);
                cuKernel.BlockDimensions = new ManagedCuda.VectorTypes.dim3(1, 1, 1);
                cuKernel.Run(dtimer.DevicePointer);

                dtimer.CopyToHost(timer);

                // ManagedCuda Cleanup
                dtimer.Dispose();
                CudaContext.ProfilerStop();
                ctx.Dispose();
            }
            catch (Exception x)
            {
                Console.WriteLine("Error running the PTX code: \r\n"
                    + ptxcode + "\r\n Error Message: " + x.Message);
                //throw;
            }

            return timer[1] - timer[0];
        }
    }
}