﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Jacobi.Vst.Core.Properties {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Jacobi.Vst.Core.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   すべてについて、現在のスレッドの CurrentUICulture プロパティをオーバーライドします
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   The value is not a valid four character code. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string FourCharacterCode_InvalidValue {
            get {
                return ResourceManager.GetString("FourCharacterCode_InvalidValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Could not find the managed VST plugin assembly with either the .net.dll or .net.vstdll extension. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string ManagedPluginFactory_FileNotFound {
            get {
                return ResourceManager.GetString("ManagedPluginFactory_FileNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   &quot;{0}&quot; does not expose a public class that implements the IVstPluginCommandStub interface. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string ManagedPluginFactory_NoPublicStub {
            get {
                return ResourceManager.GetString("ManagedPluginFactory_NoPublicStub", resourceCulture);
            }
        }
        
        /// <summary>
        ///   The argument is empty. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string Throw_ArgumentIsEmpty {
            get {
                return ResourceManager.GetString("Throw_ArgumentIsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   The value should lie between &apos;{0}&apos; and &apos;{1}&apos;. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string Throw_ArgumentNotInRange {
            get {
                return ResourceManager.GetString("Throw_ArgumentNotInRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   &apos;{0}&apos; is too long. Maximum length is {1} characters. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string Throw_ArgumentTooLong {
            get {
                return ResourceManager.GetString("Throw_ArgumentTooLong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   The Audio buffer is read-only. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string VstAudioBuffer_BufferNotWritable {
            get {
                return ResourceManager.GetString("VstAudioBuffer_BufferNotWritable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   The destination buffer is too small. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string VstAudioBuffer_BufferTooSmall {
            get {
                return ResourceManager.GetString("VstAudioBuffer_BufferTooSmall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cannot construct an event of type Unknown. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string VstEvent_InvalidEventType {
            get {
                return ResourceManager.GetString("VstEvent_InvalidEventType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   The specified eventType is not generic (deprecated). に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string VstGenericEvent_InvalidEventType {
            get {
                return ResourceManager.GetString("VstGenericEvent_InvalidEventType", resourceCulture);
            }
        }
    }
}
