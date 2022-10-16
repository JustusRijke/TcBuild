/*
Implementation of the OLE IOleMessageFilter interface.

The message filter is needed to handle blocking COM method calls than can result in RPC_E_CALL_REJECTED errors.

More information:
- https://infosys.beckhoff.com/english.php?content=../content/1033/tc3_automationinterface/54043195771173899.html
- https://learn.microsoft.com/en-us/windows/win32/api/objidl/nn-objidl-imessagefilter
*/

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;

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

public class MessageFilter : IDisposable, IOleMessageFilter
{
    [DllImport("Ole32.dll")]
    static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);

    // To detect redundant calls
    private bool _disposedValue;
    private IOleMessageFilter oldFilter;

    internal MessageFilter()
    {
        MessageFilter.CoRegisterMessageFilter(this, out this.oldFilter);
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose() => Dispose(true);

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                IOleMessageFilter dummy;
                MessageFilter.CoRegisterMessageFilter(this.oldFilter, out dummy);
            }
            _disposedValue = true;
        }
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
        return 1000; // Retry after 1 second
    }
}
