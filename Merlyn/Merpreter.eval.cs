using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Merlyn.Guts;
using Merlyn.Interop;
using Merlyn.Nimue;

namespace Merlyn
{
    public partial class Merpreter
    {
        public Token Eval(List<Token> list)
        {
            if (list == null || list.Count == 0)
                return Token.Nil;

            //Inline objects have an "implicit quote"
            if (!string.IsNullOrEmpty(list[0].Name))
                return new Token(list);

            if (list[0].IsParent)
            {
                var evalled = list[0].Eval(this);
                if (list.Count > 1 && !evalled.IsFunction)
                    Error("Sibling peered list passed for evaluation -- you are probably missing a 'do' keyword");
                else if (list.Count == 1)
                    return evalled;
                else
                    list[0] = evalled;
            }

            int i, j, k;
            long l;
            Token lastVal = Token.Nil, val = Token.Nil;
            string s1 = "", s2 = "", name = "";
            List<string> ls = new List<string>();
            List<Token> ts = new List<Token>();
            Token toke = null, request = null;

            switch (list[0].Toke?.ToString()?.ToLower())
            {
                #region IO and string manipulation

                case "print":
                    for (i = 1; i < list.Count; i++)
                        Output((lastVal = list[i].Eval(this)).ToString());
                    return lastVal;

                case "string":
                case "str":
                    s1 = "";
                    for (i = 1; i < list.Count; i++)
                        s1 += list[i].Eval(this).ToString();
                    return new Token(s1);

                #endregion

                #region Web service stuff and serialization

                case "json":
                    if(!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'json', expected 1");
                    toke = list[1].Eval(this);
                    return new Token(toke.ToJSON(this));

                case "jsonv":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'jsonv', expected 1");
                    toke = list[1].Eval(this);
                    return new Token(toke.ToJSON(this, true));

                case "dejson":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'dejson', expected 1");
                    toke = ScanInlineObject(list[1].Eval(this).ToString(), true);
                    return toke;
                #endregion

                #region Tree/list manipulation

                case "quote":
                    return new Token(list.Quote());

                case "eval":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'eval', expected 1");
                    return list[1].Eval(this).Eval(this);

                case "skw":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword 'skw', expected 2");
                    lastVal = new Token(list[1].Eval(this));
                    if(list[2].IsParent)
                        lastVal.Children.AddRange(list[2].Eval(this).Children);
                    else
                        lastVal.Children.Add(list[2]);
                    return lastVal;

                case "1st":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword '1st', expected 1");
                    lastVal = list[1].Eval(this);
                    if (lastVal.IsParent)
                        return lastVal.Children[0];
                    return lastVal;
                case "rest":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'rest', expected 1");
                    lastVal = list[1].Eval(this);
                    if (lastVal.IsParent)
                        return new Token(lastVal.Children.Quote());
                    return Token.Nil;

				case "nth":
					if (!list.ValidateParamCount(2))
						Error("Wrong number of arguments to keyword 'nth', expected 2");
					toke = list[1].Eval(this);
					if (!toke.IsNumeric || toke.IsDecimal)
						Error("The first parameter to 'nth' must be a number");
					l = (long)toke.Toke;

					lastVal = list[2].Eval(this);
					if (lastVal.IsParent) {
						if (lastVal.Children.Count > l)
							return lastVal.Children[(int)(l - 1)];
						return Token.Nil;
					}
					return lastVal;

				case "range":
					if (!list.ValidateParamCount(3))
						Error("Wrong number of arguments to keyword 'range', expected 3");

					toke = list[1].Eval(this);
					if (!toke.IsNumeric || toke.IsDecimal)
						Error("The first parameter to 'range' must be a number");
					j = (int)((long)toke.Toke);

					toke = list[2].Eval(this);
					if (!toke.IsNumeric || toke.IsDecimal)
						Error("The second parameter to 'range' must be a number");
					k = (int)((long)toke.Toke);

					if (j < 1)
						Error("First parameter to 'range' cannot be less than 1");

					lastVal = list[3].Eval(this);
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
                        toke = list[i].Eval(this);
                        if(toke.IsParent)
                            ts.AddRange(toke.Children);
                        else
                            ts.Add(toke);
                    }
                    return new Token(ts);

                case "filter":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword 'filter', expected 2");
                    toke = list[2].Eval(this);

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

                case "map":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword 'map', expected 2");
                    toke = list[2].Eval(this);

                    if (!toke.IsParent)
                        Error("Map can only operate on a list");

                    var mappedItems = new List<Token>();
                    mappedItems.Add(new Token("quote"));
                    foreach (var t in toke.Children)
                    {
                        ts.Clear();
                        ts.Add(list[1]);
                        ts.Add(t);

                        mappedItems.Add(Eval(ts));
                    }
                    return new Token(mappedItems.ToArray());

                #endregion

                #region "Object" manipulation

                case ".":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of arguments to keyword '.', expected at least 2");

                    toke = list[1].Eval(this);

                    for (i = 2; i < list.Count; i++)
                    {
                        var t2 = list[i].Eval(this);
                        if (t2.IsParent)
                            t2 = t2.Eval(this);
                        s1 = t2.ToString();

                        if (toke.Children == null || !toke.Children.HasProperty(s1))
                            Error($"Cannot dereference property {s1} off {toke.ToString()}");
                        else
                            toke = toke.Children.GetProperty(s1);
                    }
                    return toke;

                case ".s":
                case ".set":
                    if (!list.ValidateParamCount(3, true))
                        Error("Wrong number of arguments to keyword '.set', expected at least 3");

                    if (list[1].IsParent)
                        Error("First argument to '.set' must be a name, not simply a list or inline object");

                    name = list[1].Toke.ToString();
                    if (!Symbols.CanGet(name))
                        Error("Can't find variable " + name + " for .set command");

                    toke = Symbols.Get(name);

                    for (i = 2; i < list.Count-1; i++)
                    {
                        var t2 = list[i].Eval(this);
                        if (t2.IsParent)
                            t2 = t2.Eval(this);
                        s1 = t2.ToString();

                        if (toke.Children == null || !toke.Children.HasProperty(s1))
                            Error($"Cannot dereference property {s1} off {toke.ToString()}");
                        else
                        {
                            if(i < list.Count - 2)
                                toke = toke.Children.GetProperty(s1);
                            //s1 will hold the name of the property we're working with
                        }
                    }

                    val = list[list.Count - 1].Eval(this);
                    if (!toke.Children.SetProperty(s1, val))
                        Error("Could not find property " + name + " to set using .set.  Consider concat/pairing it, or using .sod instead.");

                    return Symbols.Get(name);

                case ".sod":
                    if (!list.ValidateParamCount(3, true))
                        Error("Wrong number of arguments to keyword '.set', expected at least 3");

                    if (list[1].IsParent)
                        Error("First argument to '.set' must be a name, not simply a list or inline object");

                    name = list[1].Toke.ToString();
                    if (!Symbols.CanGet(name))
                        Error("Can't find variable " + name + " for .set command");

                    toke = Symbols.Get(name);

                    for (i = 2; i < list.Count - 1; i++)
                    {
                        var t2 = list[i].Eval(this);
                        if (t2.IsParent)
                            t2 = t2.Eval(this);
                        s1 = t2.ToString();

                        if (i < list.Count - 2)
                        {
                            if (toke.Children == null || !toke.Children.HasProperty(s1))
                                Error($"Cannot dereference property {s1} off {toke.ToString()}");
                            else
                            {
                                toke = toke.Children.GetProperty(s1);
                            }
                        }
                    }

                    val = list[list.Count - 1].Eval(this);
                    if (!toke.Children.SetProperty(s1, val))
                        toke.Children.AddProperty(s1, val);

                    return Symbols.Get(name);

                case ".?":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of arguments to keyword '.?', expected at least 2");

                    toke = list[1].Eval(this);

                    for (i = 2; i < list.Count; i++)
                    {
                        var t2 = list[i].Eval(this);
                        if (t2.IsParent)
                            t2 = t2.Eval(this);
                        s1 = t2.ToString();

                        if (toke.Children == null || !toke.Children.HasProperty(s1))
                            return Token.Nil;
                        else
                            toke = toke.Children.GetProperty(s1);
                    }
                    return toke;

                case "pair":
                    if(!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword 'pair', expected at least 2");

                    s1 = list[1].Eval(this).ToString();
                    toke = list[2].Eval(this);
                    if (toke.IsParent)
                        return new Token(new Token(s1, toke.Children));
                    else
                        return new Token(new Token(s1, toke.Toke));
                #endregion

                #region Variables

                case "def":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword 'def', expected 2");
                    if(list[1].IsParent)
                        Error("First argument to 'def' must be a name, not any kind of list");
                    s1 = list[1].Toke.ToString();
                    if (Symbols.CanGet(s1))
                        Error($"Cannot def '{s1}', variable already defined");
                    Symbols.Set(s1, lastVal = list[2].Eval(this));
                    return lastVal;
                case "set":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword 'set', expected 2");
                    if (list[1].IsParent)
                        Error("First argument to 'set' must be a name, not any kind of list");
                    s1 = list[1].Toke.ToString();
                    if(!Symbols.CanGet(s1))
                        Error($"Cannot set '{s1}', variable not defined yet");
                    Symbols.Set(s1, lastVal = list[2].Eval(this));
                    return lastVal;
                case "sod":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword 'sod', expected 2");
                    if (list[1].IsParent)
                        Error("First argument to 'sod' must be a name, not any kind of list");
                    s1 = list[1].Toke.ToString();
                    Symbols.Set(s1, lastVal = list[2].Eval(this));
                    return lastVal;

                case "v":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'v', expected 1");
                    if (Symbols.CanGet(s1 = list[1].Toke.ToString()))
                        return Symbols.Get(s1);
                    else
                        Error("Variable not found: " + s1);
                    return Token.Nil;

                case "let":
                    if (!list.ValidateParamCount(1, true))
                        Error("Wrong number of arguments to keyword 'let', expected at least 2");
                    if (!list[1].IsParent)
                        Error("Let must include at least one parameter in the let-list");
                    if (list[1].Children.Count != 0 && list[1].Children.Count % 2 != 0)
                        Error("The let-list must have an even number of parameters");

                    Guid letId = Guid.NewGuid();
                    for (i = 0; i < list[1].Children.Count; i += 2)
                    {
                        s1 = list[1].Children[i].Toke.ToString();
                        toke = list[1].Children[i + 1].Eval(this);

                        Symbols.Let(s1, toke, letId);
                    }

                    for (i = 2; i < list.Count; i++)
                        lastVal = list[i].Eval(this);

                    Symbols.ClearLetId(letId);
                    return lastVal;

                #endregion

                #region Math and CondOps

                case "+":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of arguments to keyword '+', expected at least 2");
                    return MathHelper.Add(list.Quote().Select(t => t.Eval(this)).ToArray());
                case "-":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of arguments to keyword '-', expected at least 2");
                    return MathHelper.Subtract(list.Quote().Select(t => t.Eval(this)).ToArray());

                case "*":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of arguments to keyword '*', expected at least 2");
                    return MathHelper.Multiply(list.Quote().Select(t => t.Eval(this)).ToArray());
                case "/":
                    if (!list.ValidateParamCount(2, true))
                        Error("Wrong number of arguments to keyword '/', expected at least 2");
                    return MathHelper.Divide(list.Quote().Select(t => t.Eval(this)).ToArray());

                case "=":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword '=', expected 2");
                    return MathHelper.Equals(list[1].Eval(this), list[2].Eval(this));

                case "!":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword '!', expected 1");
                    return MathHelper.Not(list[1].Eval(this));

                case "!=":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword '!=', expected 2");
                    return MathHelper.Not(MathHelper.Equals(list[1].Eval(this), list[2].Eval(this)));

                case ">":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword '>', expected 2");
                    return MathHelper.GreaterThan(list[1].Eval(this), list[2].Eval(this));
                case "<":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword '<', expected 2");
                    return MathHelper.LessThan(list[1].Eval(this), list[2].Eval(this));

                case ">=":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword '>=', expected 2");
                    return MathHelper.Not(MathHelper.LessThan(list[1].Eval(this), list[2].Eval(this)));
                case "<=":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword '<=', expected 2");
                    return MathHelper.Not(MathHelper.GreaterThan(list[1].Eval(this), list[2].Eval(this)));

                #endregion

                #region Conditional Elements

                case "list?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'list?', expected 1");
                    toke = list[1].Eval(this);
                    if (toke.Children != null)
                        return Token.True;
                    return Token.False;

                case "fn?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'fn?', expected 1");
                    toke = list[1].Eval(this);
                    s1 = toke.ToString();
                    if (Symbols.FuncExists(s1))
                        return Token.True;
                    return Token.False;

                case "nil?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'nil?', expected 1");
                    toke = list[1].Eval(this);
                    if(toke.IsNil)
                        return Token.True;
                    return Token.False;

                case "obj?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'obj?', expected 1");
                    toke = list[1].Eval(this);
                    if (toke.Children != null && toke.Children.Any(t => !string.IsNullOrEmpty(t.Name)))
                        return Token.True;
                    return Token.False;

                case "num?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'num?', expected 1");
                    toke = list[1].Eval(this);
                    if (toke.IsNumeric)
                        return Token.True;
                    return Token.False;

                case "str?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'str?', expected 1");
                    toke = list[1].Eval(this);
                    if (!toke.IsParent && !toke.IsNumeric)
                        return Token.True;
                    return Token.False;

                case "def?":
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'def?', expected 1");
                    toke = list[1].Eval(this);
                    s1 = toke.ToString();
                    if (Symbols.CanGet(s1))
                        return Token.True;
                    if (Symbols.FuncExists(s1))
                        return Token.True;
                    return Token.False;

                #endregion

                #region Functions and Lambdas

                case "defn":
                    if (!list.ValidateParamCount(3))
                        Error("Wrong number of arguments to keyword 'defn', expected 3");
                    s1 = list[1].Toke.ToString();
                    toke = new Token();
                    toke.Params = new List<string>();

                    if (!list[2].IsParent)
                        Error("Parameter list for defn must be a list (even if empty or containing only one element)");

                    foreach (var p in list[2].Children)
                    {
                        if (p.IsParent || p.IsNumeric)
                            Error("List or numeric value passed to parameter list for defn");
                        else
                            toke.Params.Add(p.Toke.ToString());
                    }

                    if (!list[3].IsParent)
                        Error("Function body must be a list");

                    toke.Children.AddRange(list[3].Children);
                    Symbols.AddFunc(s1, toke);
                    return Token.Nil;

                case "=>":
                case "lambda":
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword '=>', expected 2");
                    if (!list[1].IsParent)
                        Error("First param to => must be the parameter list of the lambda");
                    if (!list[2].IsParent)
                        Error("Second param to => must be the body of the lambda");

                    Token lambda = new Token(list[2].Children);
                    lambda.Params = list[1].Children.Select(e => {
                        if (e.IsParent)
                            Error("Parameter list in lambda can't have lists in it");

                        return e.ToString();
                    }).ToList();
                    return lambda;

                #endregion

                #region Control Commands

                case "if":
                    if (!list.ValidateParamCount(2) && !list.ValidateParamCount(3))
                        Error("Wrong number of arguments to keyword 'if', expected 2 or 3");
                    bool hasElse = list.Count == 4;
                    toke = list[1].Eval(this);
                    if (MathHelper.Not(toke).Toke == Token.False.Toke)
                        return list[2].Eval(this);
                    else
                    {
                        if(hasElse)
                            return list[3].Eval(this);

                        return Token.Nil;
                    }

				case "do":
					for(i =1; i<list.Count;i++)
					{
						lastVal = list[i].Eval(this);
					}
					return lastVal;

                #endregion

                #region Network

                //Global
                case "stop":
                    if (!Server.Serving)
                        Error("Cannot use 'stop' keyword when we are not acting as a network server");
                    if (!list.ValidateParamCount(0) && !list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'stop', expected 0 or 1");

                    Server.Serving = false;
                    Thread.Sleep(150);

                    if (list.Count == 2)
                        Server.Result = list[1].Eval(this);
                    else
                        Server.Result = Token.Nil;
                    return Server.Result;

                //HTTP
                case "http":
					if (Server.Serving)
						Error("Can't start an HTTP server while already serving something");
					if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword 'http', expected 2");
                    toke = list[1].Eval(this);
                    if (!toke.IsNumeric)
                        Error("First argument to http command must be a port number");

                    l = (long)toke.Toke;   //l is now port number
                    toke = list[2];
                    if (!toke.IsParent)
                        toke = toke.Eval(this);

                    Server.ListenForHttp(this, toke, (int)l);

                    i = 100;        //1 second total retry time
                    while (null == (toke = Server.Result) && i-- > 0)
                        Thread.Sleep(10);

                    if (toke == null)
                        Error("There was a problem getting the result of the last server run");

                    Server.Result = null;
                    return toke;

				case "content":
					if(!list.ValidateParamCount(2))
						Error("Wrong number of arguments to keyword 'content', expected 2");
					if (!Server.Serving || Server.ConType != ConnectionType.HTTP)
						Error("Cannot use 'content' keyword if we are not currently in an http server context");
					s1 = list[1].Eval(this).Toke.ToString();

					HttpHelper.ContentType = s1;
					return list[2].Eval(this);
				case "status":
					if (!list.ValidateParamCount(2))
						Error("Wrong number of arguments to keyword 'status', expected 2");
					if (!Server.Serving || Server.ConType != ConnectionType.HTTP)
						Error("Cannot use 'status' keyword if we are not currently in an http server context");

					toke = list[1].Eval(this);
					if (!toke.IsNumeric || toke.IsDecimal)
						Error("First parameter to 'status' must be numeric");

					HttpHelper.ResponseStatus = (int)((long)toke.Toke);
					return list[2].Eval(this);

				case "route":
                    if ((list.Count - 1) % 2 != 0)
                        Error("Parameters to 'route' keyword must be paired");
                    if (!Server.Serving || Server.ConType != ConnectionType.HTTP)
                        Error("Cannot use 'route' keyword if we are not currently in an http server context");
                    request = Symbols.Get(Symbols.AutoVars.HttpRequest);
                    s1 = request.Children.GetProperty("url").Toke.ToString();
                    
                    for (i = 1; i < list.Count; i += 2)
                    {
                        s2 = list[i].Eval(this).Toke.ToString();
                        if (s1.ToLower().EndsWith(s2.ToLower()))
                            return list[i + 1].Eval(this);
                        if (s2.ToLower() == "default")
                            toke = list[i + 1];
                    }

                    if (toke != null)
                        return toke.Eval(this);
                    return Token.Nil;

				//Telnet
				case "telnet":
					if (Server.Serving)
						Error("Can't start a telnet server while already serving something");
					if (!list.ValidateParamCount(2) && !list.ValidateParamCount(3))
                        Error("Wrong number of arguments to keyword 'telnet', expected 2 or 3");
                    toke = list[1].Eval(this);
                    if (!toke.IsNumeric)
                        Error("First argument to telnet command must be a port number");

                    l = (long)toke.Toke;   //l is now port number
                    toke = list[2];
                    if (!toke.IsParent)
                        toke = toke.Eval(this);

                    if (list.Count == 4)
                    {
                        var conToke = list[3];
                        if (!conToke.IsParent)
                            conToke = conToke.Eval(this);

                        Server.ListenForTelnet(this, toke, (int)l, conToke);
                    }
                    else
                        Server.ListenForTelnet(this, toke, (int)l);

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
                        Error("Wrong number of arguments to keyword 'send', expected 1");

                    s1 = list[1].Eval(this).ToString();
                    Server.SendTo(Guid.Parse(Symbols.Get(Symbols.AutoVars.ConnectionId).Toke.ToString()), s1);
                    return new Token(s1);

                case "sendall":
                    if (!Server.Serving)
                        Error("Cannot use 'sendAll' keyword when we are not acting as a network server");
                    if (Server.ConType != ConnectionType.MUD)
                        Error("The 'sendAll' keyword can only be used in a telnet server context");
                    if (!list.ValidateParamCount(1))
                        Error("Wrong number of arguments to keyword 'sendAll', expected 1");

                    s1 = list[1].Eval(this).ToString();
                    Server.SendToAll(s1);
                    return new Token(s1);

                case "sendto":
                    if (!Server.Serving)
                        Error("Cannot use 'sendTo' keyword when we are not acting as a network server");
                    if (Server.ConType != ConnectionType.MUD)
                        Error("The 'sendTo' keyword can only be used in a telnet server context");
                    if (!list.ValidateParamCount(2))
                        Error("Wrong number of arguments to keyword 'sendTo', expected 2");

                    s1 = list[1].Eval(this).ToString();
                    s2 = list[2].Eval(this).ToString();
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

                #region Misc

                case "nop":
                    Output("No Op encountered");
                    return Token.Nil;

                case "qnop":
                    return Token.Nil;

				case "import":
					if (!list.ValidateParamCount(1))
						Error("Wrong number of arguments to keyword 'import', expected 1");
					Loader.LoadDLL(list[1].ToString());
					return Token.Nil;

                #endregion

                default:
                    if (list[0].IsFunction)
                        return list[0].EvalLambda(this, list.Quote().ToArray());
                    else if (Symbols.FuncExists(s1 = list[0].Toke.ToString()))
                        return Symbols.CallFunc(s1, this, list.Quote().ToArray());
                    else if(Symbols.CanGet(s1))
                    {
                        var hopefulLambda = Symbols.Get(s1);
                        if (hopefulLambda.IsFunction)
                            return hopefulLambda.EvalLambda(this, list.Quote().ToArray());
                        else
                            return hopefulLambda;
                    } else 
                        Error("Unknown keyword/function: " + list[0].Toke);
                    break;
            }

            return Token.Nil;
        }

        public Token Eval(string code)
        {
            var scanned = Scan(code);
            return scanned.Eval(this);
        }
    }
}
