using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

static class ArrayExtensions
{

    /*
        reference implementation from c++ std lib
        
        template <class BidirectionalIterator, class UnaryPredicate>
        BidirectionalIterator partition (BidirectionalIterator first,
                                        BidirectionalIterator last, UnaryPredicate pred)
        {
        while (first!=last) {
            while (pred(*first)) {
            ++first;
            if (first==last) return first;
            }
            do {
            --last;
            if (first==last) return first;
            } while (!pred(*last));
            swap (*first,*last);
            ++first;
        }
        return first;
        }
    
     */
    public static int Partition<T>(this T[] a, Func<T, bool> predicate)
    {
        int first = 0;
        int last = a.Length;

        while (first != last)
        {
            while (predicate(a[first]))
            {
                ++first;
                if (first == last) return first;
            }
            do
            {
                --last;
                if (first == last)
                {
                    return first;
                }
            } while (!predicate(a[last]));

            T t = a[first];
            a[first] = a[last];
            a[last] = t;

            ++first;
        }

        return first;
    }

    public static void TestPartition()
    {
        int[] values = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        int v = values.Partition((j) =>
        {
            return j % 2 == 0;
        });
        if (v != 5)
        {
            Debug.LogErrorFormat("[ArrayExtensions::TestPartition] - Expect first odd value at 5, got: {0}", v);
        }

        values = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        v = values.Partition((j) =>
        {
            return j == 3;
        });
        if (v != 1)
        {
            Debug.LogErrorFormat("[ArrayExtensions::TestPartition] - Expect values != 3 to start at position 1, got: {0}", v);
        }

        values = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        v = values.Partition((j) =>
        {
            return j == 10;
        });
        if (v != 0)
        {
            Debug.LogErrorFormat("[ArrayExtensions::TestPartition] - Expect partition point to be at beginning of array since value 10 doesn't show, got: {0}", v);
        }
    }
}