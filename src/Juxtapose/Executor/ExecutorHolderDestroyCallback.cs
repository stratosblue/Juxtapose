namespace Juxtapose
{
    /// <summary>
    /// <see cref="IJuxtaposeExecutorHolder"/> 需要销毁时的回调
    /// </summary>
    /// <param name="identifier"></param>
    public delegate void ExecutorHolderDestroyCallback(string identifier);
}