using Jacobi.Vst.Interop.Host;
using System;
using System.Collections.Generic;

namespace zanac.MAmidiMEmo.Instruments.Vst
{
    public class VstPluginContextWrapper : IDisposable
    {
        private VstPluginContext f_Context;

        public VstPluginContext Context
        {
            get
            {
                return f_Context;
            }
        }

        public Dictionary<string, int> VstParameterIndexes
        {
            get;
            private set;
        }

        public VstSettings VstSettings
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vstPluginContext"></param>
        public VstPluginContextWrapper(VstPluginContext vstPluginContext)
        {
            f_Context = vstPluginContext;

            VstParameterIndexes = new Dictionary<string, int>();
            var vst = new VstSettings();
            vst.VstPluginContext = this;

            for (int i = 0; i < vstPluginContext.PluginInfo.ParameterCount; i++)
            {
                string name = vstPluginContext.PluginCommandStub.GetParameterName(i);
                float val = vstPluginContext.PluginCommandStub.GetParameter(i);
                vst.SetPropertyValue(name, val);
                VstParameterIndexes.Add(name, i);
            }

            VstSettings = vst;
        }

        #region IDisposable Support

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                lock (InstrumentBase.VstPluginContextLockObject)
                {
                    if (f_Context != null)
                    {
                        f_Context.Dispose();
                        f_Context = null;
                    }
                }

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~VstPluginContextWrapper()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
