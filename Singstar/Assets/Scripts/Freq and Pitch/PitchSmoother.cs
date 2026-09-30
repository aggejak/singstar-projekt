public class PitchSmoother
{
    private float older;
    private float previous;
    private int historyCount;

    public float Process(float frequency)
    {
        // Ingen giltig ton: glöm tidigare mätningar.
        if (frequency <= 0f ||
            float.IsNaN(frequency) ||
            float.IsInfinity(frequency))
        {
            Reset();
            return 0f;
        }

        float result = frequency;

        // Beräkna median när vi har två tidigare värden.
        if (historyCount >= 2)
        {
            float a = older;
            float b = previous;
            float c = frequency;

            // Sortera kopiorna så att a <= b <= c.
            if (a > b)
            {
                float temp = a;
                a = b;
                b = temp;
            }

            if (b > c)
            {
                float temp = b;
                b = c;
                c = temp;
            }

            if (a > b)
            {
                float temp = a;
                a = b;
                b = temp;
            }

            result = b;
        }

        // Spara råvärdena i tidsordning.
        older = previous;
        previous = frequency;

        if (historyCount < 2)
            historyCount++;

        return result;
    }

    public void Reset()
    {
        older = 0f;
        previous = 0f;
        historyCount = 0;
    }
}