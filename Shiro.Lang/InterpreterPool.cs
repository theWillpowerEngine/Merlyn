using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shiro.Support
{
    public class InterpreterPool
    {
        protected object _submissionLock = new object();
        protected Interpreter[] _interpreters = null;

        public InterpreterPool(int poolSize=5, Interpreter cloneFrom = null)
        {
            _interpreters = new Interpreter[poolSize];
            for (var i = 0; i < poolSize; i++)
                _interpreters[i] = (cloneFrom != null) ? Interpreter.CloneFrom(cloneFrom): new Interpreter();
        }

        protected Interpreter GetLaziestInterpreter()
        {
            Interpreter retVal = null;
            var workCount = int.MaxValue;

            lock(_submissionLock)
            {
                foreach(var i in _interpreters)
                {
                    if(i.QueueDepth < workCount)
                    {
                        workCount = i.QueueDepth;
                        retVal = i;
                    }

                    if (workCount == 0)
                        return retVal;
                }
            }

            return retVal;
        }

        public Task<Token> Evaluate(Token cmd, Token val = null)
        {
            var i = GetLaziestInterpreter();

            var ret = new Task<Token>(() =>
            {
                Token t = null;

                lock (_submissionLock)
                    i.EvalAsync(cmd, val ?? Token.Nil.Clone(), toke =>
                      {
                          t = toke;
                      });

                while (t == null)
                    Thread.Sleep(10);

                return t;
            });

            return ret;
        }
    }
}
