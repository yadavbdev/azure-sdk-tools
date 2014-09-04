using Microsoft.Azure.Management.Redis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.RedisCache.Models
{
    public static class SizeConverter
    {
        public const string C0 = "250MB";
        public const string C1 = "1GB";
        public const string C2 = "2.5GB";
        public const string C3 = "6GB";
        public const string C4 = "13GB";
        public const string C5 = "26GB";
        public const string C6 = "53GB";

        public const string C0String = "C0";
        public const string C1String = "C1";
        public const string C2String = "C2";
        public const string C3String = "C3";
        public const string C4String = "C4";
        public const string C5String = "C5";
        public const string C6String = "C6";

        public static string GetSizeInRedisSpecificFormat(string actualSizeFromUser)
        { 
            switch(actualSizeFromUser)
            {
                // accepting actual sizes
                case C0:
                    return C0String;
                case C1:
                    return C1String;
                case C2:
                    return C2String;
                case C3:
                    return C3String;
                case C4:
                    return C4String;
                case C5:
                    return C5String;
                case C6:
                    return C6String;

                // accepting C0, C1 etc.
                case C0String:
                    return C0String;
                case C1String:
                    return C1String;
                case C2String:
                    return C2String;
                case C3String:
                    return C3String;
                case C4String:
                    return C4String;
                case C5String:
                    return C5String;
                case C6String:
                    return C6String;
                
                default:
                    return C1String;
            }
        }

        public static string GetSizeInUserSpecificFormat(string skuFamily, int skuCapacity)
        {
            string sizeConstant = skuFamily + skuCapacity.ToString();
            switch (sizeConstant)
            {
                // accepting C0, C1 etc.
                case C0String:
                        return C0;
                case C1String:
                        return C1;
                case C2String:
                        return C2;
                case C3String:
                        return C3;
                case C4String:
                        return C4;
                case C5String:
                        return C5;
                case C6String:
                        return C6;

                default:
                    return C1;
            }
        }
    }
}
