// PTX Instruction Generator and Benchmark Utility 
// This projected is licensed under the terms of the MIT license.
// NO WARRANTY. THE SOFTWARE IS PROVIDED TO YOU “AS IS” AND “WITH ALL FAULTS.”
// ANY USE OF THE SOFTWARE IS ENTIRELY AT YOUR OWN RISK.
// Created by Ryan S. White in 2009; Updated in 2011; Updated in 2016
// Note: This code was not written for public consumption so please excuse the sloppiness.

using System;
using System.Collections.Generic;

namespace InstNS
{
    public static class Tools
    {
        public static InstCat[] instCats = {/*INSERT CATS HERE*/};

        public static Inst[] insts = {/*INSERT INSTS HERE*/};

        public static int[] GetRandList65536()
        {
            int[] list = new int[65536];
            Random rand = new Random();

            int i = 0;
            for (int j = 0; j < 1000/*INST COUNT*/; j++)
                for (int r = 0; r < insts[i].popularity; r++)
                    list[j++] = i;

            for (int j = 0; j < list.Length; j++)
            {
                int toSwapWith = rand.Next(0, list.Length);
                int temp = list[toSwapWith];
                list[toSwapWith] = list[j];
                list[j] = temp;
            }
            return list;
        }

        public static int[] ReShuffle(int[] list)
        {
            Random rand = new Random();
            for (int j = 0; j < list.Length; j++)
            {
                int toSwapWith = rand.Next(0, list.Length);
                int temp = list[toSwapWith];
                list[toSwapWith] = list[j];
                list[j] = temp;
            }
            return list;
        }
    }

    public class InstCat
    {
        public int ID;
        public int count = 0;
        public int popularity = 0;
        public string desc;
        public InstCat(int ID, int count, int popularity, string desc)
        {
            this.ID = ID;
            this.count = count;
            this.popularity = popularity;
            this.desc = desc;
        }
    }

    public class Inst
    {
        /// <summary>
        /// The instruction ID.
        /// </summary>
        public int id;
        public int popularity = 1;
        public bool notInFrontOfInput3 = false;
        public int ticks = -1;
        /// <summary>
        /// The template for this instruction. (Ex: mul.wide.s16 ds32, ss16, ss16;)
        /// </summary>
        public string lineString;
        /// <summary>
        /// The category of this instruction. (DivFloat,AddInt,SubFloat)
        /// Formed from the line# that was expanded to form this resulting instruction.)
        /// </summary>
        public int catNum;
        public List<VarType> inputs = new List<VarType>();
        public VarType outputType;

        //public Inst(int id, int catNum, int popularity, int ticks, string lineString, VarType outputType, int inputcount, VarType inputTypes0, VarType inputTypes1, VarType inputTypes2, VarType inputTypes3, bool notInFrontOfInput3)
        //{
        //   this.id            = id;
        //   this.catNum        = catNum;
        //   this.popularity    = popularity;
        //   this.ticks         = ticks;
        //   this.lineString    = lineString;
        //   this.outputType    = outputType;
        //   this.inputcount = inputcount;
        //   this.inputTypes[0] = inputTypes0;
        //   this.inputTypes[1] = inputTypes1;
        //   this.inputTypes[2] = inputTypes2;
        //   this.inputTypes[3] = inputTypes3;
        //   this.notInFrontOfInput3 = notInFrontOfInput3;
        //}

        //Regex reg = new Regex(@"((s|d)((?:(s|u|f|b)(08|16|32|64))|Pred)",RegexOptions.Compiled);

        /// <summary>Returns the instruction with the opps filled in. (ie mult.f32 v1, v2, v3)</summary>
        public string ToString(int dest, params int[] sources)
        {
            string toReturn = lineString + " v" + dest;
            for (int i = 0; i < inputs.Count; i++)
                toReturn += ", v" + sources[i];
                //toReturn += ((notInFrontOfInput3) ? ", !v" : ", v") + source2;

            toReturn += ";\r\n";
            return toReturn;
        }

        /// <summary>
        /// Returns a string like ID:3 SrcLine:23 D:f32 S:s32,s32 add.rn.f32 
        /// </summary>
        public override string ToString()
        {
            string sources = "";
            foreach (VarType ri in inputs)
        		sources += ri + " ";
            return "ID:"+ id.ToString() +"\tSrcLine:" +catNum.ToString() + "\tD:"+ outputType + "\tS:"+ sources + "\t " + lineString;
        }

        /// <summary>
        /// Adds an input to the Input list.
        /// </summary>
        public void AddInput(VarType toAdd)
        {
            for (int i = 0; i < inputs.Count; i++)
                if (inputs[i] == VarType.Non)
                {
                    inputs[i] = toAdd;
                    break;
                }
        }
    }

    public enum VarType
    {
        f32 = 0,
        Pred = 1,
        Non = 2,
        b32 = 3,
        u08 = 4,
        u16 = 5,
        u32 = 6,
        u64 = 7,
        s08 = 8,
        s16 = 9,
        s32 = 10,
        s64 = 11,
        f16 = 12,
        b16 = 13,
        f64 = 14,
        b64 = 15
    }
}
