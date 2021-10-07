using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments.Vst
{

    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<VstPlugin>))]
    [DataContract]
    public class VstPlugin : IDisposable
    {

        private VstPluginContextWrapper f_PluginContext;

        [Browsable(false)]
        public VstPluginContextWrapper PluginContext
        {
            get
            {
                return f_PluginContext;
            }
        }

        private string f_PluginPath;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("VST")]
        [Description("Set VST plugin(32bit) DLL path.\r\n" +
            "★★ WARNING ★★\r\n" +
            "Remember, this setting is for current PC environment. So, almost users can not use this VST.")]
        [EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        [RefreshProperties(RefreshProperties.All)]
        public string PluginPath
        {
            get
            {
                return f_PluginPath;
            }
            set
            {
                if (f_PluginPath != value)
                {
                    f_PluginPath = value;

                    lock (InstrumentBase.VstPluginContextLockObject)
                    {
                        var octx = f_PluginContext;
                        if (octx != null)
                        {
                            octx.Dispose();
                            f_PluginContext = null;
                            f_EffectPresetName = null;
                            f_Settings = null;
                        }
                    }
                    var ctx = OpenPlugin(f_PluginPath);
                    if (ctx != null)
                    {
                        lock (InstrumentBase.VstPluginContextLockObject)
                        {
                            f_PluginContext = ctx;
                            EffectPresetName = ctx.Context.PluginCommandStub.GetProgramNameIndexed(0);
                            Settings = ctx.VstSettings;
                        }
                    }
                }
            }
        }

        public bool ShouldSerializePluginPath()
        {
            return !string.IsNullOrEmpty(PluginPath);
        }

        public void ResetPluginPath()
        {
            PluginPath = null;
        }

        private string f_EffectPresetName;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("VST")]
        [Description("Set VST plugin preset")]
        [Editor(typeof(UITypeEditorVstPresetDropDown), typeof(UITypeEditor))]
        public string EffectPresetName
        {
            get
            {
                return f_EffectPresetName;
            }
            set
            {
                if (f_EffectPresetName != value)
                {
                    f_EffectPresetName = value;
                    var ctx = f_PluginContext;
                    if (ctx != null)
                    {
                        for (int i = 0; i < ctx.Context.PluginInfo.ProgramCount; i++)
                        {
                            if (value == null || // "null": set to default( No.1 )
                                value.Equals(ctx.Context.PluginCommandStub.GetProgramNameIndexed(i), StringComparison.Ordinal))
                            {
                                ctx.Context.PluginCommandStub.SetProgram(i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public bool ShouldSerializeEffectPresetName()
        {
            return !string.IsNullOrEmpty(PluginPath);
        }

        public void ResetEffectPresetName()
        {
            PluginPath = null;
        }

        private VstSettings f_Settings;

        [Category("VST")]
        [DataMember]
        [Description("Set VST plugin parameters")]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(VstSettingsJsonConverter<VstSettings>))]
        public VstSettings Settings
        {
            get
            {
                return f_Settings;
            }
            set
            {
                f_Settings = value;

                lock (InstrumentBase.VstPluginContextLockObject)
                {
                    var ctx = PluginContext;
                    if (ctx != null)
                    {
                        ctx.VstSettings = value;
                        foreach (var kvp in ctx.VstParameterIndexes)
                        {
                            float val = (float)f_Settings.GetPropertyValue(kvp.Key);
                            ctx.Context.PluginCommandStub.SetParameter(kvp.Value, val);
                        }
                        f_Settings.VstPluginContext = ctx;
                    }
                }
            }
        }

        [DataMember]
        [Description("VST Effect Control Change System-wide Settings  <MIDI 16ch>\r\n" +
    "Link Data Entry message value with the VST property value\r\n" +
    "eg) \"Reverb,Chorus\" ... You can change Reverb and Chorus depth property values dynamically via MIDI Control Change No.9x message.")]
        [DisplayName("VST Effect Control Change System-wide Settings[VECCSS]")]
        public VstEffectControlChangeSettings[] VECCSS
        {
            get;
            set;
        }

        public bool ShouldSerializeVECCSS()
        {
            foreach (var dt in VECCSS)
            {
                if (dt.VstEffectSoundControl1 != null ||
                    dt.VstEffectSoundControl2 != null ||
                    dt.VstEffectSoundControl3 != null ||
                    dt.VstEffectSoundControl4 != null ||
                    dt.VstEffectSoundControl5 != null)
                    return true;
            }
            return false;
        }

        public void ResetVECCSS()
        {
            for (int i = 0; i < VECCSS.Length; i++)
            {
                VECCSS[i].VstEffectSoundControl1 = null;
                VECCSS[i].VstEffectSoundControl2 = null;
                VECCSS[i].VstEffectSoundControl3 = null;
                VECCSS[i].VstEffectSoundControl4 = null;
                VECCSS[i].VstEffectSoundControl5 = null;
            }
        }

        private VstPluginContextWrapper OpenPlugin(string pluginPath)
        {
            try
            {
                if (File.Exists(pluginPath))
                {
                    HostCommandStub hostCmdStub = new HostCommandStub();
                    VstPluginContext ctx = VstPluginContext.Create(pluginPath, hostCmdStub);
                    if (ctx.PluginInfo.AudioInputCount != 2)
                        return null;
                    if (ctx.PluginInfo.AudioOutputCount != 2)
                        return null;
                    if ((ctx.PluginInfo.Flags & VstPluginFlags.CanReplacing) == 0)
                        return null;

                    hostCmdStub.PluginCalled += (s, e) =>
                    {
                        HostCommandStub stub = (HostCommandStub)s;

                        // can be null when called from inside the plugin main entry point.
                        if (stub.PluginContext.PluginInfo != null)
                        {
                            //Debug.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called:" + e.Message);
                        }
                        else
                        {
                            //Debug.WriteLine("The loading Plugin called:" + e.Message);
                        }
                    };

                    // add custom data to the context
                    ctx.Set("PluginPath", pluginPath);
                    ctx.Set("HostCmdStub", hostCmdStub);

                    // actually open the plugin itself
                    ctx.PluginCommandStub.Open();
                    ctx.PluginCommandStub.SetSampleRate(Program.CurrentSamplingRate);
                    ctx.PluginCommandStub.MainsChanged(true);
                    ctx.PluginCommandStub.StartProcess();
                    initVECCSS();
                    return new VstPluginContextWrapper(ctx);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                //ignore
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public VstPlugin()
        {
            initVECCSS();
        }

        private void initVECCSS()
        {
            VECCSS = new VstEffectControlChangeSettings[]{
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings(),
                new VstEffectControlChangeSettings()
            };
        }

        #region IDisposable Support

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                if (f_PluginContext != null)
                {
                    f_PluginContext.Dispose();
                    f_PluginContext = null;
                }

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~VstPlugin()
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
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

}
