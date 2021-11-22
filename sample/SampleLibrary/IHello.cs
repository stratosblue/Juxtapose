namespace SampleLibrary
{
    public interface IHello
    {
        #region Public 方法

        /// <summary>
        /// say
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<string> SayAsync(string name);

        /// <summary>
        /// where
        /// </summary>
        /// <returns></returns>
        string Where();

        #endregion Public 方法
    }
}