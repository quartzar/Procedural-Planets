// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class BinaryTree
// {
//     private int Bitfield { get; set; }
//     private int MaxDepth { get; }
//
//     public BinaryTree(int maxDepth)
//     {
//         MaxDepth = maxDepth;
//         Bitfield = 1 << (1 << maxDepth - 1);
//     }
//
//     // Utility function to find the least significant bit of x.
//     private int FindLSB(int x)
//     {
//         int lsb = 0;
//         while ((x & 1) == 0)
//         {
//             x >>= 1;
//             lsb++;
//         }
//         return lsb;
//     }
//
//     // Utility function to find the most significant bit of x.
//     private int FindMSB(int x)
//     {
//         int msb = 0;
//         while (x > 1)
//         {
//             x >>= 1;
//             msb++;
//         }
//         return msb;
//     }
//
//     // Convert bit index to heap index.
//     public int BitToHeapIndex(int x)
//     {
//         int N = x > 0 ? FindLSB(x) + 1 : MaxDepth + 1;
//         int N0 = N == MaxDepth + 1 ? 0 : (x >> N) << N;
//         int d = MaxDepth - (int)Mathf.Log(1 + N0, 2);
//         int k = (1 << (MaxDepth - d)) + (N0 >> (MaxDepth - d));
//         return k;
//     }
//
//     // Convert heap index to bit index.
//     public int HeapToBitIndex(int k)
//     {
//         int dk = FindMSB(k);
//         int x = k * (1 << (MaxDepth - dk)) - (1 << MaxDepth);
//         return x;
//     }
//
//     // Split a node.
//     public void SplitNode(int k)
//     {
//         int x = HeapToBitIndex(2 * k + 1);
//         Bitfield |= (1 << x);
//     }
//
//     // Merge nodes.
//     public void MergeNodes(int d)
//     {
//         int x = HeapToBitIndex((1 << d) + 1);
//         Bitfield &= ~(1 << x);
//     }
// }