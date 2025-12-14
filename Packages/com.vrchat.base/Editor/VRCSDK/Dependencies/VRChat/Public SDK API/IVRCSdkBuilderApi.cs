using System;
using System.Threading.Tasks;

namespace VRC.SDKBase.Editor
{
    /// <summary>
    /// This is the public interface you, as a user of the SDK, can use to interact with the SDK Builder
    /// </summary>
    public interface IVRCSdkBuilderApi: IVRCSdkControlPanelBuilder
    {
        // Build Events
        event EventHandler<object> OnSdkBuildStart;
        event EventHandler<string> OnSdkBuildProgress;
        event EventHandler<string> OnSdkBuildFinish;
        event EventHandler<string> OnSdkBuildSuccess;
        event EventHandler<string> OnSdkBuildError;

        event EventHandler<SdkBuildState> OnSdkBuildStateChange;
        SdkBuildState BuildState { get; }

        // Upload Events
        event EventHandler OnSdkUploadStart;
        event EventHandler<(string status, float percentage)> OnSdkUploadProgress;
        event EventHandler<string> OnSdkUploadFinish;
        event EventHandler<string> OnSdkUploadSuccess;
        event EventHandler<string> OnSdkUploadError;

        event EventHandler<SdkUploadState> OnSdkUploadStateChange;
        SdkUploadState UploadState { get; }

        // Shorthand methods
        // for shorthand build and upload methods check the Avatars and Worlds SDKs respectively
        void CancelUpload();
    }
    
    public enum SdkBuildState
    {
        Idle,
        Building,
        Success,
        Failure
    }

    public enum SdkUploadState
    {
        Idle,
        Uploading,
        Success,
        Failure
    }
}
