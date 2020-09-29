namespace Unity.Mathematics
{
    public static class float2x2Extensions
    {
        public static float2 TranslateFloat2(this float2x2 Mat2, float2 ogFloat2)
        {
            float newX = Mat2.c0.x * ogFloat2.x + Mat2.c0.y * ogFloat2.y;
            float newY = Mat2.c1.x * ogFloat2.x + Mat2.c1.y * ogFloat2.y;

            return new float2(newX, newY);
        }
        //sadly operators are not possible for extension classes
        //public static float2  operator* (this float2x2 Mat2, float2 originalFloat2)
        //{
        //}
    }
}