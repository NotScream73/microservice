using System.Diagnostics;

namespace Domain;

public class ParallelMergeSort
{
    public (int[], long) Sort(int[] array, int maxDegreeOfParallelism)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = ParallelMergeSortHelper(array, maxDegreeOfParallelism);
        stopwatch.Stop();
        return (result, stopwatch.ElapsedMilliseconds);
    }

    private int[] ParallelMergeSortHelper(int[] array, int maxDegreeOfParallelism)
    {
        if (array.Length <= 1)
            return array;

        int mid = array.Length / 2;

        if (maxDegreeOfParallelism > 1)
        {
            int[][] subArrays = new int[2][];
            Parallel.Invoke(
                () => subArrays[0] = ParallelMergeSortHelper(array.Take(mid).ToArray(), maxDegreeOfParallelism / 2),
                () => subArrays[1] = ParallelMergeSortHelper(array.Skip(mid).ToArray(), maxDegreeOfParallelism / 2)
            );
            return Merge(subArrays[0], subArrays[1]);
        }
        else
        {
            return Merge(
                ParallelMergeSortHelper(array.Take(mid).ToArray(), 1),
                ParallelMergeSortHelper(array.Skip(mid).ToArray(), 1)
            );
        }
    }

    private int[] Merge(int[] left, int[] right)
    {
        int[] result = new int[left.Length + right.Length];
        int i = 0, j = 0, k = 0;

        while (i < left.Length && j < right.Length)
        {
            if (left[i] > right[j])
                result[k++] = left[i++];
            else
                result[k++] = right[j++];
        }

        while (i < left.Length) result[k++] = left[i++];
        while (j < right.Length) result[k++] = right[j++];

        return result;
    }
}
