// PTX Instruction Generator and Benchmark Utility 
// This projected is licensed under the terms of the MIT license.
// NO WARRANTY. THE SOFTWARE IS PROVIDED TO YOU “AS IS” AND “WITH ALL FAULTS.”
// ANY USE OF THE SOFTWARE IS ENTIRELY AT YOUR OWN RISK.
// Created by Ryan S. White in 2009; Updated in 2011; Updated in 2016
// Note: This code was not written for public consumption so please excuse the sloppiness.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace InstNS
{
    class Program
    {
        static void Main(string[] args)
        {
            // Lets make the Console Screen a little more useful!
            Console.BufferHeight = 7000;
            Console.BufferWidth = 150;

            // Read in the 
            PTXInstructionGenerator instructionlist = new PTXInstructionGenerator(true);
            instructionlist.SaveInstructionsToXMLFile("ptxInstructions.xml");
            instructionlist.SaveInstructionsToCSFile("ptxInstructions.cs");
            foreach (Inst inst in instructionlist.GetInstructionsAsArray())
                Console.WriteLine(inst);
            foreach (Inst inst in instructionlist.GetInstructionsAsArray())
                Console.WriteLine(inst.ToString(1,2,3,4,5));

            Console.WriteLine("Time in k-ticks: " + (instructionlist.stopwatch.ElapsedTicks/1000).ToString());
            Console.WriteLine("Time in mil-sec: " + instructionlist.stopwatch.ElapsedMilliseconds.ToString());
            Console.ReadKey();
        }
    }


    class PTXInstructionGenerator
    {
        List<Inst> insts = new List<Inst>();
        List<InstCat> instCats = new List<InstCat>();
        public Stopwatch stopwatch = new Stopwatch();
        InstTimerAndTester ct;
        public bool timingsEnabled = true;
        int benchmarkBase = 0;


        public PTXInstructionGenerator(bool timingsEnabled)
        {
            if (timingsEnabled)
            {
                ct = new InstTimerAndTester();
                benchmarkBase = ct.Benchmark("", "", "");
            }

            stopwatch.Start();
            this.timingsEnabled = timingsEnabled;
            string[] del = { "\r\n", "\r", "\n" };
            string input = File.ReadAllText(@"InputSpecs.txt");

            ////////////// CLEANUP THE SPECIFICATION FILE //////////////
            // clean comments "\\"
            input = Regex.Replace(input, @"\s*//.*$", @"", RegexOptions.Multiline);
            // Let’s remove all the /*...*/ style comments
            input = Regex.Replace(input, @"/\*[^*]*\*+(?:[^*/][^*]*\*+)*/", string.Empty);
            // removed blank lines
            input = Regex.Replace(input, @"\r\n\s*(?=\r\n)", @"", RegexOptions.Multiline);
            // trim back
            input = Regex.Replace(input, @"\s*(?=\r\n)", @"", RegexOptions.Multiline);
            // split up specifiers on separate lines (one per line)
            input = Regex.Replace(input, @"]\s*(?!\r\n)", "]\r\n", RegexOptions.Multiline);
            // trim spaces and carriage returns from front
            input = Regex.Replace(input, @"\r\n\s*", "\r\n", RegexOptions.Multiline);

            ////////////// REPLACE CLASSES WITH THEIR PARTS //////////////
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            replacements.Add("IType", "u16|u32|u64|s16|s32|s64");
            replacements.Add("BType", "b16|b32|b64");
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

            //mad24.      (hi|lo)            .       (|sat)           .         (u32|s32)    d$3,s$3,s$3,s$3;
            //static0-1...(options1-2)...static1-3...(options2-4)...static2-5...(options3-6)...remaining-7......;
            Regex d = new Regex(@"(?:(.+?)\((.+?)\))?(?:(.+?)\((.+?)\))?(?:(.+?)\((.+?)\))?(?:(.+?)\((.+?)\))?(?:(.+?)\((.+?)\))?(.+)");

            int srcLineNo = 0;
            foreach (string line in lines)
            {
                Match match = d.Match(line);
                
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
            int lastcat = 0;
            int lastcalculated = 0;
            for (int i = 0; i < insts.Count; i++)
            {
                bool newcat = (insts[i].catNum != lastcat);
                if (newcat)
                {
                    for (int j = lastcalculated; j < i; j++)
                        insts[j].popularity = instCats[lastcat].popularity * insts[j].popularity / instPopularitySummed;

                    //lets start the next one.
                    instPopularitySummed = insts[i].popularity;
                    lastcalculated = i;
                    lastcat = insts[i].catNum;
                }
                else
                    instPopularitySummed += insts[i].popularity;
            }
            // and the last cat
            for (int j = lastcalculated; j < insts.Count; j++)
                insts[j].popularity = instCats[lastcat].popularity * insts[j].popularity / instPopularitySummed;


            //now lets multiply the two popularities and mult by 32768
            float curtotal = 65536;
            foreach (Inst inst in insts)
                curtotal -= inst.popularity;
                //curtotal += inst.popularity = (int)((Int64)inst.popularity * (Int64)instCats[inst.catNum].popularity / ((Int64)262144*262144/32768));

            stopwatch.Stop();
        }


        
        int curSubPopularity = 1;
        int curclass = -1;
        int curticks = -1;
        void ProcessEachLine(string line, string v1, string v2, string v3, string v4, string v5, int srcRowNo)
        {
            line = line.Trim();

            // If metadata, then process. ie. [class=sqrt,popularity=3,ticks=15] 
            if (line[0] == '[')
            {
                foreach (Match m in Regex.Matches(line, @"(class|ticks|popularity|subpopularity)=(\w+)"))
                {
                    // If this is a new curSubPopularity type then lets write it down and exit
                    string name = m.Groups[1].Value;
                    string value = m.Groups[2].Value;
                    switch (name)
                    {
                        case "class": //next class (add,sub,mul...)
                            curclass++; instCats.Add(new InstCat(curclass, 0, -9999, value));  
                            break;
                        case "ticks": curticks = int.Parse(value); break;
                        case "popularity": curSubPopularity = 1; curSubPopularity = int.Parse(value); break;
                        case "subpopularity": curSubPopularity = int.Parse(value); break;
                        default: break;
                    }
                }
                return;
            }


            ///////// replace $1, $2, $3... with option matches /////////
            int idxPattern = 5; //at least start 5 in
            int idxLast = 0;
            StringBuilder result = new StringBuilder(64);
            while (true)
            {
                //move cursor to the next $
                idxPattern = line.IndexOf('$', idxPattern + 1);

                //If there's no $'s left on this line, lets write the rest of the line and exit
                if (idxPattern < 0)
                {
                    result.Append(line, idxLast, line.Length - idxLast);
                    break;
                }

                //Output the stuff before the cursor.
                int appendSize = idxPattern - idxLast;
                if (appendSize > 0)
                  result.Append(line, idxLast, appendSize);

                //find the number in "s$2" and output the string v2.
                switch (line[idxPattern+1]-0x31)
                {
                    case 0:   result.Append(v1); break;
                    case 1:   result.Append(v2); break;
                    case 2:   result.Append(v3); break;
                    case 3:   result.Append(v4); break;
                    case 4:   result.Append(v5); break;
                    default:  throw new ArgumentException("Expected 5 Regs Max.", "Error"); 
                }

                //save lastCurserPos
                idxLast = idxPattern + 2;  //2 is lenPattern
            }
            line = result.ToString();


            ///////// Now we need to create instructions object for this line. /////////
            // instruction line
            instCats[curclass].count++;  

            // Clean up ".."
            line = Regex.Replace(line, @"(?<=\.)\.+", "");

            Inst inst = new Inst()
            {
                id = insts.Count,
                lineString = line,
                catNum = curclass,
                popularity = curSubPopularity,
            };

            string benchmarkHeader = "";
            string benchmarkTail = "";
            List<string> regParams = new List<string>(4);
            foreach (Match match in Regex.Matches(line, @"_(s|d)((?:(s|u|f|b)(8|16|32|64))|Pred)"))
            {
                string name = match.Value;
                string srcOrDesk = match.Groups[1].Value;
                VarType theType = (VarType)Enum.Parse(typeof(VarType), match.Groups[2].Value);
                string type = match.Groups[3].Value;
                string size = match.Groups[4].Value;
  
                if (srcOrDesk == "d")
                    inst.outputType = theType;
                else if (srcOrDesk == "s")
                    inst.AddInput(theType);
                else
                    throw new ArgumentException("A param should be either s or d type.", "Error");

                if (!regParams.Contains(name))
                {
                    if (theType == VarType.Pred)
                    {
                        benchmarkHeader += "\r\n\t .reg .pred " + name + ";";
                        if (srcOrDesk == "d")
                            benchmarkTail += "\r\n@" + name + "\t bra Skip;\r\n\t st.global.u32 [rd2+8], r1;\r\nSkip:";
                        regParams.Add(name);
                    }
                    else
                    {
                        benchmarkHeader += "\r\n\t .reg .b" + size + " " + name + ";";
                        benchmarkHeader += "\r\n\t mov.u32 " + name + ", %clock;";
                        if (srcOrDesk == "d")
                            benchmarkTail += "\r\n\t st.global.u32 [rd2+8], " + name + ";";
                        regParams.Add(name);
                    }
                }
                
            }

            if (timingsEnabled)
            {
                inst.ticks = ct.Benchmark(benchmarkHeader, line, benchmarkTail) - benchmarkBase;
                Console.WriteLine("Ticks: " + inst.ticks + " for " + line);
            }

            // Is there a "!" in front of the 3rd input?
            inst.notInFrontOfInput3 = Regex.IsMatch(inst.lineString, @", ?!");

            // Let's strip away the opps behind the instruction.   ie: the "d32, s32, s32"
            inst.lineString = Regex.Replace(inst.lineString, @" .*","");

            // Add the instruction to the instruction list.
            insts.Add(inst);
        }


        public Inst[] GetInstructionsAsArray()
        {
            return insts.ToArray();
        }


        public void SaveInstructionsToCSFile(string fileName)
        { 
            //// Read in the sharedclasses.cs file to outfile variable////
            StreamReader sr = new StreamReader(@"..\..\SharedClasses.cs");
            StringBuilder outfile = new StringBuilder(sr.ReadToEnd());
            sr.Close();

            //// Do the categories replacement ////
            StringBuilder catsOutput = new StringBuilder();
            foreach (InstCat ic in instCats)
                catsOutput.AppendLine(@"new InstCat(" + ic.ID + "," + ic.count + "," + ic.popularity + ",\"" + ic.desc +"\"),");
            outfile.Replace(@"/*INSERT CATS HERE*/", catsOutput.ToString()); 

            //// Do the instructions replacement ////
            StringBuilder instsOutput = new StringBuilder();
            foreach (Inst inst in insts)
            {
                instsOutput.AppendLine(@"new Inst(" + inst.id
                    + "," + inst.catNum
                    + "," + inst.popularity
                    + "," + inst.ticks / 3
                    + ",\"" + inst.lineString
                    + "\",VarType." + inst.outputType
                    + "," + inst.inputs.Count);

                foreach (var input in inst.inputs)
                    instsOutput.AppendLine(", VarType." + input.ToString());

                instsOutput.AppendLine(", " + inst.notInFrontOfInput3.ToString().ToLower() + "),");
            }
            outfile.Replace(@"/*INSERT INSTS HERE*/", instsOutput.ToString()); 

            outfile.Replace(@"1000/*INST COUNT*/", insts.Count.ToString()); 

            //// Cleanup - remove "," in ",};"
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
            foreach (InstCat instclass in instCats)
            {
                textWriter.WriteStartElement("Class" );
                textWriter.WriteAttributeString("id", instclass.ID.ToString());
                textWriter.WriteAttributeString("popularity", instclass.popularity.ToString());
                textWriter.WriteAttributeString("count", instclass.count.ToString());
                textWriter.WriteAttributeString("text", instclass.desc.ToString());
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
                foreach (VarType input in inst.inputs)
                {
                    textWriter.WriteStartElement("Inputs");
                    textWriter.WriteValue(input.ToString());
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
            }
            textWriter.WriteEndDocument();
            textWriter.Close();
        }


        public Inst[] GetInstructionsAsArray(int inputcount)
        {
            var filteredInstruction = from n in insts
            //where n.src.FirstOrDefault().type == RegisterType.f32 
            //  || n.src.FirstOrDefault().type == RegisterType.s32
            where n.inputs.Count == inputcount
            select n;
            return filteredInstruction.ToArray();
        }


        string MultipleReplace(string text, Dictionary<string, string> replacements)
        {
            return Regex.Replace(text,
                "(" + string.Join("|", replacements.Keys.ToArray()) + ")",
                delegate(Match m) { return replacements[m.Value]; });
        }
    }
} // end namespace
