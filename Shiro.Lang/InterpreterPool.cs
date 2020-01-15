using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shiro.Support
{
    public class InterpreterPool : IDisposable
    {
        public class InterpreterPoolInterpreter
        {
            public Interpreter Shiro;
            protected Guid? letId = null;
            protected InterpreterPool _pool;

            public InterpreterPoolInterpreter(InterpreterPool ip, Interpreter i)
            {
                _pool = ip;
                Shiro = i;
            }

            public InterpreterPoolInterpreter Let(string name, Token val)
            {
                if (!letId.HasValue)
                    letId = Guid.NewGuid();

                Shiro.Symbols.Let(name, val, letId.Value);
                return this;
            }

            public async Task<Token> Eval(Token cmd, Token val = null)
            {
                var ret = new Task<Token>(() =>
                {
                    Token t = null;

                    Shiro.EvalAsync(cmd, val ?? Token.Nil.Clone(), toke =>
                    {
                        t = toke;
                    });

                    while (t == null)
                        Thread.Sleep(10);

                    if(letId.HasValue)
                    {
                        Shiro.Symbols.ClearLetId(letId.Value);
                        letId = null;
                    }

                    _pool.ReleaseInterpreter(Shiro);
                    return t;
                });
                ret.Start();

                return await ret;
            }
        }

        protected Dictionary<Interpreter, bool> _ints = new Dictionary<Interpreter, bool>();
        protected Interpreter _cloneFrom = null;

        protected object _submissionLock = new object();

        public InterpreterPool(Interpreter cloneFrom = null)
        {
            _cloneFrom = new Interpreter(cloneFrom.Symbols) ?? new Interpreter();
        }

        protected Interpreter GetAnInterpreter()
        {
            lock(_submissionLock)
            {
                var key = _ints.Keys.FirstOrDefault(k => _ints[k] == false);

                if (key == null)
                {
                    key = new Interpreter(_cloneFrom.Symbols);
                    _ints.Add(key, true);
                    return key;
                }

                _ints[key] = true;
                return key;
            }
        }

        protected void ReleaseInterpreter(Interpreter i)
        {
            lock (_submissionLock)
            {
                if (!_ints.ContainsKey(i) || !_ints[i])
                    throw new ApplicationException("Interpreter was released in InterpreterPool but wasn't doing anything.  Core logic error.");

                _ints[i] = false;
            }
        }

        public InterpreterPoolInterpreter Begin()
        {
            var i = GetAnInterpreter();
            return new InterpreterPoolInterpreter(this, i);
        }

        public void Dispose()
        {
            foreach (var key in _ints.Keys)
                ((IDisposable)key).Dispose();

            ((IDisposable)_cloneFrom).Dispose();
        }
    }
}
