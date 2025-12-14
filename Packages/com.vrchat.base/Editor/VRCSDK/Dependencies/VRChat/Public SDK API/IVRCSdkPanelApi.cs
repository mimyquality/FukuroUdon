using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace VRC.SDKBase.Editor
{
    /// <summary>
    /// This is the public interface you, as a user of the SDK, can use to interact with the SDK Panel
    /// </summary>
    public interface IVRCSdkPanelApi
    {
        // Unfortunately C# 7.3 doesnt allow static members in interfaces, so we're leaving them here for pure reference
        // SDK Panel Events
        // event EventHandler OnSdkPanelEnable;
        // event EventHandler OnSdkPanelDisable;

        // event EventHandler<SdkPanelState> OnSdkPanelStateChange;
        // SdkPanelState PanelState { get; }
        
        // SDK Panel Methods

        /// <summary>
        /// Shows a dismissible notification at the bottom of the builder panel
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="titleColor">A color class, e.g. `green`</param>
        /// <param name="timeout">If timeout is greater than 0 - the notification will auto-dismiss after the given timeout</param>
        /// <returns></returns>
        Task ShowBuilderNotification(string title, VisualElement content, string titleColor, int timeout = 0);
        Task DismissNotification();

        // ReSharper disable once InvalidXmlDocComment
        /// <summary>
        /// Get a reference to a builder of a particular Interface type (e.g. IVRCSdkAvatarBuilderApi)
        ///
        /// As of C# 7.3 - we cannot define static members in interfaces, but the method does exist on the class 
        /// </summary>
        /// <param name="builder">Desired builder or null</param>
        /// <typeparam name="T">Builder to look up, e.g. IVRCSdkAvatarBuilderApi</typeparam>
        /// <returns>Whether the builder was found</returns>
        // static bool TryGetBuilder<T>(out T builder) where T : IVRCSdkBuilderApi;
    }
    
    public enum SdkPanelState
    {
        Idle,
        Building,
        Uploading
    }
}