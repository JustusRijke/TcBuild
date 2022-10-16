/*
Implementation of the OLE IOleMessageFilter interface.

The message filter is needed to handle blocking COM method calls than can result in RPC_E_CALL_REJECTED errors.

More information / example implementations:
- https://infosys.beckhoff.com/english.php?content=../content/1033/tc3_automationinterface/54043195771173899.html
- https://learn.microsoft.com/en-us/windows/win32/api/objidl/nn-objidl-imessagefilter
*/


using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
/*

public class OleMessageFilter : IOleMessageFilter
{
    [DllImport("Ole32.dll")]
    static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);

    // Start the filter.
    public static void Register()
    {
        IOleMessageFilter newFilter = new OleMessageFilter();
        IOleMessageFilter oldFilter = null;
        Marshal.ThrowExceptionForHR(CoRegisterMessageFilter(newFilter: newFilter, oldFilter: out oldFilter));
    }

    // Done with the filter, close it.
    public static void Revoke()
    {
        IOleMessageFilter oldFilter = null;
        Marshal.ThrowExceptionForHR(CoRegisterMessageFilter(newFilter: null, oldFilter: out oldFilter));
    }


*/
[ComImport]
[Guid("00000016-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IOleMessageFilter
{
    [PreserveSig]
    int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);

    [PreserveSig]
    int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);

    [PreserveSig]
    int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);
}

public class MessageFilter : IOleMessageFilter
{
    [DllImport("Ole32.dll")]
    static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);

    // Start the filter.
    public static void Register()
    {
        IOleMessageFilter newFilter = new MessageFilter();
        IOleMessageFilter oldFilter = null;
        Marshal.ThrowExceptionForHR(CoRegisterMessageFilter(newFilter: newFilter, oldFilter: out oldFilter));
    }

    // Done with the filter, close it.
    public static void Revoke()
    {
        IOleMessageFilter oldFilter = null;
        Marshal.ThrowExceptionForHR(CoRegisterMessageFilter(newFilter: null, oldFilter: out oldFilter));
    }


    // Handle incoming thread requests.
    // https://learn.microsoft.com/en-us/windows/win32/api/objidl/nf-objidl-imessagefilter-handleincomingcall
    int IOleMessageFilter.HandleInComingCall(int dwCallType, System.IntPtr hTaskCaller, int dwTickCount, System.IntPtr lpInterfaceInfo)
    {
        return (int)SERVERCALL.SERVERCALL_ISHANDLED;
    }


    // Indicates that a message has arrived while COM is waiting to respond to a remote call.
    // https://learn.microsoft.com/en-us/windows/win32/api/objidl/nf-objidl-imessagefilter-messagepending
    int IOleMessageFilter.MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType)
    {
        return (int)PENDINGMSG.PENDINGMSG_WAITDEFPROCESS;
    }
    // Thread call was rejected, so try again.
    // https://learn.microsoft.com/en-us/windows/win32/api/objidl/nf-objidl-imessagefilter-retryrejectedcall
    int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
    {
        //   return (dwRejectType == (int)SERVERCALL.SERVERCALL_RETRYLATER) ? 99 : -1;
        if (dwRejectType == 2)
        // flag = SERVERCALL_RETRYLATER.
        {
            // Retry the thread call immediately if return >=0 & 
            // <100.
            return 99;
        }
        // Too busy; cancel call.
        return -1;
    }
}
