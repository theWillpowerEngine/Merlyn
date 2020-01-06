using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;
using Shiro.Guts;
using Shiro.Interop;
using System.Threading;

namespace Shiro
{
    public partial class Interpreter
    {
        public static string Version = "0.2.6";
        internal Symbols Symbols;
        internal Loader Loader = new Loader();

        private static ThreadConduit _tc = new ThreadConduit();
        internal static ThreadConduit Conduit
        {
            get
            {
                if (_tc == null)
                    _tc = new ThreadConduit();
                return _tc;
            }
        }
        public void CleanUpQueues()
        {
            _tc = new ThreadConduit();
            PublishedThings = new List<PublishedThing>();
        }

        public Interpreter()
        {
            Symbols = new Symbols(this);
        }
        internal Interpreter(Symbols s)
        {
            Symbols = new Symbols(this);
            Symbols.CloneFrom(s);
        }

        public static Action<string> Error = (msg) =>
        {
            throw new ApplicationException(msg);
        };

		public static Action<string> Output = s =>
		{
			Console.WriteLine(s);
		};

        public static Func<Interpreter, string, bool> LoadModule = DefaultModuleLoader;

        public static bool DefaultModuleLoader(Interpreter m, string s)
        {
            if (!File.Exists(s) && !File.Exists(s + ".shr"))
                return false;

            string code = "";
            if (File.Exists(s))
                code = File.ReadAllText(s);
            else
                code = File.ReadAllText(s + ".shr");

            m.InnerEval(code);
            return true;
        }

        private class PublishedThing
        {
            internal Token Eval, Val;
            internal string DeliverTo;
            internal Interpreter ShiroToDeliverTo;

            internal bool WantsDelivery => DeliverTo != null && ShiroToDeliverTo != null;
        }

        private List<PublishedThing> PublishedThings = new List<PublishedThing>();
        private object PublishLock = new object();

        internal void Publish(Token eval, Token val, string awaitDelivery = null, Interpreter awaitDeliveryInterpreter = null)
        {
            lock (PublishLock)
                PublishedThings.Add(new PublishedThing()
                {
                    Eval = eval,
                    Val = val,
                    DeliverTo = awaitDelivery,
                    ShiroToDeliverTo = awaitDeliveryInterpreter
                });
        }

        internal void DispatchPublications()
        {
            Thread.Sleep(0);
            lock (PublishLock)
            {
                foreach(var pt in PublishedThings)
                {
                    Guid letId = Guid.NewGuid();

                    Symbols.Let("val", pt.Val.Clone(), letId);
                    var res = pt.Eval.Eval(this);
                    Symbols.ClearLetId(letId);

                    if (pt.WantsDelivery)
                        pt.ShiroToDeliverTo.Symbols.Deliver(pt.DeliverTo, res.Clone());
                }

                PublishedThings.Clear();
            }
        }

        public void RegisterAutoFunction(string name, Func<Interpreter, Token, Token> func)
        {
            Symbols.AddAutoFunc(name, func);
        }
        public bool IsFunctionName(string name)
        {
            return Symbols.FuncExists(name);
        }
        public bool IsVariableName(string name)
        {
            return Symbols.CanGet(name);
        }

        public string GetFunctionsForAutoComplete()
        {
            return Symbols.GetFunctionsForAutoComplete();
        }
    }
}
