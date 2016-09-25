// PTX Instruction Generator and Timer
// This projected is licensed under the terms of the MIT license.
// NO WARRANTY. THE SOFTWARE IS PROVIDED TO YOU “AS IS” AND “WITH ALL FAULTS.”
// ANY USE OF THE SOFTWARE IS ENTIRELY AT YOUR OWN RISK.
// Created by Ryan S. White in 2009; Updated in 2011
// Note: This code was written for myself and was never originally intended to be shared
//       so please excuse the sloppiness.          

using System;
using GASS.CUDA;
using GASS.CUDA.Types;

namespace InstNS
{
    class InstTimerAndTester
    {
    //////////// The kernel that generated the PTX below.. //////////////////
    //#ifndef _CLOCK_KERNEL_H_
    //#define _CLOCK_KERNEL_H_
    //extern "C" __global__ void timedReduction(clock_t * timer)
    //{
	
    //    int s32 = threadIdx.x;
    //    short s16 = threadIdx.x;
    //    unsigned int u32 = threadIdx.x;
    //    unsigned short u16 = threadIdx.x;
    //    //float16 f16=1; ???
    //    float f32 = threadIdx.x;
	
    //    __syncthreads();
    //    timer[0] = clock();
    //    //do stuff here

    //    timer[1] = clock();
    //    timer[1]=timer[1]-90;
    //}
    //#endif // _CLOCK_KERNEL_H_


const string top = @"
	.version 2.2
	.target sm_20	
	.reg .u32 startTime;
	.reg .u32 endTime;
	.reg .u32 _filler;
	.reg .u32 parmOffset;

	.func (.reg .f32 %fv1) _Z8CallInsthff (.reg .u32 %ra1, .reg .f32 %fa2, .reg .f32 %fa3)
	{
	.reg .u32 %r<126>;
	.reg .f32 %f<75>;
	.reg .pred %p<67>;

	cvt.u8.u32 	%r1, %ra1;
	mov.f32 	%f1, %fa2;
	mov.f32 	%f2, %fa3;


    setp.lo.u32 	%p1, %r1, 20;  //less then
	@%p1 bra 	_skipb;


    setp.hs.u32 	%p1, %r1, 28;
	@%p1 bra 	_skip28;
    setp.hs.u32 	%p1, %r1, 24;
	@%p1 bra 	_skip24;
    setp.hs.u32 	%p1, %r1, 20;
	@%p1 bra 	_skip20;
_skipb:
    setp.hs.u32 	%p1, %r1, 16;
    @%p1 bra 	_skip16;
    setp.hs.u32 	%p1, %r1, 12;
	@%p1 bra 	_skip12;
    setp.hs.u32 	%p1, %r1, 8;
	@%p1 bra 	_skip8;
    setp.hs.u32 	%p1, %r1, 4;
	@%p1 bra 	_skip4;

    mov.u32 	%r2, 0;
	setp.eq.s32 	%p1, %r1, %r2;
	@%p1 add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r3, 1;
	setp.eq.s32 	%p2, %r1, %r3;
	@%p2  sub.f32 	%fv1, %f1, %f2;
	mov.u32 	%r4, 2;
	setp.eq.s32 	%p3, %r1, %r4;
	@%p3  mul.f32 	%fv1, %f1, %f2;
	mov.u32 	%r5, 3;
	setp.eq.s32 	%p4, %r1, %r5;
	@%p4  div.approx.f32 	%fv1, %f1, %f2;
	bra.uni 	$LBB115__Z8CallInsthff; 
_skip4:
	mov.u32 	%r6, 4;
	setp.eq.s32 	%p5, %r1, %r6;
	@%p5  min.f32 	%fv1, %f1, %f2;
	mov.u32 	%r7, 5;
	setp.eq.s32 	%p6, %r1, %r7;
	@%p6  max.f32 	%fv1, %f1, %f2;
	mov.u32 	%r8, 6;
	setp.eq.s32 	%p7, %r1, %r8;
	@%p7  add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r9, 7;
	setp.eq.s32 	%p8, %r1, %r9;
	@%p8  add.f32 	%fv1, %f1, %f2;
	bra.uni 	$LBB115__Z8CallInsthff; 
_skip8:
    mov.u32 	%r10, 8;
	setp.eq.s32 	%p9, %r1, %r10;
	@%p9 max.f32 	%fv1, %f1, %f2;
	mov.u32 	%r11, 9;
	setp.eq.s32 	%p10, %r1, %r11;
	@%p10 min.f32 	%fv1, %f1, %f2;
	mov.u32 	%r12, 10;
	setp.eq.s32 	%p11, %r1, %r12;
	@%p11 sub.f32 	%fv1, %f1, %f2;
	mov.u32 	%r13, 11;
	setp.eq.s32 	%p12, %r1, %r13;
	@%p12 add.f32 	%fv1, %f1, %f2;
	bra.uni 	$LBB115__Z8CallInsthff; 
_skip12:
	mov.u32 	%r14, 12;
	setp.eq.s32 	%p13, %r1, %r14;
	@%p13 add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r15, 13;
	setp.eq.s32 	%p14, %r1, %r15;
	@%p14 add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r16, 14;
	setp.eq.s32 	%p15, %r1, %r16;
	@%p15 add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r17, 15;
	setp.eq.s32 	%p16, %r1, %r17;
	@%p16 add.f32 	%fv1, %f1, %f2;
	bra.uni 	$LBB115__Z8CallInsthff; 
_skip16:
	mov.u32 	%r18, 16;
	setp.eq.s32 	%p17, %r1, %r18;
	@%p17  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r19, 17;
	setp.eq.s32 	%p18, %r1, %r19;
	@%p18  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r20, 18;
	setp.eq.s32 	%p19, %r1, %r20;
	@%p19  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r21, 19;
	setp.eq.s32 	%p20, %r1, %r21;
	@%p20  	add.f32 	%fv1, %f1, %f2;
	bra.uni 	$LBB115__Z8CallInsthff; 
_skip20:
	mov.u32 	%r22, 20;
	setp.eq.s32 	%p21, %r1, %r22;
	@%p21  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r23, 21;
	setp.eq.s32 	%p22, %r1, %r23;
	@%p22  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r24, 22;
	setp.eq.s32 	%p23, %r1, %r24;
	@%p23  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r25, 23;
	setp.eq.s32 	%p24, %r1, %r25;
	@%p24  	add.f32 	%fv1, %f1, %f2;
	bra.uni 	$LBB115__Z8CallInsthff; 
_skip24:
    mov.u32 	%r26, 24;
	setp.eq.s32 	%p25, %r1, %r26;
	@%p25  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r27, 25;
	setp.eq.s32 	%p26, %r1, %r27;
	@%p26  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r28, 26;
	setp.eq.s32 	%p27, %r1, %r28;
	@%p27  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r29, 27;
	setp.eq.s32 	%p28, %r1, %r29;
	@%p28  	add.f32 	%fv1, %f1, %f2;
	bra.uni 	$LBB115__Z8CallInsthff; 
_skip28:
	mov.u32 	%r30, 28;
	setp.eq.s32 	%p29, %r1, %r30;
	@%p29  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r31, 29;
	setp.eq.s32 	%p30, %r1, %r31;
	@%p30  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r32, 30;
	setp.eq.s32 	%p31, %r1, %r32;
	@%p31  	add.f32 	%fv1, %f1, %f2;
	mov.u32 	%r33, 31;
	setp.eq.s32 	%p32, %r1, %r33;
	@%p32  	add.f32 	%fv1, %f1, %f2;
$LBB115__Z8CallInsthff:
	ret; // inserted: 
	} // _Z8CallInsthff


	.entry timedReduction
	{
	.reg .u32 %r<126>;
	.reg .f32 %f<75>;
	.reg .pred %p<67>;
	.reg .f32 %fv<3>;
.param .u32 __parm_timer;
	.reg .u8   _su8;	//.reg .v2 .u8   _su8v2;	.reg .v4 .u8   _su8v4;
	.reg .s8   _ss8;	//.reg .v2 .s8   _ss8v2;	.reg .v4 .s8   _ss8v4;
	.reg .b8   _sb8;	//.reg .v2 .b8   _sb8v2;	.reg .v4 .b8   _sb8v4;
	.reg .u16 _su16;	//.reg .v2 .u16 _su16v2;	.reg .v4 .u16 _su16v4;
	.reg .s16 _ss16;	//.reg .v2 .s16 _ss16v2;	.reg .v4 .s16 _ss16v4;
	.reg .b16 _sb16;	//.reg .v2 .b16 _sb16v2;	.reg .v4 .b16 _sb16v4;
	.reg .u32 _su32; 	//.reg .v2 .u32 _su32v2;	.reg .v4 .u32 _su32v4;
	.reg .s32 _ss32;	//.reg .v2 .s32 _ss32v2;	.reg .v4 .s32 _ss32v4;
	.reg .b32 _sb32;	//.reg .v2 .b32 _sb32v2;	.reg .v4 .b32 _sb32v4;
	.reg .f16 _sf16;	//.reg .v2 .f16 _sf16v2;	.reg .v4 .f16 _sf16v4;
	.reg .f32 _sf32;	//.reg .v2 .f32 _sf32v2;	.reg .v4 .f32 _sf32v4;
	                    //
	.reg .u8   _du8;	//.reg .v2 .u8   _du8v2;	.reg .v4 .u8   _du8v4;
	.reg .s8   _ds8;	//.reg .v2 .s8   _ds8v2;	.reg .v4 .s8   _ds8v4;
	.reg .b8   _db8;	//.reg .v2 .b8   _db8v2;	.reg .v4 .b8   _db8v4;
	.reg .u16 _du16;	//.reg .v2 .u16 _du16v2;	.reg .v4 .u16 _du16v4;
	.reg .s16 _ds16;	//.reg .v2 .s16 _ds16v2;	.reg .v4 .s16 _ds16v4;
	.reg .b16 _db16;	//.reg .v2 .b16 _db16v2;	.reg .v4 .b16 _db16v4;
	.reg .u32 _du32;	//.reg .v2 .u32 _du32v2;	.reg .v4 .u32 _du32v4;
	.reg .s32 _ds32;	//.reg .v2 .s32 _ds32v2;	.reg .v4 .s32 _ds32v4;
	.reg .b32 _db32;	//.reg .v2 .b32 _db32v2;	.reg .v4 .b32 _db32v4;
	.reg .f16 _df16;	//.reg .v2 .f16 _df16v2;	.reg .v4 .f16 _df16v4;
	.reg .f32 _df32;	//.reg .v2 .f32 _df32v2;	.reg .v4 .f32 _df32v4;
	
	.reg .pred _sPdt;
	.reg .pred _dPdt;
	.reg .pred P5; //temp


	
	mov.u32 _su32 , %clock;
    shr.u32 _su32,_su32,1;
	cvt.rn.f32.u32  _sf32 , _su32;
	cvt.u8.u32    _su8, _su32;
	cvt.s8.u32    _ss8, _su32;
	cvt.u16.u32  _su16 , _su32;
	cvt.s16.u32  _ss16 , _su32;
	cvt.u32.u32  _su32 , _su32;
	cvt.s32.u32  _ss32 , _su32;
	cvt.rn.f16.u32  _sf16 , _su32;
	and.b16 _sb16, _su16, 0x1234;//_sb8 only load and set uses _sb8
	and.b32 _sb32, _su32, 0x12345678;

	
	add.u32 _filler,_filler,_filler; //some junk - avoids a read after write 
	add.u32 _filler,_filler,_filler; //some junk - avoids a read after write
	
    and.b32 _su32, _su32, 0x1F;
	//cvt.u8.u32 	%r1, _su32;
    mov.u32 %r1, _su32;
	mov.f32 	%f1, _sf32;
	mov.f32 	%f2, _sf32;


    mov.s32 	startTime, %clock;
	add.u32 _filler,_filler,_filler; //some junk - avoids a read after write
	//time the stuff from here
";const string bot = @"
	//end here
	add.u32 _filler,_filler,_filler; //some junk - avoids a read after write
	mov.s32 	endTime, %clock;

	
	
	ld.param.u32 	parmOffset, [__parm_timer];

	st.global.s32 	[parmOffset+0], startTime;
	sub.u32	endTime, endTime,52; //subtracted cost if there is no test instruction 
	st.global.s32 	[parmOffset+4], endTime; 
	
    st.global.u8  	[parmOffset+12], _du8 ; //du8 
    st.global.s8  	[parmOffset+16], _ds8 ;	//ds8 
    st.global.b8  	[parmOffset+20], _db8 ;	//db8 
    st.global.u16 	[parmOffset+24],_du16 ;	//du16
    st.global.s16 	[parmOffset+28],_ds16 ;	//ds16
    st.global.b16 	[parmOffset+32],_db16 ;	//db16
    st.global.u32 	[parmOffset+36],_du32 ;	//du32
    st.global.s32 	[parmOffset+40],_ds32 ;	//ds32
    st.global.b32 	[parmOffset+44],_db32 ;	//db32
    @_dPdt  st.global.f32 	[parmOffset+48],_df32 ; //df32

//    st.global.v2.u8  	[parmOffset+52], _du8v2 ;  //du8v2 
//    st.global.v2.s8  	[parmOffset+56], _ds8v2 ;  //ds8v2 
//    st.global.v2.b8  	[parmOffset+60], _db8v2 ;  //db8v2 
//    st.global.v2.u16 	[parmOffset+64],_du16v2 ;  //du16v2
//    st.global.v2.s16 	[parmOffset+68],_ds16v2 ;  //ds16v2
//    st.global.v2.b16 	[parmOffset+72],_db16v2 ;  //db16v2
//    st.global.v2.u32 	[parmOffset+76],_du32v2 ;  //du32v2
//    st.global.v2.s32 	[parmOffset+80],_ds32v2 ;  //ds32v2
//    st.global.v2.b32 	[parmOffset+84],_db32v2 ;  //db32v2
//    st.global.v2.f32 	[parmOffset+88],_df32v2 ;  //df32v2
//    											  
//    st.global.v4.u8  	[parmOffset+92],  _du8v4 ;//du8v4 
//    st.global.v4.s8  	[parmOffset+96],  _ds8v4 ;//ds8v4 
//    st.global.v4.b8  	[parmOffset+100], _db8v4 ;//db8v4 
//    st.global.v4.u16 	[parmOffset+104],_du16v4 ;//du16v4
//    st.global.v4.s16 	[parmOffset+108],_ds16v4 ;//ds16v4
//    st.global.v4.b16 	[parmOffset+112],_db16v4 ;//db16v4
//    st.global.v4.u32 	[parmOffset+116],_du32v4 ;//du32v4
//    st.global.v4.s32 	[parmOffset+120],_ds32v4 ;//ds32v4
//    st.global.v4.b32 	[parmOffset+124],_db32v4 ;//db32v4
//    @_dPdt st.global.v4.f32 	[parmOffset+128],_df32v4 ;//df32v4 
					   
    st.global.u32 	[parmOffset+56],_filler ; //var for timing before program starts
	

	exit;
	} 
";
        CUDA cuda;
        CUResult lasterror;

        System.Diagnostics.Process p;
        public InstTimerAndTester()
        {
            // Init CUDA, select 1st device.
            cuda = new CUDA(0, true);
            lasterror = cuda.LastError;

            p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "ptxas.exe";
            p.StartInfo.Arguments = @"""" + Environment.GetEnvironmentVariable("TEMP") + @"\clock.ptx""  -o """ + Environment.GetEnvironmentVariable("TEMP") + @"\clock.cubin""";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
        }

        ~InstTimerAndTester()
        {
            //cuda.Free(dtimer);
        }

        public int Time(string inst)
        {
            //Write PTX file
            //output = System.Text.RegularExpressions.Regex.Replace(output,@"""(\d\d\d\d)-0?(\d+)-0?(\d+)T00:00:00-\d\d:00""", @"""${2}/${3}/${1}""", System.Text.RegularExpressions.RegexOptions.Multiline);
            string ptxFile = System.IO.Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "clock.ptx");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(ptxFile);
            sw.Write(top); 
            sw.Write(inst);
            sw.Write(bot);
            sw.Flush(); sw.Close();

            // PTX --> Cubin
            p.Start();
            System.IO.StreamReader sr = p.StandardOutput;
            p.WaitForExit();
            string output = sr.ReadLine();
            //Console.WriteLine(output);
            //Console.WriteLine(p.StartInfo.Arguments);


            lasterror = cuda.LastError;
            cuda.LoadModule(ptxFile);
            lasterror = cuda.LastError;
            CUfunction func = cuda.GetModuleFunction("timedReduction");
            lasterror = cuda.LastError;

            int[] timer = new int[202]; //2 for timer, 200 for junk

            CUdeviceptr dtimer = cuda.Allocate<int>(timer);
            lasterror = cuda.LastError;

            cuda.SetParameter(func,0, (uint)dtimer.Pointer);
            lasterror = cuda.LastError;
            cuda.SetParameterSize(func, (uint)(IntPtr.Size * 1));
            lasterror = cuda.LastError;
            cuda.SetFunctionBlockShape(func, 1, 1, 1);
            lasterror = cuda.LastError;
            cuda.Launch(func, 1, 1);
            lasterror = cuda.LastError;
            cuda.CopyDeviceToHost<int>(dtimer, timer);
            lasterror = cuda.LastError;
            cuda.Free(dtimer);
            lasterror = cuda.LastError;

            return timer[1] - timer[0];
        }
    }
}
