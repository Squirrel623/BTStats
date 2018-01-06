using System;
using System.Collections.Generic;
using System.Text;

namespace BTStatsCorePopulator
{
    public abstract class BaseLoggedInMetric
    {
        protected HashSet<string> loggedInUsers = new HashSet<string>(Users.InitialLoggedInUsers);
    }
}
