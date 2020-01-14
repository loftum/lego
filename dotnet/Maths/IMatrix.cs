namespace Maths
{
    public interface IMatrix
    {
        /// <summary>
        /// Row-major two-dimensional array
        /// </summary>
        /// <returns>this matrix as row-major two-dimensional array</returns>
        float[,] ToGrid();
    }
}