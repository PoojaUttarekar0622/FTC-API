using Helper.Interface;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;


namespace Helper.Hub_Config
{
    public class SignalRHub : Hub<ITypedHubClient>
    {
    }
}
