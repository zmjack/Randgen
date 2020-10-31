﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Randgen
{
    public class StringGenerator : IGenerator<string>
    {
        public delegate void WarningDelegate(double afterProbability, string message);

        public string Format { get; private set; }
        public double AllowedProbability { get; private set; }
        public double MaxCount { get; private set; }
        public event WarningDelegate OnWarning;

        private readonly string[] CodeSegments;

        public StringGenerator(string format, double allowedProbability = 0.9)
        {
            if (allowedProbability < 0 || allowedProbability >= 1) throw new FormatException($"The specified parameter '{nameof(allowedProbability)}' must be greater then 0 and less than 1");

            Format = format;
            AllowedProbability = allowedProbability;
            CodeSegments = CalcCodeSegments(format);
            MaxCount = CalcMaxCount(CodeSegments);
        }

        public double AfterProbability(int count) => (MaxCount - count) / MaxCount;
        public double AfterProbability(int count, string[] excepts)
        {
            //TODO: In this version, hit rate is not accurate
            return (MaxCount - count - excepts.Length) / MaxCount;
        }

        public string[] Take(int count)
        {
            var afterProbability = AfterProbability(count);
            if (afterProbability < AllowedProbability)
            {
                OnWarning?.Invoke(afterProbability, "The probability of generation is too low after take.");
            }
            return Generate(count);
        }
        public string TakeOne() => Take(1)[0];

        public string[] Take(int count, string[] excepts)
        {
            var afterProbability = AfterProbability(count);
            if (afterProbability < AllowedProbability)
            {
                OnWarning?.Invoke(afterProbability, "The probability of generation is too low after take.");
            }

            var list = new List<string>();
            for (var n = count; list.Count < count; n = count - list.Count)
                list.AddRange(Take(n).Except(excepts.Concat(list)));

            return list.ToArray();
        }
        public string TakeOne(string[] excepts) => Take(1, excepts)[0];

        private string[] Generate(int count)
        {
            var random = new Random();
            var code = new byte[CodeSegments.Length];
            var sets = new HashSet<string>();

            if (count > MaxCount) throw new OverflowException();

            for (int recordAmount = 0; recordAmount < count;)
            {
                random.NextBytes(code);
                for (int i = 0; i < code.Length; i++)
                    code[i] = (byte)(code[i] % (byte)CodeSegments[i].Length);
                var generatedCode = new string(CodeSegments.Select((segment, i) => segment[code[i]]).ToArray());

                if (sets.Add(generatedCode))
                    recordAmount++;
            }

            return sets.ToArray();
        }

        private unsafe static bool CheckFormat(string format)
        {
            int bracketCount = 0;

            fixed (char* pformat = format)
            {
                char* p = pformat;
                while (*p != 0)
                {
                    if (*p == '$')
                    {
                        if (new[] { 'c', 'w', 'l', 'u', 'd', '[', ']', '$' }.Contains(*(p + 1)))
                            p += 2;
                        else return false;
                    }
                    else
                    {
                        if (*p == '[') bracketCount++;
                        else if (*p == ']') bracketCount--;

                        p++;
                    }

                    if (bracketCount < 0) return false;
                }
            }

            return true;
        }

        private unsafe static string[] CalcCodeSegments(string format)
        {
            if (!CheckFormat(format)) throw new FormatException("Can not analysis the specified format.");

            var segments = new List<string>();
            fixed (char* pformat = format)
            {
                var segment = new StringBuilder();
                var inBracket = false;

                char* p = pformat;
                while (*p != 0)
                {
                    if (*p == '$')
                    {
                        switch (*(p + 1))
                        {
                            case 'c': segment.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"); break;
                            case 'w': segment.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"); break;
                            case 'l': segment.Append("abcdefghijklmnopqrstuvwxyz"); break;
                            case 'u': segment.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ"); break;
                            case 'd': segment.Append("0123456789"); break;
                            case '[': segment.Append("["); break;
                            case ']': segment.Append("]"); break;
                            case '$': segment.Append("$"); break;
                        }
                        p += 2;
                    }
                    else if (*p == '[')
                    {
                        inBracket = true;
                        p++;
                    }
                    else if (*p == ']')
                    {
                        inBracket = false;
                        p++;
                    }
                    else segment.Append(*p++);

                    if (!inBracket)
                    {
                        segments.Add(new string(segment.ToString().ToCharArray().Distinct().ToArray()));
                        segment.Clear();
                    }
                }
            }
            return segments.ToArray();
        }

        private static double CalcMaxCount(string[] segments)
        {
            double maxCount = 1;
            foreach (var segment in segments)
                maxCount *= segment.Length;
            return maxCount;
        }

    }

}
