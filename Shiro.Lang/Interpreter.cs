using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;
using Shiro.Guts;
using System.Threading;

namespace Shiro
{
    public partial class Interpreter : IDisposable
    {
        public static string Version = "0.3.1";
        public Guid InterpreterId = Guid.NewGuid();
        internal Symbols Symbols;
        internal Loader Loader = new Loader();

        public abstract class ShiroPlugin
        {
            public abstract void RegisterAutoFunctions(Interpreter shiro);
            protected Symbols GetSym(Interpreter shiro)
            {
                return shiro.Symbols;
            }
        }

        void IDisposable.Dispose()
        {
            CleanUpQueues();
            Symbols = null;
            Loader = null;
        }

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
            PublishedThings = new List<PublishedThing>();
            foreach(var id in MySubscriptions.Keys)
            {
                Conduit.Unsubscribe(MySubscriptions[id], id);
            }
            MySubscriptions.Clear();
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
        private Dictionary<Guid, string> MySubscriptions = new Dictionary<Guid, string>();

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

        protected bool _atomicEvaluation = false;

        internal void DispatchPublications()
        {
            Thread.Sleep(0);

            if (_atomicEvaluation || PublishedThings.Count == 0)
                return;
            
            lock (PublishLock)
            {
                while(PublishedThings.Count > 0)
                {
                    var pt = PublishedThings[0];
                    Guid letId = Guid.NewGuid();

                    PublishedThings.RemoveAt(0);

                    Symbols.Let("val", pt.Val.Clone(), letId);
                    _atomicEvaluation = true;
                    var res = pt.Eval.Eval(this, true);
                    _atomicEvaluation = false;
                    Symbols.ClearLetId(letId);

                    if (pt.WantsDelivery)
                        pt.ShiroToDeliverTo.Symbols.Deliver(pt.DeliverTo, res.Clone());
                }

                PublishedThings.Clear();
            }
        }

        public void RegisterAutoFunction(string name, Func<Interpreter, Token, Token> func, string helpTip = null)
        {
            Symbols.AddAutoFunc(name, func, helpTip);
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

        public string GetHelpTipFor(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return null;

            return Symbols.GetHelpTipFor(word);
        }
    }
}
