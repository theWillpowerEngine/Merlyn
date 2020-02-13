using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Shiro.Guts;
using Shiro.Nimue;

namespace Shiro
{
    public partial class Interpreter
    {
        private Token _bestGuessAtThisForLambda = null;     //I have a feeling let-scopes just "make this work" but I also have a feeling my initial feeling is wrong (Note:  That *may* have been because let-scopes had a hidden fuck up in them, so far this seems pretty solid).

        public Token Eval(List<Token> list, bool atomic = false, bool skipRootObjectLambdas = false)
        {
            if (list == null || list.Count == 0)
                return Token.Nil;

            //Inline objects have an "implicit quote"
            if (!string.IsNullOrEmpty(list[0].Name))
                return MiscHelper.EvaluateObject(this, new Token(list), atomic);

            if (!atomic)
                DispatchPublications();
            else
                Thread.Sleep(0);

            /*   BEGIN Sperge:
             * 
             * Okay there are four possible conditions we need to handle when the first item in our list is itself a list. They are:
             * 
             *  1)  A racist wants us to think their list is Jewish, ie:
             *              (((print 'hello world')))
             *              
             *  2)  We're calling lambda we build or load, but not an object lambda, ie:
             *              ((=> (x y) (+ $x $y)) 2 2)
             *              
             *              (do 
             *                  (sod lambda-name (=> (x y) (+ $x $y)))
             *                  ($lambda-name 2 2))
             *                           
             * 3)  Object lambdas.  These are different because the '.' keyword attempts to auto-evaluate an object-lambda which can
             *     be called without parameters if it is found at the root level of a list.  In this situation we want it to return the
             *     value of the lambda, not attempt to evaluate it.  Note that this only applies to lambdas which could be called without
             *     params, but we want all object lambdas to work basically the same way if possible.  ie (using prop from SSL):
             *              (do 
             *                  (sod o { name: (prop 'Dan' (v)->(print `name changed to {$v}`)), age: (prop 36) })
             *                  (o.name 'Bob'))
             *                  
             * 4)  Sibling-Peered syntax error, ie:
             *              (print 1)(print 2)
             *              
             *              ((print 1) 123)
             *              
             *              etc.
             *              
             *   END Sperge     */

            if (list[0].IsParent)
            {
                //1)  A single Jewish list as written by a racist
                if (list.Count == 1)
                    return list[0].Eval(this, atomic, skipRootObjectLambdas);

                //2 or 3 -- We somehow get an in-memory lambda as the command (object lambda or <other>)
                Token evalled = list[0].Eval(this, atomic, true);     //Evaluate the command list (without attempt to auto-eval object lambdas)
                if (evalled.IsFunction)
                    list[0] = evalled;
                else    //4)  Sadtrombone
                    Error("Sibling peered list passed for evaluation -- you are probably missing a 'do' keyword");
            }

            int i, j, k;
            long l;
            Token lastVal = Token.Nil, val = Token.Nil;
            string s1 = "", s2 = "", name = "";
            List<string> ls = new List<string>();
            List<Token> ts = new List<Token>();
            Token toke = null, request = null, toke2 = null;

            switch (list[0].Toke?.ToString()?.ToLower())
            {
                #region IO and string manipulation

                case "print":
                    for (i = 1; i < list.Count; i++)
                    {
                        lastVal = list[i].Eval(this, atomic, skipRootObjectLambdas);
                        Output(lastVal.ToString() + Environment.NewLine);
                    }
                    return lastVal;

                case "printnb":
                case "pnb":
                    for (i = 1; i < list.Count; i++)
                        Output((lastVal = list[i].Eval(this, atomic, skipRootObjectLambdas)).ToString());
                    return lastVal;

                case "string":
                case "str":
                    s1 = "";
                    for (i = 1; i < list.Count; i++)
                        s1 += list[i].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    return new Token(s1);

                case "contains":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters of keyword 'contains', expected 2");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    s2 = list[2].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    return s1.Contains(s2) ? Token.True : Token.False;

                case "lower":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters of keyword 'lower', expected 1");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    return new Token(s1.ToLower());
                
                case "upper":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters of keyword 'upper', expected 1");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    return new Token(s1.ToUpper());

                //This can have a shitty, long name because there's a reader shortcut for it
                case "interpolate":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters of keyword 'interpolate', expected 1");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    return new Token(MiscHelper.Interpolate(this, s1));

                case "split":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters of keyword 'split', expected 2");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    s2 = list[2].Eval(this, atomic, skipRootObjectLambdas).ToString();

                    var split = s1.Split(new string[] { s2 }, StringSplitOptions.RemoveEmptyEntries);
                    return new Token(split.Select(s => new Token(s)).ToList());

                #endregion

                #region Web service stuff and serialization

                case "json":
                    if(!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'json', expected 1");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    return new Token(toke.ToJSON(this));

                case "jsonv":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'jsonv', expected 1");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    return new Token(toke.ToJSON(this, true));

                case "dejson":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'dejson', expected 1");
                    toke = ScanInlineObject(list[1].Eval(this, atomic, skipRootObjectLambdas).ToString(), true);
                    return toke;
                #endregion

                #region Tree/list manipulation

                case "quote":
                    for (i = 1; i < list.Count; i++)
                    {
                        toke = list[i].Eval(this, atomic, skipRootObjectLambdas);
                        ts.Add(toke);
                    }
                    return new Token(ts)
                    {
                        IsQuotedList = true
                    };

                case "eval":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'eval', expected 1");
                    return list[1].Eval(this, atomic, skipRootObjectLambdas).Eval(this, atomic, skipRootObjectLambdas);

                case "skw":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'skw', expected 2");
                    lastVal = new Token(list[1].Eval(this, atomic, skipRootObjectLambdas));
                    if(list[2].IsParent)
                        lastVal.Children.AddRange(list[2].Eval(this, atomic, skipRootObjectLambdas).Children);
                    else
                        lastVal.Children.Add(list[2]);
                    return lastVal;

                case "kw":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'kw', expected 1");
                    lastVal = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (lastVal.IsParent)
                    {
                        if (lastVal.Children.Count == 0)
                            return Token.Nil;
                        else
                            return lastVal.Children[0];
                    }
                    return lastVal;
                case "params":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'params', expected 1");
                    lastVal = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (lastVal.IsParent)
                        return new Token(lastVal.Children.Quote());
                    return Token.Nil;

				case "nth":
					if (!list.ValidateParamCount(2))
						Error("Wrong number of parameters to keyword 'nth', expected 2");
					toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
					if (!toke.IsNumeric || toke.IsDecimal)
						Error("The first parameter to 'nth' must be a number");
					l = (long)toke.Toke;

					lastVal = list[2].Eval(this, atomic, skipRootObjectLambdas);
					if (lastVal.IsParent) {
						if (lastVal.Children.Count > l)
							return lastVal.Children[(int)(l - 1)];
						return Token.Nil;
					}
					return lastVal;

				case "range":
					if (!list.ValidateParamCount(3))
						Error("Wrong number of parameters to keyword 'range', expected 3");

					toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
					if (!toke.IsNumeric || toke.IsDecimal)
						Error("The first parameter to 'range' must be a number");
					j = (int)((long)toke.Toke);

					toke = list[2].Eval(this, atomic, skipRootObjectLambdas);
					if (!toke.IsNumeric || toke.IsDecimal)
						Error("The second parameter to 'range' must be a number");
					k = (int)((long)toke.Toke);

					if (j < 1)
						Error("First parameter to 'range' cannot be less than 1");

					lastVal = list[3].Eval(this, atomic, skipRootObjectLambdas);
					if (lastVal.IsParent)
					{
						for (i = j - 1; (i < (j + k) - 1 && i < lastVal.Children.Count); i++)
							ts.Add(lastVal.Children[i]);

						return new Token(ts);
					}
					return lastVal;

				case "concat":
                    for (i = 1; i < list.Count; i++)
                    {
                        toke = list[i].Eval(this, atomic, skipRootObjectLambdas);
                        if(toke.IsParent)
                            ts.AddRange(toke.Children);
                        else
                            ts.Add(toke);
                    }
                    return new Token(ts);

                case "filter":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'filter', expected 2");
                    toke = list[2].Eval(this, atomic, skipRootObjectLambdas);

                    if (!toke.IsParent)
                        Error("Filter can only operate on a list");

                    var filteredItems = new List<Token>();
                    foreach (var t in toke.Children)
                    {
                        ts.Clear();
                        ts.Add(list[1]);
                        ts.Add(t);

                        Token filterCheck = Eval(ts);
                        if (MathHelper.Not(filterCheck).Toke == Token.False.Toke)
                            filteredItems.Add(t);
                    }
                    return new Token(filteredItems.ToArray());

                case "apply":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'apply', expected 2");
                    toke = list[2].Eval(this, atomic, skipRootObjectLambdas);

                    if (!toke.IsParent)
                        Error("apply can only operate on a list");

                    var mappedItems = new List<Token>();
                    foreach (var t in toke.Children)
                    {
                        ts.Clear();
                        ts.Add(list[1]);
                        ts.Add(t);

                        mappedItems.Add(Eval(ts));
                    }
                    return new Token(mappedItems);

                case "map":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'map', expected 2");
                    toke = list[2].Eval(this, atomic, skipRootObjectLambdas);

                    if (!toke.IsParent)
                        Error("map can only operate on a list");

                    foreach (var t in toke.Children)
                    {
                        ts.Clear();
                        ts.Add(list[1]);
                        ts.Add(t);

                        Eval(ts);
                    }
                    return new Token(toke.Children);

                #endregion

                #region Objects

                case ".":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword '.', expected at least 2");

                    toke = toke2 = list[1].Eval(this, atomic, skipRootObjectLambdas);

                    for (i = 2; i < list.Count; i++)
                    {
                        var t2 = list[i].Eval(this, atomic, skipRootObjectLambdas);
                        if (t2.IsParent)
                            t2 = t2.Eval(this, atomic, skipRootObjectLambdas);
                        s1 = t2.ToString();

                        if (toke.Children == null || !toke.Children.HasProperty(this, s1))
                            Error($"Cannot dereference property {s1} off {toke.ToString()}");
                        else
                        {
                            toke2 = toke;
                            toke = toke.Children.GetProperty(this, s1);
                        }
                    }

                    if (!toke.IsFunction)
                        toke = toke.Eval(this, atomic);

                    if (toke.IsFunction && toke.IsLambdaWhichCanBeCalledWithoutParameters && !skipRootObjectLambdas)
                        return toke.EvalLambda(toke2, this);
                    else
                        _bestGuessAtThisForLambda = toke2;

                    return toke;

                case ".s":
                case ".set":
                    if (!list.ValidateParamCount(3, true))
                        Error("Wrong number of parameters to keyword '.set', expected at least 3");

                    if (list[1].IsParent)
                        Error("First argument to '.set' must be a name, not simply a list or inline object");

                    name = list[1].Toke.ToString();
                    if (!Symbols.CanGet(name))
                        Error("Can't find variable " + name + " for .set command");

                    toke = Symbols.Get(name);

                    for (i = 2; i < list.Count-1; i++)
                    {
                        var t2 = list[i].Eval(this, atomic, skipRootObjectLambdas);
                        if (t2.IsParent)
                            t2 = t2.Eval(this, atomic, skipRootObjectLambdas);
                        s1 = t2.ToString();

                        if (toke.Children == null || !toke.Children.HasProperty(this, s1))
                            Error($"Cannot dereference property {s1} off {toke.ToString()}");
                        else
                        {
                            if(i < list.Count - 2)
                                toke = toke.Children.GetProperty(this, s1);
                            //s1 will hold the name of the property we're working with
                        }
                    }

                    val = list[list.Count - 1].Eval(this, atomic, skipRootObjectLambdas);
                    if (!toke.Children.SetProperty(this, s1, val))
                        Error("Could not find property " + name + " to set using .set.  Consider concat/pairing it, or using .sod instead.");

                    return Symbols.Get(name);

                case ".sod":
                    if (!list.ValidateParamCount(3, true))
                        Error("Wrong number of parameters to keyword '.sod', expected at least 3");

                    if (list[1].IsParent)
                        Error("First argument to '.sod' must be a name, not simply a list or inline object");

                    name = list[1].Toke.ToString();
                    if (!Symbols.CanGet(name))
                        Error("Can't find variable " + name + " for .sod command");

                    toke = Symbols.Get(name);

                    for (i = 2; i < list.Count - 1; i++)
                    {
                        var t2 = list[i].Eval(this, atomic, skipRootObjectLambdas);
                        if (t2.IsParent)
                            t2 = t2.Eval(this, atomic, skipRootObjectLambdas);
                        s1 = t2.ToString();

                        if (i < list.Count - 2)
                        {
                            if (toke.Children == null || !toke.Children.HasProperty(this, s1))
                                Error($"Cannot dereference property {s1} off {toke.ToString()}");
                            else
                            {
                                toke = toke.Children.GetProperty(this, s1);
                            }
                        }
                    }

                    val = list[list.Count - 1].Eval(this, atomic, skipRootObjectLambdas);
                    if (!toke.Children.SetProperty(this, s1, val))
                        toke.Children.AddProperty(this, s1, val);

                    return Symbols.Get(name);

                case ".c":
                case ".call":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword '.call', expected at least 2");

                    toke = toke2 = list[1].Eval(this, atomic, skipRootObjectLambdas);

                    for (i = 2; i < list.Count-1; i++)
                    {
                        var t2 = list[i].Eval(this, atomic, skipRootObjectLambdas);
                        if (t2.IsParent)
                            t2 = t2.Eval(this, atomic, skipRootObjectLambdas);
                        s1 = t2.ToString();

                        if (toke.Children == null || !toke.Children.HasProperty(this, s1))
                            Error($"Cannot dereference property {s1} off {toke.ToString()}");
                        else
                        {
                            toke2 = toke;
                            toke = toke.Children.GetProperty(this, s1);
                        }
                    }

                    if(!toke.IsFunction)
                        toke = toke.Eval(this, atomic, skipRootObjectLambdas);

                    if (!toke.IsFunction)
                        Error("Cannot call non-function property " + s1);

                    var argList = list[list.Count - 1];
                    if (argList.IsParent)
                        return toke.EvalLambda(toke2, this, argList.Children.ToArray());
                    else
                        return toke.EvalLambda(toke2, this, argList);

                case ".d":
                case ".def":
                    if (!list.ValidateParamCount(3, true))
                        Error("Wrong number of parameters to keyword '.def', expected at least 3");

                    if (list[1].IsParent)
                        Error("First argument to '.def' must be a name, not simply a list or inline object");

                    name = list[1].Toke.ToString();
                    if (!Symbols.CanGet(name))
                        Error("Can't find variable " + name + " for .def command");

                    toke = Symbols.Get(name);

                    for (i = 2; i < list.Count - 1; i++)
                    {
                        var t2 = list[i].Eval(this, atomic, skipRootObjectLambdas);
                        if (t2.IsParent)
                            t2 = t2.Eval(this, atomic, skipRootObjectLambdas);
                        s1 = t2.ToString();

                        if (i < list.Count - 2)
                        {
                            if (toke.Children == null || !toke.Children.HasProperty(this, s1))
                                Error($"Cannot dereference property {s1} off {toke.ToString()}");
                            else
                            {
                                toke = toke.Children.GetProperty(this, s1);
                            }
                        }
                    }

                    val = list[list.Count - 1].Eval(this, atomic, skipRootObjectLambdas);
                    if (!toke.Children.HasProperty(this, s1))
                        toke.Children.AddProperty(this, s1, val);
                    else
                        Error($"Attempt to .def already existing property {s1}.  Let me introduce you to '.sod' instead.");

                    return Symbols.Get(name);

                case ".?":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword '.?', expected at least 2");

                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);

                    for (i = 2; i < list.Count; i++)
                    {
                        var t2 = list[i].Eval(this, atomic, skipRootObjectLambdas);
                        if (t2.IsParent)
                            t2 = t2.Eval(this, atomic, skipRootObjectLambdas);
                        s1 = t2.ToString();

                        if (toke.Children == null || !toke.Children.HasProperty(this, s1))
                            return Token.Nil;
                        else
                            toke = toke.Children.GetProperty(this, s1);
                    }
                    return toke.Eval(this, atomic);

                case "pair":
                    if(!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'pair', expected 2");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    toke = list[2].Eval(this, atomic, skipRootObjectLambdas);

                    if (toke.IsParent)
                        return new Token(new Token(s1, toke.Children));
                    else
                        return new Token(new Token(s1, toke.Toke));

                case "enclose":
                    if (!list.ValidateParamCount(2) && !list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'enclose', expected 1 or 2");

                    if (list.ValidateParamCount(2))
                    {
                        //Object enclosure ('private' variables)
                        toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                        toke2 = list[2].Eval(this, atomic, skipRootObjectLambdas);

                        if (!toke.IsObject || !toke2.IsObject)
                            Error($"Both parameters passed to enclose must be objects if you pass 2 things.  Instead I got these two things: {toke.ToString()} -AND- {toke2.ToString()}");

                        toke2.Enclosure = toke;
                        return toke2;
                    } else
                    {
                        //Lambda enclosure (closure scope)
                        toke = list[1].Eval(this, atomic, skipRootObjectLambdas);

                        if (!toke.IsFunction)
                            Error($"When taking only a single parameter, the parameter to enclose should be a lambda, not {toke.ToString()}");

                        toke.Enclosure = Symbols.GetLetScopeAsTardEnclosure();
                        return toke;
                    }
                #endregion

                #region Implementers and Mixins
                case "impl":
                case "implementer":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'implementer', expected 2");
                    if (list[1].IsParent)
                        Error("First argument to 'implementer' must be a name, not any kind of list");
                    s1 = list[1].Toke.ToString();

                    toke = list[2].Eval(this, atomic, skipRootObjectLambdas);
                    if (!toke.IsObject)
                        Error("Second parameter to implementer must be an object, not: " + toke.ToString());

                    Symbols.SetImplementer(s1, toke);
                    return lastVal;

                case "mixin":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword 'mixin', expected at least 2");

                    toke = toke2 = list[list.Count - 1].Eval(this, atomic, skipRootObjectLambdas);
                    List<string> mixins = new List<string>();

                    for (i = 1; i < list.Count-1; i++)
                    {
                        var mixin = list[i];
                        if (mixin.IsParent)
                            Error("All parameters but the last to 'mixin' must be the names of implementers");

                        s1 = mixin.ToString();
                        if (!Symbols.CanGetImplementer(s1))
                            Error("Can't find implementer " + s1);

                        mixins.Add(s1);
                    }
                    return MiscHelper.MixIn(this, toke, mixins.ToArray());

                case "new":
                    if (!list.ValidateParamCount(1, true))
                        Error("Wrong number of parameters to keyword 'new', expected at least 1");

                    s1 = list[1].Eval(this, atomic).ToString();
                    if (!Symbols.CanGetImplementer(s1))
                        Error($"Can't create a new {s1}, implementer not found.");

                    var args = new List<Token>();
                    for (i = 2; i < list.Count; i++)
                        args.Add(list[i].Eval(this, atomic));

                    var impl = Symbols.GetImplementer(s1);
                    if(impl.Children.HasProperty(this, s1))
                    {
                        var ctor = impl.Children.GetProperty(this, s1);
                        if (!ctor.IsFunction)
                            Error($"Constructor for {s1} must be an object-lambda, not {ctor.ToString()}.");

                        if (ctor.Params.Count < args.Count)
                            Error($"Too many params passed to constructor for {s1}, expected a max of {ctor.Params.Count}, got {args.Count} instead.");

                        while (ctor.Params.Count > args.Count)
                            args.Add(Token.Nil);

                        toke = MiscHelper.MixIn(this, new Token(new List<Token>()), new string[] { s1 });
                        if(impl.Enclosure != null)
                            toke.Enclosure = impl.Enclosure.Clone();
                        ctor.EvalLambda(toke, this, args.ToArray());
                        return toke;
                    }
                    else
                    {
                        //Default, empty CTOR
                        if (args.Count == 0)
                            return MiscHelper.MixIn(this, new Token(new List<Token>()), new string[] { s1 });

                        Error($"Not constructor found for {s1} and you supplied {args.Count} parameters so the default can't be used.");
                    }
                    return Token.Nil;

                case "impl?":
                case "quack?":      //lol
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'impl?', expected 2");

                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    s1 = list[2].Eval(this, atomic, skipRootObjectLambdas).ToString();

                    if (!Symbols.CanGetImplementer(s1))
                        Error("impl? called with unknown implementer " + s1);

                    return MiscHelper.DoesItQuack(this, toke, Symbols.GetImplementer(s1), s1);

                #endregion

                #region Variables

                case "def":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'def', expected 2");
                    if(list[1].IsParent)
                        Error("First argument to 'def' must be a name, not any kind of list");
                    s1 = list[1].Toke.ToString();
                    if (Symbols.CanGet(s1))
                        Error($"Cannot def '{s1}', variable already defined");
                    Symbols.Set(s1, lastVal = list[2].Eval(this, atomic, skipRootObjectLambdas));
                    return lastVal;
                case "set":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'set', expected 2");
                    if (list[1].IsParent)
                        Error("First argument to 'set' must be a name, not any kind of list");
                    s1 = list[1].Toke.ToString();
                    if(!Symbols.CanGet(s1))
                        Error($"Cannot set '{s1}', variable not defined yet");
                    Symbols.Set(s1, lastVal = list[2].Eval(this, atomic, skipRootObjectLambdas));
                    return lastVal;
                case "sod":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'sod', expected 2");
                    if (list[1].IsParent)
                        Error("First argument to 'sod' must be a name, not any kind of list");
                    s1 = list[1].Toke.ToString();
                    Symbols.Set(s1, lastVal = list[2].Eval(this, atomic, skipRootObjectLambdas));
                    return lastVal;

                case "undef":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'undef', expected 1");
                    if (list[1].IsParent)
                        Error("First argument to 'undef' must be a name, not any kind of list");

                    s1 = list[1].Toke.ToString();
                    if (!Symbols.CanGet(s1))
                        return Token.Nil;

                    lastVal = Symbols.Get(s1);
                    Symbols.UnSet(s1);
                    return lastVal;

                case "v":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'v', expected 1");
                    if (Symbols.CanGet(s1 = list[1].Toke.ToString()))
                        return Symbols.Get(s1);
                    else
                        Error("Variable not found: " + s1);
                    return Token.Nil;

                case "gv":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'gv', expected 1");
                    if (Symbols.CanGetGlobal(s1 = list[1].Toke.ToString()))
                        return Symbols.GetGlobal(s1);
                    else
                        Error("Variable not found: " + s1);
                    return Token.Nil;

                case "let":
                    if (!list.ValidateParamCount(1, true))
                        Error("Wrong number of parameters to keyword 'let', expected at least 2");
                    if (!list[1].IsParent)
                        Error("Let must include at least one parameter in the let-list");
                    if (list[1].Children.Count != 0 && list[1].Children.Count % 2 != 0)
                        Error("The let-list must have an even number of parameters");

                    Guid letId = Guid.NewGuid();
                    for (i = 0; i < list[1].Children.Count; i += 2)
                    {
                        s1 = list[1].Children[i].Toke.ToString();
                        toke = list[1].Children[i + 1].Eval(this, atomic, skipRootObjectLambdas);

                        Symbols.Let(s1, toke, letId);
                    }

                    try
                    {
                        for (i = 2; i < list.Count; i++)
                            lastVal = list[i].Eval(this, atomic, skipRootObjectLambdas);
                    }
                    catch (Exception ex)
                    {
                        Error(ex.Message);
                    }
                    finally
                    {
                        Symbols.ClearLetId(letId);
                    }
                    return lastVal;

                case "relet":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'relet', expected 2");
                    
                    s1 = list[1].Toke.ToString();
                    Symbols.ReLet(s1, lastVal = list[2].Eval(this, atomic, skipRootObjectLambdas));
                    return lastVal;

                #endregion

                #region Math and CondOps

                case "+":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword '+', expected at least 2");
                    return MathHelper.Add(list.Quote().Select(t => t.Eval(this, atomic, skipRootObjectLambdas)).ToArray());
                case "-":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword '-', expected at least 2");
                    return MathHelper.Subtract(list.Quote().Select(t => t.Eval(this, atomic, skipRootObjectLambdas)).ToArray());

                case "*":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword '*', expected at least 2");
                    return MathHelper.Multiply(list.Quote().Select(t => t.Eval(this, atomic, skipRootObjectLambdas)).ToArray());
                case "/":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword '/', expected at least 2");
                    return MathHelper.Divide(list.Quote().Select(t => t.Eval(this, atomic, skipRootObjectLambdas)).ToArray());

                case "=":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword '=', expected 2");
                    return MathHelper.Equals(list[1].Eval(this, atomic, skipRootObjectLambdas), list[2].Eval(this, atomic, skipRootObjectLambdas));

                case "!":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword '!', expected 1");
                    return MathHelper.Not(list[1].Eval(this, atomic, skipRootObjectLambdas));

                case "!=":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword '!=', expected 2");
                    return MathHelper.Not(MathHelper.Equals(list[1].Eval(this, atomic, skipRootObjectLambdas), list[2].Eval(this, atomic, skipRootObjectLambdas)));

                case ">":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword '>', expected 2");
                    return MathHelper.GreaterThan(list[1].Eval(this, atomic, skipRootObjectLambdas), list[2].Eval(this, atomic, skipRootObjectLambdas));
                case "<":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword '<', expected 2");
                    return MathHelper.LessThan(list[1].Eval(this, atomic, skipRootObjectLambdas), list[2].Eval(this, atomic, skipRootObjectLambdas));

                case ">=":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword '>=', expected 2");
                    return MathHelper.Not(MathHelper.LessThan(list[1].Eval(this, atomic, skipRootObjectLambdas), list[2].Eval(this, atomic, skipRootObjectLambdas)));
                case "<=":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword '<=', expected 2");
                    return MathHelper.Not(MathHelper.GreaterThan(list[1].Eval(this, atomic, skipRootObjectLambdas), list[2].Eval(this, atomic, skipRootObjectLambdas)));

                case "and":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword 'and', expected at least 2");

                    for(i=1; i<list.Count; i++)
                        if (!list[i].Eval(this, atomic).IsTrue)
                            return Token.False;

                    return Token.True;

                case "or":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of parameters to keyword 'or', expected at least 2");

                    for (i = 1; i < list.Count; i++)
                        if (list[i].Eval(this, atomic).IsTrue)
                            return Token.True;

                    return Token.False;
                #endregion

                #region Conditional Elements

                case "awaiting?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'awaiting?', expected 1");

                    s1 = list[1].Toke.ToString();
                    if (!Symbols.CanGet(s1))
                        Error("Can't check if unknown variable " + s1 + " is being awaited.");

                    return Symbols.IsVarBeingAwaited(s1) ? Token.True : Token.False;

                case "queue?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'queue?', expected 1");

                    s1 = list[1].Toke.ToString();

                    return Conduit.HasQueue(s1) ? Token.True : Token.False;

                case "error?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'error?', expected 1");

                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (toke.IsObject && toke.Children.HasProperty(this, "error"))
                        return Token.True;

                    return Token.False;

                case "list?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'list?', expected 1");

                    if (list[1].IsQuotedList)
                        return Token.True;

                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (toke.Children != null)
                        return Token.True;
                    return Token.False;

                case "fn?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'fn?', expected 1");
                    toke = list[1];
                    s1 = toke.ToString();
                    if (Symbols.FuncExists(s1))
                        return Token.True;
                    if (toke.IsFunction)
                        return Token.True;

                    toke = toke.Eval(this, atomic, skipRootObjectLambdas);
                    s1 = toke.ToString();
                    if (Symbols.FuncExists(s1))
                        return Token.True;
                    if (toke.IsFunction)
                        return Token.True;

                    return Token.False;

                case "nil?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'nil?', expected 1");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if(toke.IsNil)
                        return Token.True;
                    return Token.False;

                case "obj?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'obj?', expected 1");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (toke.Children != null && toke.Children.Any(t => !string.IsNullOrEmpty(t.Name)))
                        return Token.True;
                    return Token.False;

                case "num?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'num?', expected 1");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (toke.IsNumeric)
                        return Token.True;
                    return Token.False;

                case "str?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'str?', expected 1");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (!toke.IsParent && !toke.IsNumeric)
                        return Token.True;
                    return Token.False;

                case "def?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'def?', expected 1");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    s1 = toke.ToString();
                    if (Symbols.CanGet(s1))
                        return Token.True;
                    if (Symbols.FuncExists(s1))
                        return Token.True;
                    if (Symbols.CanGetImplementer(s1))
                        return Token.True;
                    return Token.False;

                #endregion

                #region Functions and Lambdas

                case "defn":
                    if (!list.ValidateParamCount(3))
                        Error("Wrong number of parameters to keyword 'defn', expected 3");
                    s1 = list[1].Toke.ToString();
                    toke = new Token();
                    toke.Children = new List<Token>();
                    toke.Params = new List<Param>();

                    if (!list[2].IsParent)
                        Error("Parameter list for defn must be a list (even if empty or containing only one element)");

                    foreach (var p in list[2].Children)
                    {
                        if (p.IsParent || p.IsNumeric)
                            Error("List or numeric value passed to parameter list for defn");
                        else
                            toke.Params.Add(new Param(this, p.Toke.ToString()));
                    }

                    if (!list[3].IsParent)
                        Error("Function body must be a list");

                    toke.Children.AddRange(list[3].Children);
                    Symbols.AddFunc(s1, toke);
                    return Token.Nil;

                case "=>":
                case "fn":
                    if (!list.ValidateParamCount(1) && !list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword '=>', expected 1 or 2");

                    Token lambda = (list.Count == 2) ? list[1] : list[2];
                    if (!lambda.IsParent)
                        Error("Lambda body must be a list, not: " + lambda.ToString());

                    lambda = lambda.Clone();
                    if (list.Count == 3)
                    {
                        if (list[1].IsParent)
                            lambda.Params = list[1].Children.Select(e =>
                            {
                                if (e.IsParent)
                                    Error("Parameter list in lambda can't have lists in it");

                                return new Param(this, e.ToString());
                            }).ToList();
                        else
                            lambda.Params = new List<Param>(new Param[] { new Param(this, list[1].Toke.ToString()) });
                    } else
                        lambda.Params = new List<Param>();
                    return lambda;

                #endregion

                #region Control Commands

                case "if":
                    if (!list.ValidateParamCount(2) && !list.ValidateParamCount(3))
                        Error("Wrong number of parameters to keyword 'if', expected 2 or 3");
                    bool hasElse = list.Count == 4;
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (MathHelper.Not(toke).Toke == Token.False.Toke)
                        return list[2].Eval(this, atomic, skipRootObjectLambdas);
                    else
                    {
                        if(hasElse)
                            return list[3].Eval(this, atomic, skipRootObjectLambdas);

                        return Token.Nil;
                    }

                case "throw":
                    if (list.Count == 1)
                        throw new ShiroException(Token.Nil);

                    throw new ShiroException(list[1].Eval(this, atomic, skipRootObjectLambdas));

                case "catch":
                    if (!list.ValidateParamCount(2) && !list.ValidateParamCount(3))
                        Error("Wrong number of parameters to keyword 'catch', expected 2 or 3");

                    toke = list[1];
                    var finBlock = list.Count == 4 ? list[3] : null;
                    try
                    {
                        toke = toke.Eval(this, atomic, skipRootObjectLambdas);
                        if (toke.IsFunction && toke.Params.Count == 0)
                            toke = toke.EvalLambda(null, this);
                    } 
                    catch(ShiroException shex)
                    {
                        letId = Guid.NewGuid();
                        Symbols.Let("ex", shex.Exception, letId);
                        toke = list[2].Eval(this, atomic, skipRootObjectLambdas);
                        Symbols.ClearLetId(letId);
                    } 
                    finally
                    {
                        if (finBlock != null)
                        {
                            letId = Guid.NewGuid();
                            Symbols.Let("result", toke, letId);
                            toke = finBlock.Eval(this, atomic, skipRootObjectLambdas);
                            Symbols.ClearLetId(letId);
                        }
                    }
                    return toke;

                case "try":
                    if (!list.ValidateParamCount(1) && !list.ValidateParamCount(2) && !list.ValidateParamCount(3))
                        Error("Wrong number of parameters to keyword 'try', expected 1, 2 or 3");

                    toke = list[1];
                    var finBlock2 = list.Count == 4 ? list[3] : null;
                    try
                    {
                        toke = toke.Eval(this, atomic, skipRootObjectLambdas);
                        if (toke.IsFunction && toke.Params.Count == 0)
                            toke = toke.EvalLambda(null, this);

                    }
                    catch (Exception ex)
                    {
                        if (list.Count >= 3)
                        {
                            letId = Guid.NewGuid();
                            Symbols.Let("ex", new Token(ex.Message), letId);
                            toke = list[2].Eval(this, atomic, skipRootObjectLambdas);
                            Symbols.ClearLetId(letId);
                        }
                    }
                    finally
                    {
                        if (finBlock2 != null)
                        {
                            letId = Guid.NewGuid();
                            Symbols.Let("result", toke, letId);
                            toke = finBlock2.Eval(this, atomic, skipRootObjectLambdas);
                            Symbols.ClearLetId(letId);
                        }
                    }
                    return toke;
                    
                case "do":
					for(i =1; i<list.Count;i++)
					{
						lastVal = list[i].Eval(this, atomic);
					}
					return lastVal;

                case "while":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters of keyword 'while', expected 2");

                    var condition = list[1];
                    var action = list[2];

                    while(condition.Eval(this, atomic, skipRootObjectLambdas).IsTrue)
                    {
                        lastVal = action.Eval(this, atomic, skipRootObjectLambdas);
                    }
                    return lastVal;

                case "switch":
                    if (!list.ValidateParamCount(3, true))
                        Error("'switch' keyword requires a minimum of 3 params (a value and a single case)");

                    bool doesSwitchHaveDefault = !((list.Count - 1) % 2 != 0);
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    s1 = toke.ToString();
                   
                    for (i = 2; i < list.Count - 1; i += 2)
                    {
                        if (!list[i].IsParent && !list[i].IsFunction)
                        {
                            s2 = list[i].Toke.ToString();
                            if (s1 == s2)
                                return list[i + 1].Eval(this, atomic, skipRootObjectLambdas);

                            try
                            {
                                toke2 = new Token(new Token(s2), toke).Eval(this, atomic, skipRootObjectLambdas);
                            }
                            catch (Exception) { toke2 = Token.Nil; }

                            if (toke2.IsTrue)
                                return list[i + 1].Eval(this, atomic, skipRootObjectLambdas);
                        }
                        else if (list[i].IsFunction)
                        {
                            if (list[i].Params.Count != 1)
                                Error("Anonymous function passed as a potential switch case must take only a single parameter.  My best attempt to render the offending lambda is: " + list[i].ToString());

                            var lr = list[i].EvalLambda(null, this, toke);
                            if (lr.IsTrue)
                                return list[i + 1].Eval(this, atomic, skipRootObjectLambdas);
                        }
                        else
                        {
                            toke2 = list[i].Eval(this, atomic, skipRootObjectLambdas);
                            if (!toke2.IsFunction)
                                Error("I don't know how to use " + toke2.ToString() + " as a switch case.");

                            var lr = toke2.EvalLambda(null, this, toke);
                            if (lr.IsTrue)
                                return list[i + 1].Eval(this, atomic, skipRootObjectLambdas);
                        }
                    }

                    if (!doesSwitchHaveDefault)
                        return Token.Nil;

                    return list[list.Count - 1].Eval(this, atomic, skipRootObjectLambdas);

                #endregion

                #region HTTP

                //Global
                case "stop":
                    if (!Server.Serving)
                        Error("Cannot use 'stop' keyword when we are not acting as a network server");
                    if (!list.ValidateParamCount(0) && !list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'stop', expected 0 or 1");

                    Server.Serving = false;
                    Thread.Sleep(150);

                    if (list.Count == 2)
                        Server.Result = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    else
                        Server.Result = Token.Nil;
                    return Server.Result;

                //HTTP
                case "http":
					if (Server.Serving)
						Error("Can't start an HTTP server while already serving something");
					if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'http', expected 2");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (!toke.IsNumeric)
                        Error("First argument to http command must be a port number");

                    l = (long)toke.Toke;   //l is now port number
                    toke = list[2];
                    if (!toke.IsParent)
                        toke = toke.Eval(this, atomic, skipRootObjectLambdas);

                    Server.ListenForHttp(this, toke, (int)l);

                    i = 100;        //1 second total retry time
                    while (null == (toke = Server.Result) && i-- > 0)
                        Thread.Sleep(10);

                    if (toke == null)
                        Error("There was a problem getting the result of the last server run");

                    Server.Result = null;
                    return toke;

                case "static":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'static', expected 2");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).Toke.ToString();
                    toke = list[2].Eval(this, atomic, skipRootObjectLambdas);
                    if (!Directory.Exists(s1))
                        Error($"Can't map static content at '{s1}', no such directory found.");
                    if (!toke.IsFunction)
                        Error($"Second parameter to static must be a lambda, not '{toke.ToString()}'");

                    if (!s1.EndsWith("\\") && !s1.EndsWith("/"))
                        s1 += "\\";

                    request = Symbols.Get(Symbols.AutoVars.HttpRequest);
                    toke2 = toke.EvalLambda(toke, this, request.Children.GetProperty(this, "url"));
                    s2 = toke2.ToString();

                    s2 = s1 + s2;
                    if (!File.Exists(s2))
                    {
                        HttpHelper.ResponseStatus = 404;
                        return new Token("File '" + s2 + "' wasn't found.");
                    }

                    HttpHelper.ContentType = MimeTypes.GetMimeType(Path.GetFileName(s2));
                    HttpHelper.ByteArray = File.ReadAllBytes(s2);
                    return Token.Nil;

                case "content":
					if(!list.ValidateParamCount(2))
						Error("Wrong number of parameters to keyword 'content', expected 2");
					if (!Server.Serving || Server.ConType != ConnectionType.HTTP)
						Error("Cannot use 'content' keyword if we are not currently in an http server context");
					s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).Toke.ToString();

					HttpHelper.ContentType = s1;
					return list[2].Eval(this, atomic, skipRootObjectLambdas);
				case "status":
					if (!list.ValidateParamCount(2))
						Error("Wrong number of parameters to keyword 'status', expected 2");
					if (!Server.Serving || Server.ConType != ConnectionType.HTTP)
						Error("Cannot use 'status' keyword if we are not currently in an http server context");

					toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
					if (!toke.IsNumeric || toke.IsDecimal)
						Error("First parameter to 'status' must be numeric");

					HttpHelper.ResponseStatus = (int)((long)toke.Toke);
					return list[2].Eval(this, atomic, skipRootObjectLambdas);

				case "route":
                    if ((list.Count - 1) % 2 != 0)
                        Error("Parameters to 'route' keyword must be paired");
                    if (!Server.Serving || Server.ConType != ConnectionType.HTTP)
                        Error("Cannot use 'route' keyword if we are not currently in an http server context");
                    request = Symbols.Get(Symbols.AutoVars.HttpRequest);
                    s1 = request.Children.GetProperty(this, "url").Toke.ToString();
                    
                    for (i = 1; i < list.Count; i += 2)
                    {
                        if (!list[i].IsParent && !list[i].IsFunction)
                        {
                            s2 = list[i].Toke.ToString();
                            if (NetHelper.IsRouteMatch(s1,  s2))
                                return list[i + 1].Eval(this, atomic, skipRootObjectLambdas);
                            if (s2.ToLower() == "default")
                                toke = list[i + 1];
                        }
                        else if (list[i].IsFunction)
                        {
                            if (list[i].Params.Count != 1)
                                Error("Anonymous Function passed as a potential route must take only a single parameter.  My best attempt to render the offending lambda is: " + list[i].ToString());

                            var lr = list[i].EvalLambda(null, this, request.Children.GetProperty(this, "url"));
                            if (lr.IsTrue)
                                return list[i + 1].Eval(this, atomic, skipRootObjectLambdas);
                        }
                        else
                        {
                            var iHopeThisIsALambdaYouTool = list[i].Eval(this, atomic, skipRootObjectLambdas);
                            if(!iHopeThisIsALambdaYouTool.IsFunction)
                                Error("You passed a list as a potential route, it has to be a string or a lambda");
                            
                            var lr = iHopeThisIsALambdaYouTool.EvalLambda(null, this, request.Children.GetProperty(this, "url"));
                            if (lr.IsTrue)
                                return list[i + 1].Eval(this, atomic, skipRootObjectLambdas);
                        }
                    }

                    if (toke != null)
                        return toke.Eval(this, atomic, skipRootObjectLambdas);
                    return Token.Nil;

                #region REST (It's big)
                case "rest":
                    if (!list.ValidateParamCount(2))
                        Error("The rest keyword requires 2 parameters");
                    if (!Server.Serving || Server.ConType != ConnectionType.HTTP)
                        Error("Cannot use 'rest' keyword if we are not currently in an http server context");
                    
                    request = Symbols.Get(Symbols.AutoVars.HttpRequest);
                    var restMethod = request.Children.GetProperty(this, "method").Toke.ToString();
                    s1 = request.Children.GetProperty(this, "url").Toke.ToString();
                    s2 = list[2].Eval(this, atomic, skipRootObjectLambdas).ToString();

                    var restDataStore = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (!restDataStore.IsParent) {
                        Error("The data source passed to the 'rest' keyword must be a list.  You ended up with: " + restDataStore.ToString());
                        HttpHelper.ResponseStatus = 400;
                        return new Token("Not Found");
                    }

                    Token restResult;
                    string[] eles;
                    string id;

                    switch(restMethod.ToLower())
                    {
                        case "get":
                            eles = s1.Split('/');
                            id = eles[eles.Length - 1];
                            restResult = restDataStore.Children.FirstOrDefault(t => t.Children.HasProperty(this, s2) && t.Children.GetProperty(this, s2).ToString() == id);
                            
                            if(restResult != null)
                            {
                                HttpHelper.ResponseStatus = 200;
                                return new Token(restResult.ToJSON(this));
                            } 
                            else
                            {
                                HttpHelper.ResponseStatus = 400;
                                return new Token("Not Found");
                            }

                        case "delete":
                            eles = s1.Split('/');
                            id = eles[eles.Length - 1];
                            restResult = restDataStore.Children.FirstOrDefault(t => t.Children.HasProperty(this, s2) && t.Children.GetProperty(this, s2).ToString() == id);

                            if (restResult != null)
                            {
                                restDataStore.Children.Remove(restResult);
                                HttpHelper.ResponseStatus = 200;
                                return new Token("Deleted");
                            }
                            else
                            {
                                HttpHelper.ResponseStatus = 500;
                                return new Token("Id not found, nothing was deleted");
                            }

                        case "post":
                            s1 = request.Children.GetProperty(this, "body").Toke.ToString();
                            toke = ScanInlineObject(s1, true);
                            id = toke.Children.GetProperty(this, s2).ToString();
                            if(restDataStore.Children.Any(t => t.Children.HasProperty(this, s2) && t.Children.GetProperty(this, s2).ToString() == id))
                            {
                                HttpHelper.ResponseStatus = 500;
                                return new Token("A duplicate ID exists, did you mean to use PUT?");
                            } 
                            else
                            {
                                restDataStore.Children.Add(toke);
                                HttpHelper.ResponseStatus = 200;
                                return toke;
                            }

                        case "put":
                            eles = s1.Split('/');
                            id = eles[eles.Length - 1];
                            s1 = request.Children.GetProperty(this, "body").Toke.ToString();
                            toke = ScanInlineObject(s1, true);
                            if(id != toke.Children.GetProperty(this, s2).ToString())
                            {
                                HttpHelper.ResponseStatus = 500;
                                return new Token("ID mismatch for PUT.  URL id was: " + id + ", object id was: " + toke.Children.GetProperty(this, s2).ToString());
                            }
                            restResult = restDataStore.Children.FirstOrDefault(t => t.Children.HasProperty(this, s2) && t.Children.GetProperty(this, s2).ToString() == id);

                            if (restResult != null)
                            {
                                restDataStore.Children.Remove(restResult);
                                restDataStore.Children.Add(toke);
                                HttpHelper.ResponseStatus = 200;
                                return toke;
                            }
                            else
                            {
                                HttpHelper.ResponseStatus = 500;
                                return new Token("PUT requires an ID in the URL (or I couldn't find the one you gave me).  You might try PATCH instead.");
                            }

                        case "patch":
                            s1 = request.Children.GetProperty(this, "body").Toke.ToString();
                            toke = ScanInlineObject(s1, true);
                            id = toke.Children.GetProperty(this, s2).ToString();
                            
                            restResult = restDataStore.Children.FirstOrDefault(t => t.Children.HasProperty(this, s2) && t.Children.GetProperty(this, s2).ToString() == id);

                            if (restResult != null)
                            {
                                restDataStore.Children.Remove(restResult);
                                restDataStore.Children.Add(toke);
                                HttpHelper.ResponseStatus = 200;
                                return toke;
                            }
                            else
                            {
                                HttpHelper.ResponseStatus = 202;
                                restDataStore.Children.Add(toke);
                                return toke;
                            }

                        default:
                            HttpHelper.ResponseStatus = 500;
                            return new Token("Shiro's built-in REST server can't handle method: " + restMethod.ToUpper());
                    }
                #endregion

                #endregion

                #region Telnet/TCP

                case "telnet":
					if (Server.Serving)
						Error("Can't start a telnet server while already serving something");
					if (!list.ValidateParamCount(2) && !list.ValidateParamCount(3))
                        Error("Wrong number of parameters to keyword 'telnet', expected 2 or 3");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (!toke.IsNumeric)
                        Error("First argument to telnet command must be a port number");

                    l = (long)toke.Toke;   //l is now port number
                    toke = list[2];
                    if (!toke.IsParent)
                        toke = toke.Eval(this, atomic, skipRootObjectLambdas);

                    if (list.Count == 4)
                    {
                        var conToke = list[3];
                        if (!conToke.IsParent)
                            conToke = conToke.Eval(this, atomic, skipRootObjectLambdas);

                        Server.ListenForTelnetOrTcp(this, toke, (int)l, conToke);
                    }
                    else
                        Server.ListenForTelnetOrTcp(this, toke, (int)l);

                    i = 100;        //1 second total retry time
                    while (null == (toke = Server.Result) && i-- > 0)
                        Thread.Sleep(10);

                    if (toke == null)
                        Error("There was a problem getting the result of the last server run");

                    Server.Result = null;
                    return toke;

                case "tcp":
                    if (Server.Serving)
                        Error("Can't start a tcp server while already serving something");
                    if (!list.ValidateParamCount(2) && !list.ValidateParamCount(3))
                        Error("Wrong number of parameters to keyword 'tcp', expected 2 or 3");
                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);
                    if (!toke.IsNumeric)
                        Error("First argument to tcp command must be a port number");

                    l = (long)toke.Toke;   //l is now port number
                    toke = list[2];
                    if (!toke.IsParent)
                        toke = toke.Eval(this, atomic, skipRootObjectLambdas);

                    if (list.Count == 4)
                    {
                        var conToke = list[3];
                        if (!conToke.IsParent)
                            conToke = conToke.Eval(this, atomic, skipRootObjectLambdas);

                        Server.ListenForTelnetOrTcp(this, toke, (int)l, conToke, true);
                    }
                    else
                        Server.ListenForTelnetOrTcp(this, toke, (int)l, null, true);

                    i = 100;        //1 second total retry time
                    while (null == (toke = Server.Result) && i-- > 0)
                        Thread.Sleep(10);

                    if (toke == null)
                        Error("There was a problem getting the result of the last server run");

                    Server.Result = null;
                    return toke;

                case "send":
                    if (!Server.Serving)
                        Error("Cannot use 'send' keyword when we are not acting as a network server");
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'send', expected 1");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    Server.SendTo(Guid.Parse(Symbols.Get(Symbols.AutoVars.ConnectionId).Toke.ToString()), s1);
                    return new Token(s1);

                case "sendall":
                    if (!Server.Serving)
                        Error("Cannot use 'sendAll' keyword when we are not acting as a network server");
                    if (Server.ConType != ConnectionType.MUD && Server.ConType != ConnectionType.TCP)
                        Error("The 'sendAll' keyword can only be used in a telnet or TCP server context");
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'sendAll', expected 1");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    Server.SendToAll(s1);
                    return new Token(s1);

                case "sendto":
                    if (!Server.Serving)
                        Error("Cannot use 'sendTo' keyword when we are not acting as a network server");
                    if (Server.ConType != ConnectionType.MUD && Server.ConType != ConnectionType.TCP)
                        Error("The 'sendTo' keyword can only be used in a telnet or tcp server context");
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'sendTo', expected 2");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    s2 = list[2].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    try
                    {
                        Server.SendTo(Guid.Parse(s1), s2);
                    }
                    catch (Exception)
                    {
                        Error($"Invalid connection id '{s1}' passed to sendTo");
                    }
                    return new Token(s1);
                #endregion

                #region Async / Threads

                case "await":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'await', expected 2");

                    s1 = list[1].ToString();
                    toke = list[2];
                    
                    if (!toke.IsParent)
                        Error("await's second parameter must be a list, not " + toke.ToString());

                    var newSym = new Symbols(null);
                    newSym.CloneFrom(Symbols);

                    Symbols.BeginAwaiting(s1);

                    var tst = new ParameterizedThreadStart((sym) =>
                    {
                        Token res;
                        try
                        {
                            using (var threadedInterpreter = new Interpreter((Symbols)sym)) { 
                                res = threadedInterpreter.Eval(toke.Children);
                                Symbols.Deliver(s1, res);
                            }
                        }
                        catch (Exception ex)
                        {
                            Symbols.Deliver(s1, Token.Error(this, ex.Message));
                        }

                    });

                    new Thread(tst).Start(newSym);
                    return Token.Nil;

                case "awaith":
                case "hermeticawait":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'awaith', expected 2");

                    s1 = list[1].ToString();
                    toke = list[2];

                    if (!toke.IsParent)
                        Error("awaith's second parameter must be a list, not " + toke.ToString());

                    Symbols.BeginAwaiting(s1);

                    var tst2 = new ThreadStart(() =>
                    {
                        Token res;
                        try
                        {
                            using (var threadedInterpreter = new Interpreter())
                            {
                                res = threadedInterpreter.Eval(toke.Children);
                                Symbols.Deliver(s1, res);
                            }
                        }
                        catch (Exception ex)
                        {
                            if(ex is ShiroException)
                                Symbols.Deliver(s1, Token.Error(this, (ex as ShiroException).Exception.ToString()));
                            else
                                Symbols.Deliver(s1, Token.Error(this, ex.Message));
                        }
                    });

                    new Thread(tst2).Start();
                    return Token.Nil;

                case "pub":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'pub', expected 2");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    toke = list[2].Eval(this, atomic, skipRootObjectLambdas);

                    Conduit.Publish(s1, toke);
                    return toke;

                case "sub":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of parameters to keyword 'sub', expected 2");

                    s1 = list[1].Eval(this, atomic, skipRootObjectLambdas).ToString();
                    toke = list[2];

                    MySubscriptions.Add(Conduit.Subscribe(s1, this, toke), s1);
                    return toke;

                case "atom":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'atom', expected 1");

                    _atomicEvaluation = true;
                    try
                    {
                        toke = list[1].Eval(this, true, skipRootObjectLambdas);
                    }
                    catch (Exception)
                    {
                        _atomicEvaluation = false;
                        throw;
                    }
                    finally
                    {
                        _atomicEvaluation = false;
                    }
                    return toke;

                #endregion

                #region Misc

                case "len":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of parameters to keyword 'len', expected 1");

                    toke = list[1].Eval(this, atomic, skipRootObjectLambdas);

                    if (toke.IsParent)
                        return new Token(toke.Children.Count.ToString());
                    return new Token(toke.ToString().Length.ToString());

                case "nop":
                    DispatchPublications();
                    if (list.Count > 1)
                        return list[1];
                    return Token.Nil;

				case "import":
					if (!list.ValidateParamCount(1))
						Error("Wrong number of parameters to keyword 'import', expected 1");
					if(!Loader.LoadDLL(this, list[1].ToString()))
                    {
                        if (!LoadModule(this, list[1].ToString()))
                            Error("Could not import module '" + list[1].ToString() + "', could not find either a DLL or a matching Shiro file");
                    }
					return Token.Nil;

                #endregion

                default:
                    if (list[0].IsFunction)
                    {
                        var retVal = list[0].EvalLambda(_bestGuessAtThisForLambda, this, list.Quote().ToArray());
                        _bestGuessAtThisForLambda = null;
                        return retVal;
                    }
                    else if (Symbols.FuncExists(s1 = list[0]?.Toke?.ToString()))
                        return Symbols.CallFunc(s1, this, list.Quote().ToArray());
                    else if (Symbols.CanGet(s1))
                    {
                        var hopefulLambda = Symbols.Get(s1);
                        if (hopefulLambda.IsFunction)
                            return hopefulLambda.EvalLambda(null, this, list.Quote().ToArray());
                        else
                            return hopefulLambda;
                    }
                    else if(s1.EndsWith("?"))
                    {
                        //Is it an implementer auto-predicate?
                        s1 = s1.TrimEnd('?');
                        if(Symbols.CanGetImplementer(s1) && list.Count == 2)
                        {
                            return new Token(new Token("impl?"), list[1], new Token(s1)).Eval(this, atomic, skipRootObjectLambdas);
                        }
                        else
                        {
                            if (list.Count != 2)
                                Error("Wrong number of paramters passed to auto-predicate for implementer " + s1);
                            else
                                Error("Could not find implementer for auto-predicate: " + s1);
                        }
                    }
                    else
                        Error("Unknown keyword/function: " + list[0].Toke);
                    break;
            }

            return Token.Nil;
        }

        protected Token InnerEval(string code)
        {
            var scanned = Scan(code);
            var retVal = scanned.Eval(this);
            return retVal;
        }
        public Token Eval(string code, bool cleanUpAfter = true)
        {
            var retVal = InnerEval(code);
            DispatchPublications();
            if(cleanUpAfter)
                CleanUpQueues();
            return retVal;
        }
    }
}
