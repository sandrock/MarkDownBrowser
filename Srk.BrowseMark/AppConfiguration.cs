
namespace Srk.BrowseMark
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class AppConfiguration
    {
        private static AppConfiguration defaultConfiguration;

        private ushort minimumTcpPort = 18001;
        
        public static AppConfiguration Default
        {
            get { return defaultConfiguration ?? (defaultConfiguration = new AppConfiguration()); }
        }

        public ushort MinimumTcpPort
        {
            get { return this.minimumTcpPort; }
            set { this.minimumTcpPort = value; }
        }
    }
}
