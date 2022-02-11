using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TwitterScraper.Nitter
{
    internal interface NitterInterface
    {
        /// <summary>
        /// nitter:latest
        /// </summary>
        void DockerCompose();
    }
}
