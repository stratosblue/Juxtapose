namespace Juxtapose.Test
{
    //当前直接使用JuxtaposeEnvironment生成代理类会导致IntelliSense无法正常工作，故使用一个中间类进行生成

    public static class JuxtaposeEnvironmentProxy
    {
        #region Public 属性

        public static bool IsSubProcess => JuxtaposeEnvironment.IsSubProcess;

        #endregion Public 属性
    }
}