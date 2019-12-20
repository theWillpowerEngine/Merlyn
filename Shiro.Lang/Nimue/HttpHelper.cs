using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Shiro.Nimue
{
    internal static class HttpHelper
    {
        enum RState
        {
            METHOD, URL, URLPARM, URLVALUE, VERSION,
            HEADERKEY, HEADERVALUE, BODY, OK
        }
        enum RespState
        {
            OK = 200,
            BAD_REQUEST = 400,
            NOT_FOUND = 404
        }

        internal struct HTTPRequest
        {
            public string Method;
            public string URL;
            public string Version;
            public Hashtable Args;
            public bool Execute;
            public Hashtable Headers;
            public int BodySize;
            public byte[] BodyData;
        }

        public struct HTTPResponse
        {
            public int status;
            public string version;
            public Hashtable Headers;
            public int BodySize;
            public byte[] BodyData;
        }

        private static string UrlDecode(string s)
        {
            return HttpUtility.UrlDecode(s);
        }

        internal static HTTPRequest ParseRequest(string rawData)
        {
            string hValue = "";
            string hKey = "";
            RState ParserState = RState.METHOD;

            HTTPRequest request = new HTTPRequest();

            int ndx = 0, bfndx = 0;
            do
            {
                switch (ParserState)
                {
                    case RState.METHOD:
                        if (rawData[ndx] != ' ')
                            request.Method += (char) rawData[ndx++];
                        else
                        {
                            ndx++;
                            ParserState = RState.URL;
                        }

                        break;
                    case RState.URL:
                        if (rawData[ndx] == '?')
                        {
                            ndx++;
                            hKey = "";
                            request.Execute = true;
                            request.Args = new Hashtable();
                            ParserState = RState.URLPARM;
                        }
                        else if (rawData[ndx] != ' ')
                            request.URL += (char) rawData[ndx++];
                        else
                        {
                            ndx++;
                            request.URL = UrlDecode(request.URL);
                            ParserState = RState.VERSION;
                        }

                        break;
                    case RState.URLPARM:
                        if (rawData[ndx] == '=')
                        {
                            ndx++;
                            hValue = "";
                            ParserState = RState.URLVALUE;
                        }
                        else if (rawData[ndx] == ' ')
                        {
                            ndx++;

                            request.URL = UrlDecode(request.URL);
                            ParserState = RState.VERSION;
                        }
                        else
                        {
                            hKey += (char) rawData[ndx++];
                        }

                        break;
                    case RState.URLVALUE:
                        if (rawData[ndx] == '&')
                        {
                            ndx++;
                            hKey = UrlDecode(hKey);
                            hValue = UrlDecode(hValue);
                            request.Args[hKey] = request.Args[hKey] != null
                                ? request.Args[hKey] + ", " + hValue
                                : hValue;
                            hKey = "";
                            ParserState = RState.URLPARM;
                        }
                        else if (rawData[ndx] == ' ')
                        {
                            ndx++;
                            hKey = UrlDecode(hKey);
                            hValue = UrlDecode(hValue);
                            request.Args[hKey] = request.Args[hKey] != null
                                ? request.Args[hKey] + ", " + hValue
                                : hValue;

                            request.URL = UrlDecode(request.URL);
                            ParserState = RState.VERSION;
                        }
                        else
                        {
                            hValue += (char) rawData[ndx++];
                        }

                        break;
                    case RState.VERSION:
                        if (rawData[ndx] == '\r')
                            ndx++;
                        else if (rawData[ndx] != '\n')
                            request.Version += (char) rawData[ndx++];
                        else
                        {
                            ndx++;
                            hKey = "";
                            request.Headers = new Hashtable();
                            ParserState = RState.HEADERKEY;
                        }

                        break;
                    case RState.HEADERKEY:
                        if (rawData[ndx] == '\r')
                            ndx++;
                        else if (rawData[ndx] == '\n')
                        {
                            ndx++;
                            if (request.Headers["Content-Length"] != null)
                            {
                                request.BodySize = Convert.ToInt32(request.Headers["Content-Length"]);
                                request.BodyData = new byte[request.BodySize];
                                ParserState = RState.BODY;
                            }
                            else
                                ParserState = RState.OK;

                        }
                        else if (rawData[ndx] == ':')
                            ndx++;
                        else if (rawData[ndx] != ' ')
                            hKey += (char) rawData[ndx++];
                        else
                        {
                            ndx++;
                            hValue = "";
                            ParserState = RState.HEADERVALUE;
                        }

                        break;
                    case RState.HEADERVALUE:
                        if (rawData[ndx] == '\r')
                            ndx++;
                        else if (rawData[ndx] != '\n')
                            hValue += (char) rawData[ndx++];
                        else
                        {
                            ndx++;
                            request.Headers.Add(hKey, hValue);
                            hKey = "";
                            ParserState = RState.HEADERKEY;
                        }

                        break;
                    case RState.BODY:
                        // Append to request BodyData
                        Array.Copy(rawData.ToCharArray(), ndx, request.BodyData, bfndx, rawData.Length - ndx);
                        bfndx += rawData.Length - ndx;
                        ndx = rawData.Length;
                        if (request.BodySize <= bfndx)
                        {
                            ParserState = RState.OK;
                        }

                        break;
                    //default:
                    //   ndx++;
                    //   break;

                }
            } while (ndx < rawData.Length);

            return request;
        }

		internal static string ContentType = null;
		internal static int? ResponseStatus = null;

		private static string GetResponseString(int status)
		{
			switch (status)
			{
				case 200:
					return "OK ";
				case 404:
					return "Not Found ";
				default:
					return "Unknown ";
			}
		}


		internal static string WrapInHttpResponse(string result)
        {
            var response = new HTTPResponse();
            var sb = new StringBuilder("");

            response.version = "HTTP/1.0";

			if (ResponseStatus.HasValue)
			{
				response.status = ResponseStatus.Value;
				ResponseStatus = null;
			} else
				response.status = (int)RespState.OK;

            response.Headers = new Hashtable();
            response.Headers.Add("Server", "Shiro/" + Interpreter.Version);
            response.Headers.Add("Date", DateTime.Now.ToString("r"));

			if(ContentType != null)
			{
				response.Headers.Add("Content-Type", ContentType);
				ContentType = null;
			}

            if (result == null)
                response.BodyData = null;
            else
                response.BodyData = Encoding.ASCII.GetBytes(result);

            string HeadersString = $"HTTP 1.0 {response.status} {GetResponseString(response.status)}\n";

            foreach (DictionaryEntry Header in response.Headers)
            {
                HeadersString += Header.Key + ": " + Header.Value + "\n";
            }

            HeadersString += "\n";

            // Send headers
            sb.Append(HeadersString);

            // Send body
            if (response.BodyData != null)
                sb.Append(Encoding.Default.GetString(response.BodyData));

            return sb.ToString();
        }

        internal static Token RequestToToken(HTTPRequest request)
        {
            Token retVal = new Token();
            retVal.Toke = null;
            retVal.Children = new List<Token>();

            retVal.Children.Add(new Token("url", request.URL));
            retVal.Children.Add(new Token("method", request.Method));

            if (request.Args != null && request.Args.Count > 0)
            {
                var tokes = new List<Token>();
                foreach (var arg in request.Args.Keys)
                    tokes.Add(new Token(arg.ToString(), (object)(request.Args[arg].ToString())));

                retVal.Children.Add(new Token("args", tokes));
            }
            else
            {
                retVal.Children.Add(new Token("args", Token.Nil));
            }

            if (request.BodyData != null)
                retVal.Children.Add(new Token("body", Encoding.Default.GetString(request.BodyData)));
            else
                retVal.Children.Add(new Token("body", ""));
            return retVal;
        }
    }
}
