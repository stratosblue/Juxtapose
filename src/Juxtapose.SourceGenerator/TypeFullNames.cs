namespace Juxtapose.SourceGenerator;

public static class TypeFullNames
{
    #region Public 类

    public static class Juxtapose
    {
        #region Public 字段

        public const string ConstantMessageCodecFactory = "global::Juxtapose.ConstantMessageCodecFactory";
        public const string ExecutorCreationContext = "global::Juxtapose.ExecutorCreationContext";
        public const string ICommunicationMessageCodecFactory = "global::Juxtapose.ICommunicationMessageCodecFactory";
        public const string IJuxtaposeExecutorOwner = "global::Juxtapose.IJuxtaposeExecutorOwner";
        public const string IMessageExchanger = "global::Juxtapose.IMessageExchanger";
        public const string IMessageExecutor = "global::Juxtapose.IMessageExecutor";
        public const string JuxtaposeContext = "global::Juxtapose.JuxtaposeContext";
        public const string JuxtaposeContext_NoGlobal = "Juxtapose.JuxtaposeContext";
        public const string JuxtaposeExecutor = "global::Juxtapose.JuxtaposeExecutor";
        public const string MessageDispatcher = "global::Juxtapose.MessageDispatcher";

        #endregion Public 字段

        #region Public 类

        public static class Messages
        {
            #region Public 字段

            public const string CreateObjectInstanceMessage = "global::Juxtapose.Messages.CreateObjectInstanceMessage";
            public const string DisposeObjectInstanceMessage = "global::Juxtapose.Messages.DisposeObjectInstanceMessage";
            public const string ExceptionMessage = "global::Juxtapose.Messages.ExceptionMessage";
            public const string IInstanceMessage = "global::Juxtapose.Messages.IInstanceMessage";
            public const string InstanceMethodInvokeMessage = "global::Juxtapose.Messages.InstanceMethodInvokeMessage";
            public const string InstanceMethodInvokeResultMessage = "global::Juxtapose.Messages.InstanceMethodInvokeResultMessage";
            public const string JuxtaposeAckMessage = "global::Juxtapose.Messages.JuxtaposeAckMessage";
            public const string JuxtaposeMessage = "global::Juxtapose.Messages.JuxtaposeMessage";

            //public const string MethodInvokeMessage = "global::Juxtapose.Messages.MethodInvokeMessage";
            //public const string MethodInvokeResultMessage = "global::Juxtapose.Messages.MethodInvokeResultMessage";
            public const string StaticMethodInvokeMessage = "global::Juxtapose.Messages.StaticMethodInvokeMessage";

            public const string StaticMethodInvokeResultMessage = "global::Juxtapose.Messages.StaticMethodInvokeResultMessage";

            #endregion Public 字段
        }

        public static class SourceGenerator
        {
            #region Public 字段

            public const string IllusionAttribute = "global::Juxtapose.SourceGenerator.IllusionAttribute";

            public const string IllusionAttribute_NoGlobal = "Juxtapose.SourceGenerator.IllusionAttribute";

            #endregion Public 字段
        }

        #endregion Public 类
    }

    #endregion Public 类

    public static class System
    {
        #region Public 字段

        public const string IDisposable = "global::System.IDisposable";

        #endregion Public 字段

        #region Public 类

        public static class Threading
        {
            #region Public 字段

            public const string CancellationToken = "global::System.Threading.CancellationToken";

            #endregion Public 字段

            #region Public 类

            public static class Tasks
            {
                #region Public 字段

                public const string Task = "global::System.Threading.Tasks.Task";
                public const string ValueTask = "global::System.Threading.Tasks.ValueTask";

                #endregion Public 字段
            }

            #endregion Public 类
        }

        #endregion Public 类
    }
}