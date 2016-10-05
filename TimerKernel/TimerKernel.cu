#ifndef _CLOCK_KERNEL_H_
#define _CLOCK_KERNEL_H_

extern "C" __global__ void timedFunc(clock_t * timer)
{
	asm volatile ("{\n\t"	
	".reg .b32 	r<5>;\n\t"
	".reg .b64 	rd<3>;\n\t"
	"ld.param.u64 	rd1, [timedFunc_param_0];\n\t"
	"cvta.to.global.u64 	rd2, rd1;\n\t"
	"mov.u32 	r1, %clock;\n\t"
	"mov.u32 	r2, %clock;\n\t"
	"mov.u32 	r1, %clock;\n\t"
	"mov.u32 	r2, %clock;\n\t"
	"mov.u32 	r1, %clock;\n\t"
	"mov.u32 	r2, %clock;\n\t"

	"mov.u32 	r1, %clock;\n\t"
	"mov.s32 r6, r6;\n\t"
	"mov.u32 	r2, %clock;\n\t"
	
	"membar.gl;\n\t"
	"st.global.u32 	[rd2], r1;\n\t"
	"st.global.u32 	[rd2+4], r2;\n\t"
	"ret;\n\t"
    "}");
}

#endif // _CLOCK_KERNEL_H_
