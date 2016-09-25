#ifndef _CLOCK_KERNEL_H_
#define _CLOCK_KERNEL_H_

extern "C" __global__ void timedReduction(clock_t * timer)
{
	int s32 = threadIdx.x;
	short s16 = threadIdx.x;
	unsigned int u32 = threadIdx.x;
	unsigned short u16 = threadIdx.x;
	//float16 f16=1; ???
	float f32 = threadIdx.x;
	
    __syncthreads();
    timer[0] = clock();
	//do stuff here

    timer[1] = clock();
	timer[1]=timer[1]-90;
}

#endif // _CLOCK_KERNEL_H_
