using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OceanProxy.ServiceHost.ViewModel.ProxyController;

namespace OceanProxy.ServiceHost.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ProxyController : ControllerBase
    {
        private readonly ILogger<ProxyController> _logger;

        public ProxyController(ILogger<ProxyController> logger)
        {
            _logger = logger;
        }

        public async Task<List<AddProxyService_Result>> AddProxyService(AddProxyService_Param param)
        {
            if (param == null || param.PortList.Count() <= 0)
            {
                return null;
            }
            List<AddProxyService_Result> list = new List<AddProxyService_Result>();
            foreach(var temp in param.PortList)
            {
                var port = GlobalPort.GetRandomPrivatePort();
                list.Add(new AddProxyService_Result() {PublicPort=temp,PrivatePort = port });
                //var serverPortListener = new ServerPortListener(temp, port);
               // await serverPortListener.StartListenerAsync();
            }

            return list;
        }
       
    }
}
