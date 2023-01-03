namespace Leeve.Application.Questionnaires;

public static class ArrayExtensions
{
    public static IEnumerable<int> GenerateArrayOfNumbers(this int count, int start = 0)
    {
        var result = new int[count];
        Array.Copy(Enumerable.Range(start, count).ToArray(), result, count);

        var random = new Random();
        var length = result.Length;
        while (length > 1)
        {
            length--;
            var next = random.Next(length + 1);
            (result[next], result[length]) = (result[length], result[next]);
        }

        return result;
    }
}