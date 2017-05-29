using System.Collections.Generic;

namespace ListExt
{

    public static class ListExtensions
    {
        /**Reverse RLE's a list*/
        public static List<int> ReverseRLE(this List<int> list) {
            List<int> unRLE = new List<int>();
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == 127) {
                    if (list[i + 2] == 1)
                        unRLE.Add(127);
                    else
                        for (int x = 0; x < (int)list[i + 2]; x++)
                            unRLE.Add(list[i + 1]);
                    i += 2;
                }
                else
                    unRLE.Add(list[i]);
            }
            return unRLE;
        }

        /**RLE's a list of values*/
        public static List<int> RLE(this List<int> list) {
            List<int> ret = new List<int>();
            for (int i = 0; i < list.Count; i++) {
                int count = 1;
                while (i < list.Count-1 - 1 && list[i] == list[i + 1]) {
                    i++;
                    count++;
                }
                if (count > 3 || list[i] == 127) { //run
                    ret.Add(127);
                    ret.Add(list[i]);
                    ret.Add(count);
                }
                else {
                    for (int x = 0; x < count; x++)
                        ret.Add(list[i]);
                }
            }
            return ret;
        }
    }
}
