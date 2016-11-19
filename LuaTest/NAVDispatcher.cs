using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaTest
{
    class NAVDispatcher
    {
        public string DispatchStringCall(string functionName, object[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Dispatching {0} (", functionName);
            foreach (object arg in args)
            {
                sb.AppendFormat("{0} {1}; ", arg.GetType(), arg.ToString());
            }
            sb.Append(")");
            return sb.ToString();
        }

        public int DispatchIntCall(string functionName, object[] args)
        {
            int result = 0;
            
            foreach (object arg in args)
            {
                if (arg.GetType() == typeof(System.Int32))
                {
                    switch (functionName)
                    {
                        case "mul":
                            result = (result == 0 ? 1 : result) * (int) arg;
                            break;

                        case "add":
                            result += (int)arg;
                            break;

                        default:
                            throw new InvalidOperationException("Unknown Function to Dispatch");
                    }
                }
                else if (arg.GetType() == typeof(System.Double))
                {
                    switch (functionName)
                    {
                        case "mul":
                            result = (result == 0 ? 1 : result) * (int)Convert.ToInt32((double) arg);
                            break;

                        case "add":
                            result += (int)Convert.ToInt32((double)arg);
                            break;

                        default:
                            throw new InvalidOperationException("Unknown Function to Dispatch");
                    }
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Type {0} not supported", arg.GetType()));
                }
            }
            
            return result;
        }

    }
}
