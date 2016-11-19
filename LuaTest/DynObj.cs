using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace LuaTest
{
    class DynObj : DynamicObject
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return new string[] {
                "GetTableValue",
                "ProcessUsername",
                "Lorem",
                "Ipsum"
            };
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return dictionary.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            dictionary[binder.Name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            // Console.WriteLine("TryInvokeMember for {0}; Arg Count: {1}", binder.Name, binder.CallInfo.ArgumentCount);
     
            switch (binder.Name)
            {
                case "GetTableValue":
                    int tableID = (int) args[0];
                    int fieldID = (int) args[1];
                    result = String.Format("GetTableValue Table ID {0}, Field ID {1}", tableID, fieldID);
                    return true;

                case "ProcessUsername":
                    string userID = (string) args[0];
                    result = String.Format("ProcessUsername UserID {0}", userID);
                    return true;
            }

            result = null;
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (binder.CallInfo.ArgumentCount == 1)
            {
                result = String.Format("Index Lookup for {0}", indexes[0]);
                return true;
            }
            else if (binder.CallInfo.ArgumentCount == 2)
            {
                result = String.Format("Index Lookup for {0},{1}", indexes[0], indexes[1]);
                return true;
            }

            result = null;
            return false;
        }
    }
}
