// PTX Instruction Generator and Timer
// This projected is licensed under the terms of the MIT license.
// NO WARRANTY. THE SOFTWARE IS PROVIDED TO YOU “AS IS” AND “WITH ALL FAULTS.”
// ANY USE OF THE SOFTWARE IS ENTIRELY AT YOUR OWN RISK.
// Created by Ryan S. White in 2009; Updated in 2011

//Notes
//==================================================
//Removed Vectors
//Removed b8 because ld st are the only instructions that can read it.
//Removed instructions with two outputs (set and setp)

//regex search for vs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RX = System.Text.RegularExpressions;
using System.Xml;
using System.IO;

namespace InstNS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BufferHeight = 7000;Console.BufferWidth = 150;

            PTXInstructionGenerator instruction_list = new PTXInstructionGenerator(true);
            instruction_list.SaveInstructionsToXMLFile("instructions.xml");
            instruction_list.SaveInstructionsToCSFile(@"test.cs");
            foreach (Inst inst in instruction_list.GetInstructionsAsArray())
                Console.WriteLine(inst);
            foreach (Inst inst in instruction_list.GetInstructionsAsArray())
                Console.WriteLine(inst.ToString(1,2,3,4,5));

            Console.WriteLine("Time in k-ticks: " + (instruction_list.stopwatch.ElapsedTicks/1000).ToString());
            Console.WriteLine("Time in mil-sec: " + instruction_list.stopwatch.ElapsedMilliseconds.ToString());
            Console.ReadKey();
        }
    }


    class PTXInstructionGenerator
    {
        public List<Inst> insts = new List<Inst>();
        public List<InstCat> instCats = new List<InstCat>();
        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        InstTimerAndTester ct;
        public bool timingsEnabled = true;

        public PTXInstructionGenerator(bool timingsEnabled)
        {
            if (timingsEnabled)
                ct = new InstTimerAndTester();
            
            //int time = ct.Time("setp.lt.and.f32 _dPdt, _sf32, 5, 5;");
            //int tim2 = ct.Time("setp.lt.f32 _dPdt, _sf32, 5;");

            stopwatch.Start();
            this.timingsEnabled = timingsEnabled;
            string[] del = { "\r\n", "\r", "\n" };
            String input = @"
[class=add,popularity=4,ticks=15] add.(rn|rz).(|sat).(f32) _d$3, _s$3, _s$3;
[class=sub,popularity=4,ticks=15] sub.(rn|rz).(|sat).(f32) _d$3, _s$3, _s$3;
[class=mul,popularity=4,ticks=15] mul.(rn|rz).(|sat).(f32) _d$3, _s$3, _s$3;
[class=mad,popularity=3,ticks=15] mad.(|sat).(f32) _d$2, _s$2, _s$2, _s$2;
[class=div,popularity=4,ticks=15] div.approx.(f32) _d$1, _s$1, _s$1;
[class=abs,popularity=2,ticks=15] abs.(f32) _d$1, _s$1;
[class=neg,popularity=2,ticks=15] neg.(f32) _d$1, _s$1;
[class=max,popularity=3,ticks=15] max.(f32) _d$1, _s$1, _s$1;
[class=min,popularity=3,ticks=15] min.(f32) _d$1, _s$1, _s$1;
[class=set,popularity=3,ticks=15]
[subpopularity=5]                 set.(CmpOpF).(f32).(f32) _d$2, _s$3, _s$3;
[subpopularity=1]                 set.(CmpOpF).(BoolOp).(f32).(f32) _d$3, _s$4, _s$4, (!|)_sPdt;
[subpopularity=1]                 setp.(CmpOpF).(f32) _dPdt, _s$2, _s$2;
[subpopularity=1]                 setp.(CmpOpF).(BoolOp).(f32) _dPdt, _s$3, _s$3, (!|)_sPdt;
[class=slt,popularity=3,ticks=15] selp.(f32) _d$1, _s$1, _s$1, _sPdt;
                                  slct.(f32).(f32) _d$1, _s$1, _s$1, _s$2;
[class=sat,popularity=3,ticks=15] cvt.(sat).(f32).(f32) _d$2, _s$3;
                                  cvt.rpi.(f32).(f32) _d$1, _s$2;       //added 6-1-09 - ceilf(a)
[class=rcpl,popularity=1,ticks=15]rcp.approx.(f32) _d$1, _s$1;
[class=sqrt,popularity=2,ticks=15]sqrt.approx.(f32) _d$1, _s$1;
[class=rsqt,popularity=2,ticks=15]rsqrt.approx.(f32) _d$1, _s$1;
[class=trig,popularity=1,ticks=15]sin.approx.(f32) _d$1, _s$1;
                                  cos.approx.(f32) _d$1, _s$1;
[class=log,popularity=2,ticks=15] lg2.approx.(f32) _d$1, _s$1;
[class=exp,popularity=2,ticks=15] ex2.(f32) _d$1, _s$1;
";

            //clean comments "\\"
            input = RX.Regex.Replace(input, @"\s*//.*$", @"", RX.RegexOptions.Multiline);
            //removed blank lines
            input = RX.Regex.Replace(input, @"\r\n\s*(?=\r\n)", @"", RX.RegexOptions.Multiline);
            //trim back
            input = RX.Regex.Replace(input, @"\s*(?=\r\n)", @"", RX.RegexOptions.Multiline);
            //split up specifiers on seperate lines (one per line)
            input = RX.Regex.Replace(input, @"]\s*(?!\r\n)", "]\r\n", RX.RegexOptions.Multiline);
            //trim front
            input = RX.Regex.Replace(input, @"\r\n\s*", "\r\n", RX.RegexOptions.Multiline);

            //RX.Regex d = new RX.Regex(@"
            Dictionary<String, String> replacements = new Dictionary<string, string>();
            replacements.Add("I_Type", "u16|u32|u64|s16|s32|s64");
            replacements.Add("B_Type", "b16|b32|b64");
            replacements.Add("Sp16v4", "tid|ntid|ctaid|nctaid");
            replacements.Add("Sp16v1", "gridid");
            replacements.Add("Sp32v1", "clock");
            replacements.Add("Vector", "v2|v4");
            replacements.Add("BoolOp", "and|or|xor");
            replacements.Add("CmpOpS", "eq|ne|lt|le|gt|ge");
            replacements.Add("CmpOpU", "eq|ne|lo|ls|hi|hs");
            replacements.Add("CmpOpB", "eq|ne");
            replacements.Add("CmpOpF", "eq|ne|lt|le|gt|ge|equ|neu|ltu|leu|gtu|geu|num|nan");
            replacements.Add("RndMdF", "rn|rz|rm|rp");
            replacements.Add("RndMdI", "rni|rzi|rmi|rpi");
            input = MultipleReplace(input, replacements);

            string[] lines = input.Split(del, StringSplitOptions.RemoveEmptyEntries);

            //mad24.      (hi|lo)            .       (|sat)           .         (u32|s32)    _d$3,_s$3,_s$3,_s$3;
            //static0-1...(options1-2)...static1-3...(options2-4)...static2-5...(options3-6)...remaining-7......;
            RX.Regex d = new RX.Regex(@"(?:(.+?)\((.+?)\))?(?:(.+?)\((.+?)\))?(?:(.+?)\((.+?)\))?(?:(.+?)\((.+?)\))?(?:(.+?)\((.+?)\))?(.+)");

            int srcLineNo = 0;
            foreach (string line in lines)
            {
                RX.Match match = d.Match(line);
                
                string remaining = match.Groups[11].Value;

                if (match.Groups[1].Success)
                { //set 1 exists
                    string[] options1 = match.Groups[2].Value.Split('|');
                    string subtotal1 = /*static2*/match.Groups[1].Value;
                    foreach (string option1 in options1)
                        if (match.Groups[3].Success)
                        { //set 2 exists
                            string[] options2 = match.Groups[4].Value.Split('|');
                            string subtotal2 = subtotal1 + option1 + /*static2*/match.Groups[3].Value;
                            foreach (string option2 in options2)
                                if (match.Groups[5].Success)
                                { //set 3 exists
                                    string[] options3 = match.Groups[6].Value.Split('|');
                                    string subtotal3 = subtotal2  + option2 + /*static3*/match.Groups[5].Value;
                                    foreach (string option3 in options3)
                                        if (match.Groups[7].Success)
                                        { //set 4 exists
                                            string[] options4 = match.Groups[8].Value.Split('|');
                                            string subtotal4 = subtotal3 +  option3 + /*static4*/match.Groups[7].Value;
                                            foreach (string option4 in options4)
                                                if (match.Groups[9].Success)
                                                { //set 5 exists
                                                    string[] options5 = match.Groups[10].Value.Split('|');
                                                    string subtotal5 = subtotal4 + option4 + /*static5*/match.Groups[9].Value;
                                                    foreach (string option5 in options5)
                                                        ProcessEachLine(subtotal5 + option5 + remaining, option1, option2, option3, option4, option5, srcLineNo);
                                                }
                                                else ProcessEachLine(subtotal4 + option4 + remaining, option1, option2, option3, option4, "", srcLineNo);
                                        }
                                        else
                                            ProcessEachLine(subtotal3 + option3 + remaining, option1, option2, option3, "", "", srcLineNo);
                                }
                                else
                                    ProcessEachLine(subtotal2 + option2 + remaining, option1, option2, "", "", "", srcLineNo);
                        }
                        else
                            ProcessEachLine(subtotal1 + option1 + remaining, option1, "", "", "", "", srcLineNo);
                }
                else
                    ProcessEachLine(remaining, "", "", "", "", "", srcLineNo);

                srcLineNo++;
            }

            ////////// CALCULATE THE FINAL POPULARITY  /////////////

            //lets normalize(with 65536) the popularity foreach cat
            int catPopularitySummed = 0;
            foreach (InstCat ic in instCats)
                catPopularitySummed += ic.popularity;
            foreach (InstCat ic in instCats)
                ic.popularity = 65536 * ic.popularity / catPopularitySummed;

            //now lets normalize the popularity for each instructions
            int instPopularitySummed = 0;
            int last_cat = 0;
            int last_calculated = 0;
            for (int i = 0; i < insts.Count; i++)
            {
                bool new_cat = (insts[i].catNum != last_cat);
                if (new_cat)
                {
                    for (int j = last_calculated; j < i; j++)
                        insts[j].popularity = instCats[last_cat].popularity * insts[j].popularity / instPopularitySummed;

                    //lets start the next one.
                    instPopularitySummed = insts[i].popularity;
                    last_calculated = i;
                    last_cat = insts[i].catNum;
                }
                else
                    instPopularitySummed += insts[i].popularity;
            }
            // and the last cat
            for (int j = last_calculated; j < insts.Count; j++)
                insts[j].popularity = instCats[last_cat].popularity * insts[j].popularity / instPopularitySummed;


            //now lets multiply the two popularities and mult by 32768
            float cur_total = 65536;
            foreach (Inst inst in insts)
                cur_total -= inst.popularity;
                //cur_total += inst.popularity = (int)((Int64)inst.popularity * (Int64)instCats[inst.catNum].popularity / ((Int64)262144*262144/32768));

            //we will have some left over - lets just add 1 to each until we are at a 65536 total.
            for (int i = 0; i < cur_total; i++)
                insts[i].popularity++;

            stopwatch.Stop();
        }


        
        int cur_subpopularity = 1;
        int cur_class = -1;
        RX.Regex reg = new RX.Regex(@"(_(s|d)((?:(s|u|f|b)(08|16|32|64))|Pdt)|\[class=(.{1,20}),popularity=(\d+),ticks=(\d+)\]|\[subpopularity=(\d+)\])",RX.RegexOptions.Compiled);
        void ProcessEachLine(string text, string v1, string v2, string v3, string v4, string v5, int srcRowNo)
        {
            //fill $1, $2, $3  0x31
            int idxPattern = 5; //at least start 5 in
            int idxLast = 0;
            StringBuilder result = new StringBuilder(64);
            while (true)
            {
                //move cursor to the next $
                idxPattern = text.IndexOf('$', idxPattern + 1);

                //If there's no $'s left on this line, lets write the rest of the line and exit
                if (idxPattern < 0)
                {
                    result.Append(text, idxLast, text.Length - idxLast);
                    break;
                }

                //Output the stuff before the cursor.
                int appendSize = idxPattern - idxLast;
                if (appendSize > 0)
                  result.Append(text, idxLast, appendSize);

                //find the number in "_s$2" and output the string v2.
                switch ((int)text[idxPattern+1]-0x31)
                {
                    case 0:   result.Append(v1); break;
                    case 1:   result.Append(v2); break;
                    case 2:   result.Append(v3); break;
                    case 3:   result.Append(v4); break;
                    case 4:   result.Append(v5); break;
                    default:  throw new System.ArgumentException("Expected 5 Regs Max.", "Error"); 
                }

                //save lastCurserPos
                idxLast = idxPattern + 2;  //2 is lenPattern
            }
            text = result.ToString();

            /////////Now we need to create instructions object for this line/////////
            RX.MatchCollection matches = reg.Matches(text);

            //If this is a new instruction type then lets write it down and exit
            if (matches[0].Groups[9].Value != "") // [subpopularity=?] type line
            {
                cur_subpopularity = int.Parse(matches[0].Groups[9].Value);
                return;
            }
            else if (matches[0].Groups[6].Value != "") // [class=sqrt,popularity=3,ticks=15] type line
            {
                cur_subpopularity = 1;

                cur_class++;   //next class (add,sub,mul...)
                string cur_desc = matches[0].Groups[6].Value;   //popularity for this group
                int cur_pop = int.Parse(matches[0].Groups[7].Value);   //popularity for this group
                int cur_ticks = int.Parse(matches[0].Groups[8].Value); //avg # of ticks for instruction
                instCats.Add(new InstCat(cur_class,0,cur_pop,cur_desc));
                return;
            }
            else // instruction line
                instCats[cur_class].count++;  

         
            //Clean up ".."
            text = RX.Regex.Replace(text, @"(?<=\.)\.+", "");

            Inst inst = new Inst() {lineString = text, catNum = srcRowNo, input_count = matches.Count-1};
            inst.lineString = text;
            inst.popularity = cur_subpopularity;
            inst.catNum = cur_class;
            if (timingsEnabled)
            {
                inst.ticks = ct.Time(text);
                Console.WriteLine("Ticks: " + inst.ticks + " for " + text);
            }
            foreach (RX.Match match in matches)
            {
                VarType theType = (VarType)Enum.Parse(typeof(VarType), match.Groups[3].Value);
                
                if (match.Groups[2].Value == "d")
                    inst.outputType = theType;
                else if (match.Groups[2].Value == "s")
                    inst.AddInput(theType);
                else
                    throw new System.ArgumentException("A param should be either s or d type.", "Error");

            }
            inst.id = insts.Count;

            //Is there a "!" in front of the 3rd input
            inst.notInFrontOfInput3 = RX.Regex.IsMatch(inst.lineString, @", ?!_");

            //Lets strip away the opps behind the instruction   ie: the "_d32, _s32, _s32"
            inst.lineString = RX.Regex.Replace(inst.lineString, @" _.*","");

            //don't add instructions over 600 ticks
            if (inst.ticks < 300)
              insts.Add(inst);
        }


        Random rand = new Random();
        public Inst PickRandomInstruction()
        {
            return insts[rand.Next(insts.Count)];
        }

        public Inst[] GetInstructionsAsArray()
        {
            return insts.ToArray();
        }


        public void SaveInstructionsToCSFile(string fileName)
        {
            ////Read in the sharedclasses.cs file to outfile variable////
            StreamReader sr = new StreamReader(@"..\..\SharedClasses.cs");
            StringBuilder outfile = new StringBuilder(sr.ReadToEnd());
            sr.Close();

            ////Do the categories replacement ////
            StringBuilder catsOutput = new StringBuilder();
            foreach (InstCat ic in instCats)
                catsOutput.AppendLine(@"new InstCat(" + ic.ID + "," + ic.count + "," + ic.popularity + ",\"" + ic.desc +"\"),");
            outfile.Replace(@"/*INSERT CATS HERE*/", catsOutput.ToString()); 

            ////Do the instructions replacement ////
            StringBuilder instsOutput = new StringBuilder();
            foreach (Inst inst in insts)
                instsOutput.AppendLine(@"new Inst(" + inst.id 
                    + "," + inst.catNum 
                    + "," + inst.popularity 
                    + "," + inst.ticks/3 
                    + ",\"" + inst.lineString 
                    + "\",VarType." + inst.outputType 
                    + "," + inst.input_count 
                    + ", VarType." + inst.inputTypes[0]
                    + ", VarType." + inst.inputTypes[1]
                    + ", VarType." + inst.inputTypes[2]
                    + ", VarType." + inst.inputTypes[3] 
                    + ", " + inst.notInFrontOfInput3.ToString().ToLower()
                    + "),");
            outfile.Replace(@"/*INSERT INSTS HERE*/", instsOutput.ToString()); 

            outfile.Replace(@"1000/*INST COUNT*/", insts.Count.ToString()); 

            

            ////Cleanup - remove "," in ",};"
            outfile.Replace(",\r\n};", "};\r\n");
            //string outToFile = System.Text.RegularExpressions.Regex.Replace(outfile.ToString(), @",};", @"};");

            //// Now save outfile string to file. ////
            StreamWriter sw = new StreamWriter(fileName);
            sw.Write(outfile.ToString());
            sw.Flush(); sw.Close();
        }


        public void SaveInstructionsToXMLFile(string filenameAndPath)
        {
            XmlTextWriter textWriter = new XmlTextWriter(filenameAndPath, null);
            XmlDocument xmldoc = new XmlDocument();

            textWriter.WriteStartDocument();
            textWriter.WriteComment("PTX Instructions List");
            textWriter.WriteComment("Generated by the PTX instruction generator");
            textWriter.WriteStartElement("Everything");
            textWriter.WriteStartElement("InstructionClasses");
            foreach (InstCat inst_class in instCats)
            {
                textWriter.WriteStartElement("Class" );
                textWriter.WriteAttributeString("id", inst_class.ID.ToString());
                textWriter.WriteAttributeString("popularity", inst_class.popularity.ToString());
                textWriter.WriteAttributeString("count", inst_class.count.ToString());
                textWriter.WriteAttributeString("text", inst_class.desc.ToString());
                textWriter.WriteEndElement();
            }
            textWriter.WriteEndElement();
            textWriter.WriteStartElement("Instructions");
            foreach (Inst inst in insts)
            {
                textWriter.WriteStartElement("Instruction" );
                textWriter.WriteAttributeString("id",            inst.id.ToString());
                textWriter.WriteAttributeString("cat",           inst.catNum.ToString());
                textWriter.WriteAttributeString("subpopularity", inst.popularity.ToString("0.0000000"));
                textWriter.WriteAttributeString("ticks",         inst.ticks.ToString());
                textWriter.WriteAttributeString("text",          inst.lineString.ToString());
                textWriter.WriteAttributeString("outputType",((int)inst.outputType).ToString());
                foreach (VarType input in inst.inputTypes)
                {
                    textWriter.WriteStartElement("Inputs");
                    textWriter.WriteValue((int)input);
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
            }
            textWriter.WriteEndDocument();
            textWriter.Close();
        }


        public Inst[] GetInstructionsAsArray(int input_count)
        {
            var filteredInstruction = from n in insts
            //where n.src.FirstOrDefault().type == RegisterType.f32 
            //  || n.src.FirstOrDefault().type == RegisterType.s32
            where n.inputTypes.Length == input_count
            select n;
            return filteredInstruction.ToArray();
        }


        string MultipleReplace(string text, Dictionary<string, string> replacements)
        {
            return RX.Regex.Replace(text,
                "(" + String.Join("|", replacements.Keys.ToArray()) + ")",
                delegate(RX.Match m) { return replacements[m.Value]; });
        }
    }
}//end namespace












//////////////////////////////
//Removed the below because...
//  The instruction did not fit well with my algorithm (mov, call
//  Was for 64 bit (note: i removed some 64-bit choices - will need to find and add them in again
////////////////////////////// 


//add.(|sat).(I_Type) _d$2, _s$2, _s$2;
//add.(|RndMdF).(|sat).(f32|f64) _d$3, _s$3, _s$3;
//sub.(|sat).(I_Type) _d$2, _s$2, _s$2;
//sub.(|RndMdF).(|sat).(f32|f64) _d$3, _s$3, _s$3;

//mul.(hi|lo).(I_Type) _d$2, _s$2, _s$2;
//mul.wide.s16 _ds32, _ss16, _ss16;
//mul.wide.u16 _du32, _su16, _su16;
//mul.wide.s32 _ds64, _ss32, _ss32;
//mul.wide.u32 _du64, _su32, _su32;
//mul.(|RndMdF).(|sat).(f32|f64) _d$3, _s$3, _s$3;

//mad.(hi|lo).(|sat).(I_Type) _d$3, _s$3, _s$3, _s$3;
//mad.wide.s16 _ds32, _ss16, _ss16, _ss16;
//mad.wide.u16 _du32, _su16, _su16, _ss16;
//mad.wide.s32 _ds64, _ss32, _ss32, _ss32;
//mad.wide.u32 _du64, _su32, _su32, _ss32;
//mad.(|RndMdF).(|sat).(f32|f64) _d$3, _s$3, _s$3, _s$3;
//mul24.(hi|lo).(u32|s32) _d$2, _s$2, _s$2;
//mad24.(hi|lo).(|sat).(u32|s32) _d$3, _s$3, _s$3, _s$3;
//sad.(I_Type) _d$1, _s$1, _s$1, _s$1;
//div.(|sat).(f32|f64) _d$2, _s$2, _s$2;
//div.(u64|s64) _d$1, _s$1, _s$1;
//div.wide.s16 _ds32, _ss16, _ss16;
//div.wide.u16 _du32, _su16, _su16;
//div.wide.s32 _ds64, _ss32, _ss32;
//div.wide.u32 _du64, _su32, _su32;
//rem.wide.s16 _ds32, _ss16, _ss16;
//rem.wide.u16 _du32, _su16, _su16;
//rem.wide.s32 _ds64, _ss32, _ss32;
//rem.wide.u32 _du64, _su32, _su32;
//abs.(s16|s32|s64|f32|f64) _d$1, _s$1;
//neg.(s16|s32|s64|f32|f64) _d$1, _s$1;
//max.(I_Type|f32|f64) _d$1, _s$1, _s$1;
//min.(I_Type|f32|f64) _d$1, _s$1, _s$1;

//set.(CmpOpU).(s32|u32|f32).(u16|u32|u64) _d$2, _s$3, _s$3;
//set.(CmpOpS).(s32|u32|f32).(s16|s32|s64) _d$2, _s$3, _s$3;
//set.(CmpOpB).(s32|u32|f32).(b16|b32|b64) _d$2, _s$3, _s$3;
//set.(CmpOpF).(s32|u32|f32).(f32|f64) _d$2, _s$3, _s$32;
//set.(CmpOpU).(BoolOp).(s32|u32|f32).(u16|u32|u64) _d$3, _s$4, _s$4, (!|)_sPdt;
//set.(CmpOpS).(BoolOp).(s32|u32|f32).(s16|s32|s64) _d$3, _s$4, _s$4, (!|)_sPdt;
//set.(CmpOpB).(BoolOp).(s32|u32|f32).(b16|b32|b64) _d$3, _s$4, _s$4, (!|)_sPdt;
//set.(CmpOpF).(BoolOp).(s32|u32|f32).(f32|f64) _d$3, _s$4, _s$4, (!|)_sPdt;

//setp.(CmpOpU).(u16|u32|u64) _dPdt, _s$2, _s$2;
//setp.(CmpOpS).(s16|s32|s64) _dPdt, _s$2, _s$2;
//setp.(CmpOpB).(b16|b32|b64) _dPdt, _s$2, _s$2;
//setp.(CmpOpF).(f32|f64) _dPdt, _s$2, _s$2;
//setp.(CmpOpU).(u16|u32|u64) _dPdt| _dPdt, _s$2, _s$2;
//setp.(CmpOpS).(s16|s32|s64) _dPdt| _dPdt, _s$2, _s$2;
//setp.(CmpOpB).(b16|b32|b64) _dPdt| _dPdt, _s$2, _s$2;
//setp.(CmpOpF).(f32|f64) _dPdt| _dPdt, _s$2, _s$2;
//setp.(CmpOpU).(BoolOp).(u16|u32|u64) _dPdt, _s$3, _s$3, (!|)_sPdt;
//setp.(CmpOpS).(BoolOp).(s16|s32|s64) _dPdt, _s$3, _s$3, (!|)_sPdt;
//setp.(CmpOpB).(BoolOp).(b16|b32|b64) _dPdt, _s$3, _s$3, (!|)_sPdt;
//setp.(CmpOpF).(BoolOp).(f32|f64) _dPdt, _s$3, _s$3, (!|)_sPdt;
//setp.(CmpOpU).(BoolOp).(u16|u32|u64) _dPdt, _dPdt, _s$3, _s$3, (!|)_sPdt;
//setp.(CmpOpS).(BoolOp).(s16|s32|s64) _dPdt, _dPdt, _s$3, _s$3, (!|)_sPdt;
//setp.(CmpOpB).(BoolOp).(b16|b32|b64) _dPdt, _dPdt, _s$3, _s$3, (!|)_sPdt;
//setp.(CmpOpF).(BoolOp).(f32|f64) _dPdt, _dPdt, _s$3, _s$3, (!|)_sPdt;

//selp.(I_Type|f32|f64|B_Type) _d$1, _s$1, _s$1, _sPdt;

//slct.(I_Type|f32|f64|B_Type).(s32|f32) _d$1, _s$1, _s$1, _s$2;

//and.(B_Type|Pdt) _d$1, _s$1, _s$1;
//or.(B_Type|Pdt) _d$1, _s$1, _s$1;
//xor.(B_Type|Pdt) _d$1, _s$1, _s$1;
//not.(B_Type|Pdt) _d$1, _s$1;
//cnot.(B_Type) _d$1, _s$1;
//shl.(B_Type) _d$1, _s$1, _s$1;
//shr.(B_Type|I_Type) _d$1, _s$1, _s$1;

//mov.(I_Type|f32|f64|B_Type|Pdt) _d$1, _s$1; //_s$|sreg|NamVar|lblAdr|_s$[Const],Const

//ld.(const|global|local|param|shared).(|Vector).(b08|u08|s08|I_Type|f32|f64|B_Type) _d$3$2, [_s$3$2]; //_s$|NamVar|a+Const|Const
//ld.volatile.(global|shared).(|Vector).(b08|u08|s08|I_Type|f32|f64|B_Type) _d$3$2, [_s$3$2];          // _s$|NamVar|a+Const|Const

//st.(global|local|shared).(|Vector).(b08|u08|s08|I_Type|f32|f64|B_Type) [_d$3], _s$3;                   //_d$|_d$+Const|Const|NamVar
//st.volatile.(global|shared).(|Vector).(b08|u08|s08|I_Type|f32|f64|B_Type) [_d$3], _s$3;                //_d$|_d$+Const|Const|NamVar

//cvt.(|sat).(u08|s08|I_Type|f16|f32|f64).(b08|u08|s08|I_Type|f16|f32|f64) _d$2, _s$3;
//cvt.(RndMdI).(u08|s08|I_Type).(f16|f32|f64) _d$2, _s$3;
//cvt.(RndMdI).f16.f16 _df16, _sf16;
//cvt.(RndMdI).f32.f32 _df32, _sf32;
//cvt.(RndMdI).f64.f64 _df64, _sf64;
//cvt.(RndMdF).(|sat).(f16|f32|f64).(b08|u08|s08|I_Type) _d$3, _s$4;

////tex.(1d|2d|3d).v4.(u32|s32|f32).(s32|f32) _d$2, [_s$, _s$3];
////bar.sync const;
////bra
////call
////ret
////exit

////atom.(global|shared).(and|or|xor|exch).b32 _db32, [_sb32], _sb32;  //_s$|NamVar|a+Const|Const
////atom.(global|shared).cas.(b32|b64) _d$2, [_s2$], _s$2, _s$2;       //_s$|NamVar|a+Const|Const
////atom.(global|shared).exch.b64 _db64, [_sb64], _sb64;               //_s$|NamVar|a+Const|Const
////atom.(global|shared).(add|min|max|inc|dec).u32 _du32, [_su32], _su32;//_s$|NamVar|a+Const|Const
////atom.(global|shared).(add).u64 _du64, [_su64], _su64;              //_s$|NamVar|a+Const|Const
////atom.(global|shared).(add|min|max).(s32|f32) _d$3, [_s$3], _s$3;   //_s$|NamVar|a+Const|Const

////red.(global|shared).(and|or|xor|exch).b32 _db32, [_sb32];  //_s$|NamVar|a+Const|Const
////red.(global|shared).cas.b32 _db32, [_sb32], _sb32, _sb32;    //_s$|NamVar|a+Const|Const
////red.(global|shared).(add|min|max|inc|dec).u32 _du32, [_su32)];//_s$|NamVar|a+Const|Const
////red.(global|shared).(add).u64 _d$3, [_su64];            //_s$|NamVar|a+Const|Const
////red.(global|shared).(add|min|max).(s32|f32) _d$3, [_s$3];//_s$|NamVar|a+Const|Const

////vote.(all|any|uni).Pdt  _dPdt, (!|)_sPdt;
//rcp.(f32|f64) _d$1, _s$1;
//sqrt.(f32|f64) _d$1, _s$1;
//rsqrt.(f32|f64) _d$1, _s$1;
//sin.(f32) _d$1, _s$1;
//cos.(f32) _d$1, _s$1; 
//lg2.(f32) _d$1, _s$1;
//ex2.(f32) _d$1, _s$1; //test